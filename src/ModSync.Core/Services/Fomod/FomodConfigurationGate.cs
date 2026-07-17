// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Blocks validate/install when a downloaded FOMOD archive is not fully configured.
    /// Only <see cref="FomodDownloadPromptState.StatusConfigured"/> satisfies the gate.
    /// </summary>
    public static class FomodConfigurationGate
    {
        public const string IssueCategory = "FOMOD";

        public const string RecoveryHint =
            "Run Fetch Downloads and complete the FOMOD installer wizard for this archive, "
            + "or re-run CLI download with --fomod-choices / MODSYNC_FOMOD_CHOICES "
            + "(interactive TTY configure also works).";

        public const string MissingInstructionsHint =
            "Status is configured but no archive-scoped install instructions were found. "
            + "Re-run Configure FOMOD (or Fetch Downloads) so wizard output is merged into the mod.";

        public const string MissingArchiveHint =
            "Re-download the archive (Fetch Downloads), then configure the FOMOD installer before validate/install.";

        public sealed class GateIssue
        {
            [NotNull]
            public ModComponent Component { get; set; }

            [NotNull]
            public string ArchiveFileName { get; set; }

            [CanBeNull]
            public string PromptStatus { get; set; }

            /// <summary>
            /// True when the archive could not be enumerated (corrupt/unreadable).
            /// Fail-closed: blocks validate/install even though FOMOD could not be confirmed.
            /// </summary>
            public bool ArchiveUnreadable { get; set; }

            /// <summary>
            /// True when a registered archive with prior FOMOD prompt state is missing on disk.
            /// Fail-closed: avoids skipping the gate via GetPaths existence filter.
            /// </summary>
            public bool ArchiveMissing { get; set; }

            [CanBeNull]
            public string InspectionFailureMessage { get; set; }
        }

        public sealed class GateWarning
        {
            [NotNull]
            public ModComponent Component { get; set; }

            [NotNull]
            public string ArchiveFileName { get; set; }

            [NotNull]
            public string Message { get; set; }
        }

        public sealed class GateResult
        {
            public bool Passed => Issues.Count == 0;

            [NotNull]
            public List<GateIssue> Issues { get; } = new List<GateIssue>();

            /// <summary>
            /// Soft findings that do not fail the gate (e.g. configured without archive-scoped instructions).
            /// </summary>
            [NotNull]
            public List<GateWarning> Warnings { get; } = new List<GateWarning>();
        }

        [NotNull]
        public static GateResult Validate(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> allComponents,
            [NotNull][ItemNotNull] IEnumerable<ModComponent> seedComponents,
            [NotNull] string modDirectory)
        {
            if (allComponents is null)
            {
                throw new ArgumentNullException(nameof(allComponents));
            }

            if (seedComponents is null)
            {
                throw new ArgumentNullException(nameof(seedComponents));
            }

            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new ArgumentException("Mod directory is required.", nameof(modDirectory));
            }

            var result = new GateResult();
            List<ModComponent> scope = ExpandWithHardDependencies(seedComponents, allComponents);

            foreach (ModComponent component in scope)
            {
                foreach (FomodDownloadedArchivePaths.RegisteredArchive entry in
                         FomodDownloadedArchivePaths.EnumerateRegisteredArchives(component, modDirectory))
                {
                    string archiveFileName = System.IO.Path.GetFileName(entry.RegisteredName);
                    string status = FomodDownloadPromptState.GetStatus(component, archiveFileName);

                    if (!entry.ExistsOnDisk)
                    {
                        // R3: prior FOMOD prompt state proves this archive mattered — fail closed when missing.
                        // Without prompt state we only soft-warn (component archive validation covers generic misses).
                        if (!string.IsNullOrEmpty(status))
                        {
                            result.Issues.Add(new GateIssue
                            {
                                Component = component,
                                ArchiveFileName = archiveFileName,
                                PromptStatus = status,
                                ArchiveMissing = true,
                            });
                        }
                        else
                        {
                            result.Warnings.Add(new GateWarning
                            {
                                Component = component,
                                ArchiveFileName = archiveFileName,
                                Message =
                                    $"Registered archive '{archiveFileName}' is not on disk under the mod directory. "
                                    + MissingArchiveHint,
                            });
                        }

                        continue;
                    }

                    if (!FomodArchiveProbe.TryInspectArchive(
                            entry.FullPath,
                            out bool isFomod,
                            out _,
                            out string inspectionFailure))
                    {
                        // Fail closed: a registered downloaded archive that cannot be inspected
                        // must not bypass the configured-only gate.
                        result.Issues.Add(new GateIssue
                        {
                            Component = component,
                            ArchiveFileName = archiveFileName,
                            ArchiveUnreadable = true,
                            InspectionFailureMessage = inspectionFailure,
                        });
                        continue;
                    }

                    if (!isFomod)
                    {
                        continue;
                    }

                    if (string.Equals(status, FomodDownloadPromptState.StatusConfigured, StringComparison.Ordinal))
                    {
                        // R1: soft check — configured status without archive-scoped instructions.
                        if (!FomodConfiguredComponentMerger.HasArchiveScopedInstructions(component, archiveFileName))
                        {
                            result.Warnings.Add(new GateWarning
                            {
                                Component = component,
                                ArchiveFileName = archiveFileName,
                                Message =
                                    $"FOMOD archive '{archiveFileName}' is marked configured but has no "
                                    + "matching archive-scoped instructions. "
                                    + MissingInstructionsHint,
                            });
                        }

                        continue;
                    }

                    result.Issues.Add(new GateIssue
                    {
                        Component = component,
                        ArchiveFileName = archiveFileName,
                        PromptStatus = status,
                    });
                }
            }

            return result;
        }

        [NotNull]
        public static List<ModComponent> ExpandWithHardDependencies(
            [NotNull][ItemNotNull] IEnumerable<ModComponent> seedComponents,
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> allComponents)
        {
            var byGuid = new Dictionary<Guid, ModComponent>();
            foreach (ModComponent component in allComponents)
            {
                if (component is null)
                {
                    continue;
                }

                // Keep first occurrence; duplicate Guids must not crash the gate.
                if (!byGuid.ContainsKey(component.Guid))
                {
                    byGuid[component.Guid] = component;
                }
            }

            var visited = new HashSet<Guid>();
            var ordered = new List<ModComponent>();
            var queue = new Queue<ModComponent>();

            foreach (ModComponent seed in seedComponents)
            {
                if (seed is null)
                {
                    continue;
                }

                queue.Enqueue(seed);
            }

            while (queue.Count > 0)
            {
                ModComponent component = queue.Dequeue();
                if (!visited.Add(component.Guid))
                {
                    continue;
                }

                ordered.Add(component);

                if (component.Dependencies is null)
                {
                    continue;
                }

                foreach (Guid dependencyGuid in component.Dependencies)
                {
                    if (byGuid.TryGetValue(dependencyGuid, out ModComponent dependency))
                    {
                        queue.Enqueue(dependency);
                    }
                }
            }

            return ordered;
        }

        [NotNull]
        public static string FormatIssueMessage([NotNull] GateIssue issue)
        {
            if (issue is null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (issue.ArchiveMissing)
            {
                string statusLabel = string.IsNullOrEmpty(issue.PromptStatus)
                    ? "known FOMOD archive"
                    : $"FOMOD archive (status '{issue.PromptStatus}')";
                return $"Registered {statusLabel} '{issue.ArchiveFileName}' is missing on disk. {MissingArchiveHint}";
            }

            if (issue.ArchiveUnreadable)
            {
                string detail = string.IsNullOrEmpty(issue.InspectionFailureMessage)
                    ? "could not be inspected"
                    : $"could not be inspected ({issue.InspectionFailureMessage})";
                return $"Archive '{issue.ArchiveFileName}' {detail}; treat as unconfigured FOMOD until it can be read. {RecoveryHint}";
            }

            string statusText = string.IsNullOrEmpty(issue.PromptStatus)
                ? "not configured"
                : $"status '{issue.PromptStatus}'";

            return $"FOMOD archive '{issue.ArchiveFileName}' is {statusText}. {RecoveryHint}";
        }
    }
}
