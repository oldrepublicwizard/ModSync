// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using ModSync.Core;
using ModSync.Core.CLI;
using ModSync.Core.Parsing;
using ModSync.Core.Services;

namespace ModSync.Tests
{
    /// <summary>
    /// Builds K2 neocities round-trip TOMLs: golden download URLs + ingested NLP install instructions.
    /// </summary>
    internal static class K2FullGuideRoundTripHelper
    {
        internal static void WriteRoundTripToml(
            string fixturePath,
            string goldenTomlPath,
            string outputTomlPath,
            params string[] overlayInstructionModNames)
        {
            if (overlayInstructionModNames is null || overlayInstructionModNames.Length == 0)
            {
                throw new ArgumentException("At least one mod name is required for instruction overlay.", nameof(overlayInstructionModNames));
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "ModSync_K2RoundTrip_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            try
            {
                string mergedToml = Path.Combine(tempDir, "merged.toml");
                string ingestedToml = Path.Combine(tempDir, "ingested.toml");

                int mergeExit = ModBuildConverter.Run(new[]
                {
                    "merge",
                    "--existing", goldenTomlPath,
                    "--incoming", fixturePath,
                    "--use-existing-order",
                    "--prefer-existing-instructions",
                    "--prefer-existing-options",
                    "--prefer-existing-modlinks",
                    "-f", "toml",
                    "-o", mergedToml,
                    "--plaintext",
                });

                if (mergeExit != 0)
                {
                    throw new InvalidOperationException($"merge failed with exit code {mergeExit}");
                }

                int convertExit = ModBuildConverter.Run(new[]
                {
                    "convert",
                    "--input", fixturePath,
                    "-f", "toml",
                    "--parse-directions",
                    "-o", ingestedToml,
                    "--plaintext",
                });

                if (convertExit != 0)
                {
                    throw new InvalidOperationException($"convert --parse-directions failed with exit code {convertExit}");
                }

                List<ModComponent> merged = FileLoadingService.LoadFromFile(mergedToml).ToList();
                List<ModComponent> ingested = FileLoadingService.LoadFromFile(ingestedToml).ToList();

                OverlayIngestedInstructions(merged, ingested, overlayInstructionModNames);
                FileLoadingService.SaveToFile(merged, outputTomlPath);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, recursive: true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        internal static void OverlayIngestedInstructions(
            List<ModComponent> merged,
            IReadOnlyList<ModComponent> ingested,
            params string[] modNameFragments)
        {
            var ingestedByName = ingested
                .GroupBy(c => c.Name?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (ModComponent component in merged)
            {
                if (!modNameFragments.Any(fragment =>
                        component.Name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    continue;
                }

                if (!ingestedByName.TryGetValue(component.Name.Trim(), out ModComponent ingestedComponent))
                {
                    ingestedComponent = ingested.FirstOrDefault(candidate =>
                        string.Equals(candidate.Name, component.Name, StringComparison.OrdinalIgnoreCase)
                        || (!string.IsNullOrEmpty(candidate.Name)
                            && candidate.Name.IndexOf(component.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (!string.IsNullOrEmpty(component.Name)
                            && component.Name.IndexOf(candidate.Name, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                if (ingestedComponent is null)
                {
                    continue;
                }

                component.Instructions.Clear();
                foreach (Instruction instruction in ingestedComponent.Instructions)
                {
                    component.Instructions.Add(instruction);
                }

                component.Options.Clear();
                foreach (Option option in ingestedComponent.Options)
                {
                    component.Options.Add(option);
                }

                component.InstallationWarning = ingestedComponent.InstallationWarning;
                component.Dependencies.Clear();
                component.Restrictions.Clear();

                EnsureExtractInstructionsFromRegistry(component);
                RefineMoveInstructionsFromDirections(component);
            }
        }

        /// <summary>
        /// NLP Move-only drafts do not satisfy download auto-discover (which keys off Extract patterns).
        /// Prepend Extract steps for registry archives that match this component's name.
        /// </summary>
        private static void EnsureExtractInstructionsFromRegistry(ModComponent component)
        {
            if (component.ResourceRegistry is null || component.ResourceRegistry.Count == 0)
            {
                return;
            }

            if (component.Instructions.Any(i => i.Action == Instruction.ActionType.Extract))
            {
                return;
            }

            var existingExtractSources = new HashSet<string>(
                component.Instructions
                    .Where(i => i.Action == Instruction.ActionType.Extract)
                    .SelectMany(i => i.Source ?? Enumerable.Empty<string>()),
                StringComparer.OrdinalIgnoreCase);

            var archiveNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, ResourceMetadata> resource in component.ResourceRegistry)
            {
                foreach (string filename in resource.Value.Files.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(filename) && ShouldIncludeRegistryArchive(component.Name, filename))
                    {
                        archiveNames.Add(filename);
                    }
                }
            }

            int insertAt = 0;
            foreach (string archiveName in archiveNames)
            {
                string source = $"<<modDirectory>>/{archiveName}";
                if (existingExtractSources.Contains(source))
                {
                    continue;
                }

                component.Instructions.Insert(insertAt++, new Instruction
                {
                    Action = Instruction.ActionType.Extract,
                    Source = new List<string> { source },
                });
            }

            if (insertAt == 0
                && component.ResourceRegistry.Count > 0
                && string.Equals(component.InstallationMethod, "Loose-File", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(component.Name))
            {
                string fallbackArchive = component.Name.Trim() + ".zip";
                string source = $"<<modDirectory>>/{fallbackArchive}";
                if (!existingExtractSources.Contains(source))
                {
                    component.Instructions.Insert(0, new Instruction
                    {
                        Action = Instruction.ActionType.Extract,
                        Source = new List<string> { source },
                    });
                }
            }
        }

        private static void RefineMoveInstructionsFromDirections(ModComponent component)
        {
            Instruction move = component.Instructions.FirstOrDefault(i => i.Action == Instruction.ActionType.Move);
            if (move is null || string.IsNullOrWhiteSpace(component.Directions))
            {
                return;
            }

            Match fileMatch = Regex.Match(
                component.Directions,
                @"\b([\w\.\-]+\.(?:dlg|2da|tga|tpc|utc|uti|utm|utd|ute|uts|utw|ssf|bwm|mdl|mdx|txi|lip|lyt|vis|pth|ncs|gui))\b",
                RegexOptions.IgnoreCase);

            if (!fileMatch.Success)
            {
                return;
            }

            string fileName = fileMatch.Groups[1].Value;
            move.Source = DraftInstructionService.ExpandLooseFileMoveSources(move.Source, fileName);
        }

        private static bool ShouldIncludeRegistryArchive(string modName, string archiveFileName)
        {
            if (string.IsNullOrWhiteSpace(modName) || string.IsNullOrWhiteSpace(archiveFileName))
            {
                return false;
            }

            string stem = Path.GetFileNameWithoutExtension(archiveFileName);
            if (stem.IndexOf(modName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            string[] tokens = modName
                .Split(new[] { ' ', '-', '_', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(token => token.Length > 3)
                .ToArray();

            if (tokens.Length == 0)
            {
                return false;
            }

            int hits = tokens.Count(token => stem.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
            return hits >= Math.Min(2, tokens.Length);
        }
    }
}
