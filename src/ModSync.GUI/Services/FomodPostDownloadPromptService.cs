// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.FileSystem;
using ModSync.Core.Services.Fomod;
using ModSync.Core.Utility;
using ModSync.Dialogs;

namespace ModSync.Services
{
    public static class FomodPostDownloadPromptService
    {
        public static async Task PromptForDetectedArchivesAsync(
            [NotNull] Window parentWindow,
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> components,
            [NotNull] string modDirectory)
        {
            if (parentWindow is null)
            {
                throw new ArgumentNullException(nameof(parentWindow));
            }

            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new ArgumentException("Mod directory cannot be null or whitespace.", nameof(modDirectory));
            }

            foreach (ModComponent component in components.Where(c => c.IsSelected))
            {
                foreach (string archivePath in GetDownloadedArchivePaths(component, modDirectory))
                {
                    string archiveFileName = Path.GetFileName(archivePath);
                    if (!FomodArchiveProbe.TryDetectInArchive(archivePath, out _))
                    {
                        continue;
                    }

                    if (!FomodDownloadPromptState.ShouldPrompt(component, archiveFileName))
                    {
                        continue;
                    }

                    bool? configure = await ConfirmationDialog.ShowConfirmationDialogAsync(
                        parentWindow,
                        $"A FOMOD installer was detected in '{archiveFileName}' for mod '{component.Name}'."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Configure installer options now?"
                        + Environment.NewLine
                        + Environment.NewLine
                        + "Choose No to skip for now. You can still use Mod Management → Configure FOMOD Mod later.");

                    if (configure != true)
                    {
                        FomodDownloadPromptState.MarkDismissed(component, archiveFileName);
                        continue;
                    }

                    string extractedDirectory = await ExtractArchiveAsync(archivePath, modDirectory).ConfigureAwait(true);
                    if (extractedDirectory is null)
                    {
                        await InformationDialog.ShowInformationDialogAsync(
                            parentWindow,
                            $"Failed to extract '{archiveFileName}' for FOMOD configuration.");
                        continue;
                    }

                    ModComponent configured = await FomodInstallerDialog.ShowForExtractedArchiveAsync(
                        parentWindow,
                        extractedDirectory).ConfigureAwait(true);

                    if (configured is null)
                    {
                        continue;
                    }

                    FomodConfiguredComponentMerger.MergeInto(component, configured);
                    FomodDownloadPromptState.MarkConfigured(component, archiveFileName);

                    await InformationDialog.ShowInformationDialogAsync(
                        parentWindow,
                        $"FOMOD configuration applied to '{component.Name}' from '{archiveFileName}'.");
                }
            }
        }

        [ItemNotNull]
        private static IEnumerable<string> GetDownloadedArchivePaths(
            [NotNull] ModComponent component,
            [NotNull] string modDirectory)
        {
            if (component.ResourceRegistry is null || component.ResourceRegistry.Count == 0)
            {
                yield break;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ResourceMetadata resource in component.ResourceRegistry.Values)
            {
                if (resource?.Files is null)
                {
                    continue;
                }

                foreach (string fileName in resource.Files.Keys)
                {
                    if (string.IsNullOrWhiteSpace(fileName) || !seen.Add(fileName))
                    {
                        continue;
                    }

                    string filePath = Path.Combine(modDirectory, fileName);
                    if (!File.Exists(filePath) || !ArchiveHelper.IsArchive(filePath))
                    {
                        continue;
                    }

                    yield return filePath;
                }
            }
        }

        [CanBeNull]
        private static async Task<string> ExtractArchiveAsync(
            [NotNull] string archivePath,
            [NotNull] string modDirectory)
        {
            string extractFolderName = Path.GetFileNameWithoutExtension(archivePath);
            string extractedDirectory = Path.Combine(modDirectory, extractFolderName);

            try
            {
                var fileSystemProvider = new RealFileSystemProvider();
                _ = await fileSystemProvider.ExtractArchiveAsync(archivePath, modDirectory).ConfigureAwait(false);

                if (FomodArchiveDiscovery.FindModuleConfigPath(extractedDirectory) != null)
                {
                    return extractedDirectory;
                }

                await Logger.LogWarningAsync(
                    $"[FomodPostDownload] Extracted '{archivePath}' but no fomod/ModuleConfig.xml found under '{extractedDirectory}'.");
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, $"[FomodPostDownload] Failed to extract '{archivePath}'");
            }

            return null;
        }
    }
}
