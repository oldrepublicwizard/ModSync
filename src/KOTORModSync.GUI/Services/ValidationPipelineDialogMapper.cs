// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using KOTORModSync.Core.Services.FileSystem;
using KOTORModSync.Core.Services.Validation;
using KOTORModSync.Dialogs;

namespace KOTORModSync.Services
{
    /// <summary>
    /// Maps <see cref="ValidationPipelineResult"/> and dry-run output to <see cref="ValidationIssue"/> rows
    /// for validation dialogs (MainWindow legacy flow, ValidationService failure analysis).
    /// </summary>
    public static class ValidationPipelineDialogMapper
    {
        public static void AddPipelineStageIssues(
            ValidationPipelineResult pipelineResult,
            List<Dialogs.ValidationIssue> modIssues,
            Action<string> appendLog = null)
        {
            if (pipelineResult is null)
            {
                throw new ArgumentNullException(nameof(pipelineResult));
            }

            if (modIssues is null)
            {
                throw new ArgumentNullException(nameof(modIssues));
            }

            foreach (ValidationPipelineStageResult stage in pipelineResult.Stages)
            {
                switch (stage.Stage)
                {
                    case ValidationPipelineStage.Environment:
                        if (!stage.Passed)
                        {
                            string summary = stage.Summary ?? "Environment validation failed";
                            modIssues.Add(new Dialogs.ValidationIssue
                            {
                                Icon = "✗",
                                ModName = "Environment",
                                IssueType = "Environment",
                                Description = summary,
                                Solution = "Verify HoloPatcher, KOTOR paths, and install directories are configured correctly.",
                            });
                            appendLog?.Invoke($"✗ [Environment] {summary}");
                        }

                        break;
                    case ValidationPipelineStage.Conflicts:
                        foreach (string message in stage.Messages)
                        {
                            if (TryParsePrefixedStageMessage(message, "ERROR:", out string modName, out string description, out string detail))
                            {
                                modIssues.Add(new Dialogs.ValidationIssue
                                {
                                    Icon = "✗",
                                    ModName = modName,
                                    IssueType = "Conflict",
                                    Description = description,
                                    Solution = "Resolve dependency or restriction conflicts before installing.",
                                });
                                appendLog?.Invoke($"✗ [Conflict] {detail}");
                            }
                            else if (TryParsePrefixedStageMessage(message, "WARNING:", out modName, out description, out detail))
                            {
                                modIssues.Add(new Dialogs.ValidationIssue
                                {
                                    Icon = "⚠",
                                    ModName = modName,
                                    IssueType = "Conflict",
                                    Description = description,
                                    Solution = "Review mod restrictions; installation may still proceed with warnings.",
                                });
                                appendLog?.Invoke($"⚠ [Conflict] {detail}");
                            }
                        }

                        break;
                    case ValidationPipelineStage.InstallOrder:
                        if (!stage.Passed)
                        {
                            string summary = stage.Summary ?? "Install order validation failed";
                            modIssues.Add(new Dialogs.ValidationIssue
                            {
                                Icon = "✗",
                                ModName = "Install Order",
                                IssueType = "InstallOrder",
                                Description = summary,
                                Solution = "Fix circular dependencies or missing prerequisites in the mod list.",
                            });
                            appendLog?.Invoke($"✗ [InstallOrder] {summary}");
                        }
                        else if (stage.HasWarnings)
                        {
                            string summary = stage.Summary ?? "Mods will be automatically reordered";
                            modIssues.Add(new Dialogs.ValidationIssue
                            {
                                Icon = "⚠",
                                ModName = "Install Order",
                                IssueType = "InstallOrder",
                                Description = summary,
                                Solution = "Review install order; the app may reorder mods automatically.",
                            });
                            appendLog?.Invoke($"⚠ [InstallOrder] {summary}");
                        }

                        break;
                    case ValidationPipelineStage.ComponentValidation:
                        foreach (string message in stage.Messages)
                        {
                            if (TryParsePrefixedStageMessage(message, "ERROR:", out string modName, out string description, out string detail))
                            {
                                modIssues.Add(new Dialogs.ValidationIssue
                                {
                                    Icon = "✗",
                                    ModName = modName,
                                    IssueType = "ArchiveValidation",
                                    Description = description,
                                    Solution = "Verify the archive exists and is not corrupted. Try re-downloading from the mod link.",
                                });
                                appendLog?.Invoke($"✗ [ArchiveValidation] {detail}");
                            }
                        }

                        break;
                }
            }
        }

        public static void AddDryRunIssues(
            DryRunValidationResult dryRunResult,
            List<Dialogs.ValidationIssue> modIssues,
            Action<string> appendLog = null)
        {
            if (dryRunResult is null)
            {
                return;
            }

            if (modIssues is null)
            {
                throw new ArgumentNullException(nameof(modIssues));
            }

            foreach (Core.Services.FileSystem.ValidationIssue coreIssue in dryRunResult.Issues)
            {
                string icon = GetIconForSeverity(coreIssue.Severity);
                modIssues.Add(new Dialogs.ValidationIssue
                {
                    Icon = icon,
                    ModName = coreIssue.AffectedComponent?.Name ?? "Unknown",
                    IssueType = coreIssue.Category ?? "Validation",
                    Description = coreIssue.Message ?? "No description available",
                    Solution = GetSolutionForIssue(coreIssue),
                    VfsIssue = coreIssue,
                    Component = coreIssue.AffectedComponent,
                });

                if (appendLog != null)
                {
                    appendLog($"{icon} [{coreIssue.Category}] {coreIssue.Message}");
                }
            }
        }

        public static string GetSolutionForIssue(Core.Services.FileSystem.ValidationIssue issue)
        {
            if (issue is null)
            {
                return string.Empty;
            }

            if (string.Equals(issue.Category, "ArchiveValidation", StringComparison.Ordinal) ||
                string.Equals(issue.Category, "ExtractArchive", StringComparison.Ordinal))
            {
                return "Verify the archive exists and is not corrupted. Try re-downloading from the mod link.";
            }

            if ((string.Equals(issue.Category, "MoveFile", StringComparison.Ordinal) ||
                 string.Equals(issue.Category, "CopyFile", StringComparison.Ordinal)) &&
                issue.Message?.Contains("does not exist", StringComparison.Ordinal) == true)
            {
                return "The required file is missing. This may indicate an incomplete mod archive or incorrect source path.";
            }

            if ((string.Equals(issue.Category, "MoveFile", StringComparison.Ordinal) ||
                 string.Equals(issue.Category, "CopyFile", StringComparison.Ordinal)) &&
                issue.Message?.Contains("already exists", StringComparison.Ordinal) == true)
            {
                return "File conflict detected. This may be expected - ensure mod installation order is correct.";
            }

            if (string.Equals(issue.Category, "DeleteFile", StringComparison.Ordinal))
            {
                return "File does not exist to delete. This may indicate incorrect instruction order.";
            }

            if (string.Equals(issue.Category, "ExecuteProcess", StringComparison.Ordinal))
            {
                return "The required executable is missing. Verify the mod archive was extracted correctly.";
            }

            return string.Empty;
        }

        internal static bool TryParsePrefixedStageMessage(
            string message,
            string prefix,
            out string modName,
            out string description,
            out string detail)
        {
            modName = "Unknown";
            description = string.Empty;
            detail = string.Empty;

            if (string.IsNullOrEmpty(message) || !message.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            detail = message.Substring(prefix.Length).Trim();
            ParseModNameAndDescription(detail, out modName, out description);
            return true;
        }

        public static void ParseModNameAndDescription(string detail, out string modName, out string description)
        {
            int colon = detail.IndexOf(':');
            if (colon > 0)
            {
                modName = detail.Substring(0, colon).Trim();
                description = detail.Substring(colon + 1).Trim();
            }
            else
            {
                modName = "Unknown";
                description = detail;
            }
        }

        private static string GetIconForSeverity(ValidationSeverity severity)
        {
            if (severity == ValidationSeverity.Error || severity == ValidationSeverity.Critical)
            {
                return "✗";
            }

            if (severity == ValidationSeverity.Warning)
            {
                return "⚠";
            }

            return "ℹ";
        }
    }
}
