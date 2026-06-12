// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.FileSystem;

namespace ModSync.Core.Services.Conflicts
{
    /// <summary>
    /// Detects file-level conflicts between selected components by simulating the install with the
    /// dry-run <see cref="VirtualFileSystemProvider"/> and attributing every game-directory write to
    /// the component (and instruction) that performed it. A path written by two or more distinct
    /// components is a conflict; the last writer in install order wins, matching real install behavior.
    /// </summary>
    /// <remarks>
    /// Analysis is restricted to writes under <see cref="MainConfig.DestinationPath"/> (the game
    /// directory). Writes into the mod workspace (for example archive extraction staging under
    /// <c>&lt;&lt;modDirectory&gt;&gt;</c>) are intermediate state, not user-facing conflicts.
    /// Path comparison is case-insensitive, matching the VFS and KOTOR's pathing expectations.
    /// </remarks>
    public sealed class FileConflictAnalyzer
    {
        /// <summary>
        /// Simulates the selected components in the given install order and reports every
        /// game-directory destination path written by more than one component.
        /// Requires <see cref="MainConfig.DestinationPath"/> to be set (mirrors how
        /// <c>DryRunValidator</c> sources its directories from <see cref="MainConfig"/>).
        /// </summary>
        /// <param name="componentsInInstallOrder">All components in install order; only components with <see cref="ModComponent.IsSelected"/> are simulated.</param>
        /// <param name="cancellationToken">Cancellation token, checked between components and instructions.</param>
        [NotNull]
        public async Task<ConflictAnalysisResult> AnalyzeAsync(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> componentsInInstallOrder,
            CancellationToken cancellationToken = default)
        {
            if (componentsInInstallOrder is null)
            {
                throw new ArgumentNullException(nameof(componentsInInstallOrder));
            }

            cancellationToken.ThrowIfCancellationRequested();

            List<ModComponent> selectedComponents = componentsInInstallOrder
                .Where(component => component?.IsSelected == true)
                .ToList();

            string gameRoot = MainConfig.DestinationPath?.FullName;
            if (selectedComponents.Count == 0 || string.IsNullOrEmpty(gameRoot))
            {
                return new ConflictAnalysisResult(
                    Array.Empty<FileConflict>(),
                    new Dictionary<Guid, int>(),
                    analyzedComponentCount: 0);
            }

            string normalizedGameRoot = Path.GetFullPath(gameRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string gameRootWithSeparator = normalizedGameRoot + Path.DirectorySeparatorChar;

            var vfs = new VirtualFileSystemProvider();
            if (MainConfig.SourcePath != null && MainConfig.SourcePath.Exists)
            {
                await vfs.InitializeFromRealFileSystemForComponentsAsync(MainConfig.SourcePath.FullName, selectedComponents).ConfigureAwait(false);
            }

            if (MainConfig.DestinationPath != null && MainConfig.DestinationPath.Exists)
            {
                await vfs.InitializeFromRealFileSystemForComponentsAsync(MainConfig.DestinationPath.FullName, selectedComponents).ConfigureAwait(false);
            }

            // Keyed by normalized absolute destination path, case-insensitive.
            var writeHistories = new Dictionary<string, PathWriteHistory>(StringComparer.OrdinalIgnoreCase);
            int analyzedComponentCount = 0;

            ModComponent currentComponent = null;
            int currentInstructionIndex = -1;

            void OnFileWritten(string writtenPath)
            {
                if (currentComponent is null || string.IsNullOrEmpty(writtenPath))
                {
                    return;
                }

                if (!writtenPath.StartsWith(gameRootWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!writeHistories.TryGetValue(writtenPath, out PathWriteHistory history))
                {
                    string relativePath = writtenPath.Substring(gameRootWithSeparator.Length)
                        .Replace(Path.DirectorySeparatorChar, '/')
                        .Replace('\\', '/');
                    history = new PathWriteHistory("<<kotorDirectory>>/" + relativePath);
                    writeHistories[writtenPath] = history;
                }

                history.RecordWrite(currentComponent, currentInstructionIndex);
            }

            vfs.FileWritten += OnFileWritten;
            try
            {
                foreach (ModComponent component in selectedComponents)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Mirror ExecuteInstructionsAsync's component-level dependency gating:
                    // components that would not install do not contribute writes.
                    if (!component.ShouldInstallComponent(selectedComponents))
                    {
                        continue;
                    }

                    currentComponent = component;
                    analyzedComponentCount++;

                    for (int instructionIndex = 0; instructionIndex < component.Instructions.Count; instructionIndex++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Instruction instruction = component.Instructions[instructionIndex];
                        instruction.SetFileSystemProvider(vfs);
                        instruction.SetParentComponent(component);
                        currentInstructionIndex = instructionIndex;

                        try
                        {
                            _ = await component.ExecuteSingleInstructionAsync(
                                instruction,
                                instructionIndex,
                                selectedComponents,
                                vfs,
                                skipDependencyCheck: false,
                                cancellationToken
                            ).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            await Logger.LogExceptionAsync(
                                ex,
                                $"Conflict analysis dry-run failed for '{component.Name}' instruction #{instructionIndex + 1} ({instruction.Action}); continuing."
                            ).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                vfs.FileWritten -= OnFileWritten;
                currentComponent = null;
            }

            var conflicts = new List<FileConflict>();
            var countsByComponent = new Dictionary<Guid, int>();
            foreach (PathWriteHistory history in writeHistories.Values)
            {
                if (history.Writers.Count < 2)
                {
                    continue;
                }

                IReadOnlyList<FileConflictWriter> writers = history.Writers
                    .Select(writer => new FileConflictWriter(writer.ComponentGuid, writer.ComponentName, writer.InstructionIndex))
                    .ToList();

                conflicts.Add(new FileConflict(
                    history.DisplayPath,
                    writers,
                    winnerComponentGuid: writers[writers.Count - 1].ComponentGuid));

                foreach (FileConflictWriter writer in writers)
                {
                    countsByComponent[writer.ComponentGuid] = countsByComponent.TryGetValue(writer.ComponentGuid, out int count)
                        ? count + 1
                        : 1;
                }
            }

            conflicts.Sort((left, right) => string.Compare(left.DestinationPath, right.DestinationPath, StringComparison.OrdinalIgnoreCase));

            return new ConflictAnalysisResult(conflicts, countsByComponent, analyzedComponentCount);
        }

        private sealed class PathWriteHistory
        {
            [NotNull]
            public string DisplayPath { get; }

            [NotNull]
            public List<WriterRecord> Writers { get; } = new List<WriterRecord>();

            public PathWriteHistory([NotNull] string displayPath)
            {
                DisplayPath = displayPath;
            }

            public void RecordWrite([NotNull] ModComponent component, int instructionIndex)
            {
                // Components execute sequentially, so repeat writes by the same component are
                // always adjacent: collapse them, keeping the most recent instruction index.
                if (Writers.Count > 0 && Writers[Writers.Count - 1].ComponentGuid == component.Guid)
                {
                    Writers[Writers.Count - 1] = new WriterRecord(component.Guid, component.Name, instructionIndex);
                    return;
                }

                Writers.Add(new WriterRecord(component.Guid, component.Name, instructionIndex));
            }
        }

        private readonly struct WriterRecord
        {
            public Guid ComponentGuid { get; }
            public string ComponentName { get; }
            public int InstructionIndex { get; }

            public WriterRecord(Guid componentGuid, string componentName, int instructionIndex)
            {
                ComponentGuid = componentGuid;
                ComponentName = componentName;
                InstructionIndex = instructionIndex;
            }
        }
    }
}
