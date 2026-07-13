// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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

            [CanBeNull]
            public string InspectionFailureMessage { get; set; }
        }

        public sealed class GateResult
        {
            public bool Passed => Issues.Count == 0;

            [NotNull]
            public List<GateIssue> Issues { get; } = new List<GateIssue>();
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
                foreach (string archivePath in FomodDownloadedArchivePaths.GetPaths(component, modDirectory))
                {
                    string archiveFileName = System.IO.Path.GetFileName(archivePath);
                    if (!FomodArchiveProbe.TryInspectArchive(
                            archivePath,
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

                    string status = FomodDownloadPromptState.GetStatus(component, archiveFileName);
                    if (string.Equals(status, FomodDownloadPromptState.StatusConfigured, StringComparison.Ordinal))
                    {
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

            if (issue.ArchiveUnreadable)
            {
                string detail = string.IsNullOrEmpty(issue.InspectionFailureMessage)
                    ? "could not be inspected"
                    : $"could not be inspected ({issue.InspectionFailureMessage})";
                return $"Archive '{issue.ArchiveFileName}' {detail}; treat as unconfigured FOMOD until it can be read. {RecoveryHint}";
            }

            string statusLabel = string.IsNullOrEmpty(issue.PromptStatus)
                ? "not configured"
                : $"status '{issue.PromptStatus}'";

            return $"FOMOD archive '{issue.ArchiveFileName}' is {statusLabel}. {RecoveryHint}";
        }
    }
}
