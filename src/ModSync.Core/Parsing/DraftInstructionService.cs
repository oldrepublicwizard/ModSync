// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace ModSync.Core.Parsing
{
    /// <summary>
    /// Result of drafting instructions for a single component from its natural-language prose.
    /// </summary>
    public sealed class DraftInstructionResult
    {
        [NotNull]
        public ModComponent Component { get; }

        public int DraftInstructionCount { get; }

        public DraftInstructionResult([NotNull] ModComponent component, int draftInstructionCount)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            DraftInstructionCount = draftInstructionCount;
        }
    }

    /// <summary>
    /// Wires <see cref="NaturalLanguageInstructionParser"/> into guide ingestion: converts a component's
    /// natural-language <see cref="ModComponent.Directions"/> prose into draft <see cref="Instruction"/> objects.
    /// Drafts are sandboxed (all paths placeholder-prefixed) and must always be flagged for user review by
    /// callers - they are never auto-trusted. Unparseable prose degrades gracefully to no drafts.
    /// </summary>
    public static class DraftInstructionService
    {
        /// <summary>
        /// Review-flag message callers should surface (e.g. as a serialized validation issue or a log warning)
        /// for every component that received draft instructions.
        /// </summary>
        [NotNull]
        public const string ReviewFlagMessage =
            "DRAFT INSTRUCTIONS: parsed from guide prose by the natural-language importer. Review before installing - never auto-trusted.";

        [NotNull] private const string ModDirectoryPlaceholder = "<<modDirectory>>";
        [NotNull] private const string KotorDirectoryPlaceholder = "<<kotorDirectory>>";
        [NotNull] private const string LegacyGameDirectoryPlaceholder = "<<gameDirectory>>";

        /// <summary>
        /// Matches KOTOR loose-file names mentioned in guide prose (e.g. 153sion.dlg).
        /// </summary>
        [NotNull]
        private static readonly Regex s_looseFileNamePattern = new Regex(
            @"\b([\w\.\-]+\.(?:dlg|2da|tga|tpc|utc|uti|utm|utd|ute|uts|utw|ssf|bwm|mdl|mdx|txi|lip|lyt|vis|pth|ncs|gui))\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Generates draft instructions for every component that has natural-language Directions prose
        /// but no authored instructions. Components that already have instructions are never touched.
        /// </summary>
        /// <returns>One result per component that received at least one draft instruction.</returns>
        [NotNull]
        [ItemNotNull]
        public static IReadOnlyList<DraftInstructionResult> GenerateDraftInstructions(
            [NotNull][ItemNotNull] IEnumerable<ModComponent> components,
            [CanBeNull] Action<string> logInfo = null,
            [CanBeNull] Action<string> logVerbose = null)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            Action<string> info = logInfo ?? (_ => { });
            Action<string> verbose = logVerbose ?? (_ => { });
            var parser = new NaturalLanguageInstructionParser(info, verbose);
            var results = new List<DraftInstructionResult>();

            foreach (ModComponent component in components)
            {
                if (component.Instructions.Count > 0)
                {
                    continue;
                }

                int added = 0;

                if (!string.IsNullOrWhiteSpace(component.Directions))
                {
                    ObservableCollection<Instruction> parsed;
                    try
                    {
                        parsed = parser.ParseInstructions(
                            component.Directions,
                            string.IsNullOrWhiteSpace(component.DownloadInstructions) ? null : component.DownloadInstructions,
                            component);
                    }
                    catch (Exception ex)
                    {
                        // Graceful degradation: unparseable prose may still get an InstallationMethod fallback.
                        verbose($"[DraftInstructions] Failed to parse prose for '{component.Name}': {ex.Message}");
                        parsed = new ObservableCollection<Instruction>();
                    }

                    foreach (Instruction instruction in parsed)
                    {
                        if (!TrySanitizeInstruction(instruction))
                        {
                            verbose($"[DraftInstructions] Dropped non-sandboxed draft ({instruction.Action}) for '{component.Name}'");
                            continue;
                        }

                        instruction.SetParentComponent(component);
                        component.Instructions.Add(instruction);
                        added++;
                    }
                }

                // K2 Full and similar guides often describe TSLPatcher/HoloPatcher installs via
                // Installation Method alone, or preference prose ("recommend the X option") that yields
                // no regex hits. Still draft a sandboxed Patcher (or loose-file Move) for review.
                if (added == 0)
                {
                    Instruction fallback = TryCreateMethodFallbackInstruction(component);
                    if (fallback != null && TrySanitizeInstruction(fallback))
                    {
                        fallback.SetParentComponent(component);
                        component.Instructions.Add(fallback);
                        added++;
                        verbose($"[DraftInstructions] Applied InstallationMethod fallback ({fallback.Action}) for '{component.Name}'");
                    }
                }

                if (added > 0)
                {
                    ApplyReviewFlag(component);
                    info($"[DraftInstructions] Drafted {added} instruction(s) from prose for '{component.Name}' - flagged for review.");
                    results.Add(new DraftInstructionResult(component, added));
                }
            }

            return results;
        }

        /// <summary>
        /// When prose does not parse into instructions, invent a minimal sandboxed draft from
        /// <see cref="ModComponent.InstallationMethod"/> and/or patcher keywords in Directions.
        /// </summary>
        [CanBeNull]
        private static Instruction TryCreateMethodFallbackInstruction([NotNull] ModComponent component)
        {
            string method = component.InstallationMethod ?? string.Empty;
            string directions = component.Directions ?? string.Empty;
            string combined = method + " " + directions;

            if (LooksLikePatcherInstall(combined))
            {
                return new Instruction
                {
                    Action = Instruction.ActionType.Patcher,
                    Source = new List<string> { ModDirectoryPlaceholder },
                    Destination = KotorDirectoryPlaceholder,
                    Overwrite = true,
                };
            }

            if (LooksLikeLooseFileInstall(method)
                && !LooksLikePatcherInstall(directions)
                && method.IndexOf("executable", StringComparison.OrdinalIgnoreCase) < 0)
            {
                string destination = directions.IndexOf("movies", StringComparison.OrdinalIgnoreCase) >= 0
                    ? KotorDirectoryPlaceholder + @"\Movies"
                    : KotorDirectoryPlaceholder + @"\Override";

                Match fileMatch = s_looseFileNamePattern.Match(directions);
                List<string> sources = fileMatch.Success
                    ? BuildLooseFileMoveSources(fileMatch.Groups[1].Value)
                    : new List<string> { ModDirectoryPlaceholder + @"\*" };

                return new Instruction
                {
                    Action = Instruction.ActionType.Move,
                    Source = sources,
                    Destination = destination,
                    Overwrite = directions.IndexOf("do not overwrite", StringComparison.OrdinalIgnoreCase) < 0
                        && directions.IndexOf("don't overwrite", StringComparison.OrdinalIgnoreCase) < 0,
                };
            }

            if (method.IndexOf("executable", StringComparison.OrdinalIgnoreCase) >= 0
                || directions.IndexOf("executable", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return new Instruction
                {
                    Action = Instruction.ActionType.Execute,
                    Source = new List<string> { ModDirectoryPlaceholder + @"\*" },
                    Destination = string.Empty,
                    Overwrite = true,
                };
            }

            return null;
        }

        private static bool LooksLikePatcherInstall([NotNull] string text)
        {
            string lower = text.ToLowerInvariant();
            return lower.IndexOf("tslpatcher", StringComparison.Ordinal) >= 0
                || lower.IndexOf("holopatcher", StringComparison.Ordinal) >= 0
                || lower.IndexOf("multi-run", StringComparison.Ordinal) >= 0;
        }

        private static bool LooksLikeLooseFileInstall([NotNull] string text)
        {
            string lower = text.ToLowerInvariant();
            return lower.IndexOf("loose-file", StringComparison.Ordinal) >= 0
                || lower.IndexOf("loose file", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Builds sandboxed Move sources for a named loose file, including nested paths after Extract.
        /// </summary>
        [NotNull]
        internal static List<string> BuildLooseFileMoveSources([NotNull] string fileName, int maxNestedDepth = 3)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name is required.", nameof(fileName));
            }

            string trimmed = fileName.Trim().Trim('"', '\'');
            var sources = new List<string> { $"{ModDirectoryPlaceholder}\\{trimmed}" };

            for (int depth = 1; depth <= maxNestedDepth; depth++)
            {
                sources.Add($"{ModDirectoryPlaceholder}\\{string.Join("\\", Enumerable.Repeat("*", depth))}\\{trimmed}");
            }

            return sources;
        }

        /// <summary>
        /// Expands existing Move sources with nested mod-directory search paths for a named file.
        /// </summary>
        [NotNull]
        internal static List<string> ExpandLooseFileMoveSources(
            [CanBeNull] IEnumerable<string> existingSources,
            [NotNull] string fileName,
            int maxNestedDepth = 3)
        {
            var merged = new List<string>();
            if (existingSources != null)
            {
                merged.AddRange(existingSources.Where(source => !string.IsNullOrWhiteSpace(source)));
            }

            foreach (string source in BuildLooseFileMoveSources(fileName, maxNestedDepth))
            {
                if (!merged.Any(existing => string.Equals(existing, source, StringComparison.OrdinalIgnoreCase)))
                {
                    merged.Add(source);
                }
            }

            return merged;
        }

        /// <summary>
        /// Normalizes placeholders and enforces the path-sandboxing rules on a draft instruction:
        /// every Source entry and any non-empty Destination must start with
        /// <c>&lt;&lt;modDirectory&gt;&gt;</c> or <c>&lt;&lt;kotorDirectory&gt;&gt;</c>,
        /// followed by a separator when a relative path is present, and must not contain
        /// <c>..</c> segments, rooted segments, or drive letters after the placeholder.
        /// </summary>
        /// <returns>false when the instruction cannot be made sandbox-safe and must be dropped.</returns>
        public static bool TrySanitizeInstruction([NotNull] Instruction instruction)
        {
            if (instruction is null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            List<string> sanitizedSources = instruction.Source
                .Where(source => !string.IsNullOrWhiteSpace(source))
                .Select(NormalizePlaceholders)
                .Where(IsSandboxedPath)
                .ToList();

            string destination = NormalizePlaceholders(instruction.Destination);
            if (!string.IsNullOrEmpty(destination) && !IsSandboxedPath(destination))
            {
                return false;
            }

            bool sourceRequired =
                instruction.Action == Instruction.ActionType.Move ||
                instruction.Action == Instruction.ActionType.Copy ||
                instruction.Action == Instruction.ActionType.Delete ||
                instruction.Action == Instruction.ActionType.Rename ||
                instruction.Action == Instruction.ActionType.Extract ||
                instruction.Action == Instruction.ActionType.Execute ||
                instruction.Action == Instruction.ActionType.Patcher;

            if (sourceRequired && sanitizedSources.Count == 0)
            {
                return false;
            }

            if (RequiresDestination(instruction.Action) && string.IsNullOrWhiteSpace(destination))
            {
                return false;
            }

            instruction.Source = sanitizedSources;
            instruction.Destination = destination;
            return true;
        }

        /// <summary>
        /// Attaches <see cref="ReviewFlagMessage"/> to a component that received draft instructions so
        /// install-review surfaces (GUI warnings and CLI validation-issue serialization) can see it.
        /// Does not overwrite an existing identical flag.
        /// </summary>
        public static void ApplyReviewFlag([NotNull] ModComponent component)
        {
            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (string.IsNullOrWhiteSpace(component.InstallationWarning))
            {
                component.InstallationWarning = ReviewFlagMessage;
                return;
            }

            if (component.InstallationWarning.IndexOf(ReviewFlagMessage, StringComparison.Ordinal) < 0)
            {
                component.InstallationWarning = ReviewFlagMessage + Environment.NewLine + component.InstallationWarning;
            }
        }

        private static bool RequiresDestination(Instruction.ActionType action)
        {
            return action == Instruction.ActionType.Move
                || action == Instruction.ActionType.Copy;
        }

        [NotNull]
        private static string NormalizePlaceholders([CanBeNull] string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Replace(LegacyGameDirectoryPlaceholder, KotorDirectoryPlaceholder);
        }

        /// <summary>
        /// Returns true when <paramref name="path"/> is confined to a placeholder root:
        /// starts with <c>&lt;&lt;modDirectory&gt;&gt;</c> or <c>&lt;&lt;kotorDirectory&gt;&gt;</c>,
        /// optionally followed by <c>/</c> or <c>\</c> and relative segments that do not escape
        /// (mirrors <c>FomodToComponentMapper.NormalizeRelativePath</c> rejection rules).
        /// </summary>
        internal static bool IsSandboxedPath([NotNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string placeholder;
            if (path.StartsWith(ModDirectoryPlaceholder, StringComparison.Ordinal))
            {
                placeholder = ModDirectoryPlaceholder;
            }
            else if (path.StartsWith(KotorDirectoryPlaceholder, StringComparison.Ordinal))
            {
                placeholder = KotorDirectoryPlaceholder;
            }
            else
            {
                return false;
            }

            if (path.Length == placeholder.Length)
            {
                return true;
            }

            char separator = path[placeholder.Length];
            if (separator != '/' && separator != '\\')
            {
                // Require an explicit separator after the placeholder (reject "<<modDirectory>>../x").
                return false;
            }

            string remainder = path.Substring(placeholder.Length + 1).Replace('\\', '/');
            foreach (string segment in remainder.Split('/'))
            {
                if (segment.Length == 0 || string.Equals(segment, ".", StringComparison.Ordinal))
                {
                    continue;
                }

                // Reject traversal, drive letters (e.g. "C:"), and rooted segments after the placeholder.
                if (string.Equals(segment, "..", StringComparison.Ordinal)
                    || segment.IndexOf(':') >= 0
                    || Path.IsPathRooted(segment))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
