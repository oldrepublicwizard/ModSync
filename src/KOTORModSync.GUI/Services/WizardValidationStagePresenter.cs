// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using KOTORModSync.Core.Services.FileSystem;
using KOTORModSync.Core.Services.Validation;

namespace KOTORModSync.Services
{
    /// <summary>
    /// Maps <see cref="ValidationPipelineResult"/> stages to install-wizard validation log lines and result cards.
    /// </summary>
    public static class WizardValidationStagePresenter
    {
        private const int MaxDisplayedDryRunIssues = 5;

        public delegate void AppendLogDelegate([NotNull] string message);

        public delegate void AddResultDelegate([NotNull] string title, [NotNull] string message);

        public static void ApplyStages(
            [NotNull] ValidationPipelineResult pipelineResult,
            int selectedModCount,
            [NotNull] AppendLogDelegate appendLog,
            [NotNull] AddResultDelegate addResult)
        {
            if (pipelineResult is null)
            {
                throw new ArgumentNullException(nameof(pipelineResult));
            }

            if (appendLog is null)
            {
                throw new ArgumentNullException(nameof(appendLog));
            }

            if (addResult is null)
            {
                throw new ArgumentNullException(nameof(addResult));
            }

            int stepIndex = 0;
            foreach (ValidationPipelineStageResult stage in pipelineResult.Stages)
            {
                stepIndex++;
                switch (stage.Stage)
                {
                    case ValidationPipelineStage.Environment:
                        ApplyEnvironmentStage(stage, stepIndex, appendLog, addResult);
                        break;
                    case ValidationPipelineStage.Conflicts:
                        ApplyConflictsStage(stage, stepIndex, selectedModCount, appendLog, addResult);
                        break;
                    case ValidationPipelineStage.InstallOrder:
                        ApplyInstallOrderStage(stage, stepIndex, appendLog, addResult);
                        break;
                    case ValidationPipelineStage.ComponentValidation:
                        ApplyComponentValidationStage(stage, stepIndex, appendLog, addResult);
                        break;
                    case ValidationPipelineStage.DryRun:
                        ApplyDryRunStage(pipelineResult, stepIndex, appendLog, addResult);
                        break;
                }
            }
        }

        private static void ApplyEnvironmentStage(
            ValidationPipelineStageResult stage,
            int stepIndex,
            AppendLogDelegate appendLog,
            AddResultDelegate addResult)
        {
            appendLog($"Step {stepIndex}: Validating installation environment");
            if (stage.Passed)
            {
                appendLog("  ✅ Environment validation passed");
                addResult("✅ Environment", stage.Summary ?? "Installation environment is valid");
            }
            else
            {
                appendLog($"  ❌ Environment validation failed: {stage.Summary}");
                addResult("❌ Environment Error", stage.Summary ?? "Environment validation failed");
            }
        }

        private static void ApplyConflictsStage(
            ValidationPipelineStageResult stage,
            int stepIndex,
            int selectedModCount,
            AppendLogDelegate appendLog,
            AddResultDelegate addResult)
        {
            appendLog($"Step {stepIndex}: Checking conflicts for {selectedModCount} selected mod(s)");
            foreach (string message in stage.Messages)
            {
                appendLog($"  {message}");
                if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                        message,
                        "WARNING:",
                        out string modName,
                        out _,
                        out string detail))
                {
                    addResult($"⚠️ {modName}", detail);
                }
                else if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                             message,
                             "ERROR:",
                             out modName,
                             out _,
                             out detail))
                {
                    addResult($"❌ {modName}", detail);
                }
            }

            if (stage.Messages.Count == 0)
            {
                appendLog("  ✅ No conflicts");
            }
        }

        private static void ApplyInstallOrderStage(
            ValidationPipelineStageResult stage,
            int stepIndex,
            AppendLogDelegate appendLog,
            AddResultDelegate addResult)
        {
            appendLog($"Step {stepIndex}: Validating mod installation order");
            foreach (string message in stage.Messages)
            {
                appendLog($"  {message}");
                if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                        message,
                        "WARNING:",
                        out string modName,
                        out _,
                        out string detail))
                {
                    addResult($"⚠️ {modName}", detail);
                }
                else if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                             message,
                             "ERROR:",
                             out modName,
                             out _,
                             out detail))
                {
                    addResult($"❌ {modName}", detail);
                }
            }

            if (stage.Passed && !stage.HasWarnings)
            {
                if (stage.Messages.Count == 0)
                {
                    appendLog("  ✅ Install order is correct");
                }

                addResult("✅ Install Order", stage.Summary ?? "Mod installation order is correct");
            }
            else if (stage.Passed && stage.HasWarnings)
            {
                addResult("⚠️ Install Order", stage.Summary ?? "Mods will be automatically reordered");
            }
            else
            {
                if (stage.Messages.Count == 0)
                {
                    appendLog($"  ❌ {stage.Summary}");
                }

                addResult("❌ Install Order", stage.Summary ?? "Install order validation failed");
            }
        }

        private static void ApplyComponentValidationStage(
            ValidationPipelineStageResult stage,
            int stepIndex,
            AppendLogDelegate appendLog,
            AddResultDelegate addResult)
        {
            appendLog($"Step {stepIndex}: Validating mod archives");
            foreach (string message in stage.Messages)
            {
                if (message.StartsWith("OK:", StringComparison.Ordinal))
                {
                    appendLog($"  ✅ {message.Substring(3).Trim()}");
                }
                else
                {
                    appendLog($"  {message}");
                    if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                            message,
                            "ERROR:",
                            out string modName,
                            out _,
                            out string detail))
                    {
                        addResult($"❌ {modName}", detail);
                    }
                    else if (ValidationPipelineDialogMapper.TryParsePrefixedStageMessage(
                                 message,
                                 "WARNING:",
                                 out modName,
                                 out _,
                                 out detail))
                    {
                        addResult($"⚠️ {modName}", detail);
                    }
                }
            }

            if (!stage.Passed)
            {
                addResult("❌ Archive Validation", stage.Summary ?? "Archive validation failed");
            }
            else if (stage.HasWarnings)
            {
                addResult("⚠️ Archive Validation", stage.Summary ?? "Archive validation passed with warnings");
            }
        }

        private static void ApplyDryRunStage(
            ValidationPipelineResult pipelineResult,
            int stepIndex,
            AppendLogDelegate appendLog,
            AddResultDelegate addResult)
        {
            appendLog($"Step {stepIndex}: Running instruction execution validation (dry-run)");
            if (pipelineResult.DryRunResult is null)
            {
                return;
            }

            DryRunValidationResult dryRunResult = pipelineResult.DryRunResult;
            if (dryRunResult.IsValid && !dryRunResult.HasWarnings)
            {
                appendLog("  ✅ All instructions validated successfully");
                addResult(
                    "✅ Instruction Execution",
                    "All instructions validated successfully. Dry-run completed without errors.");
                return;
            }

            int dryRunErrors = dryRunResult.Issues.Count(i =>
                i.Severity == ValidationSeverity.Error || i.Severity == ValidationSeverity.Critical);
            int dryRunWarnings = dryRunResult.Issues.Count(i => i.Severity == ValidationSeverity.Warning);
            appendLog($"  Found {dryRunErrors} error(s) and {dryRunWarnings} warning(s)");

            if (dryRunErrors > 0)
            {
                List<ValidationIssue> errorIssues = dryRunResult.Issues
                    .Where(i => i.Severity == ValidationSeverity.Error || i.Severity == ValidationSeverity.Critical)
                    .Take(MaxDisplayedDryRunIssues)
                    .ToList();
                foreach (ValidationIssue issue in errorIssues)
                {
                    appendLog($"    ❌ [{issue.Category}] {issue.Message}");
                    addResult(FormatDryRunIssueTitle(issue), FormatDryRunIssueMessage(issue));
                }

                string overflow = dryRunErrors > errorIssues.Count
                    ? $" Showing first {errorIssues.Count} of {dryRunErrors}."
                    : string.Empty;
                addResult(
                    "❌ Instruction Execution",
                    $"Dry-run validation failed with {dryRunErrors} error(s).{overflow}");
            }
            else if (dryRunWarnings > 0)
            {
                List<ValidationIssue> warningIssues = dryRunResult.Issues
                    .Where(i => i.Severity == ValidationSeverity.Warning)
                    .Take(MaxDisplayedDryRunIssues)
                    .ToList();
                foreach (ValidationIssue issue in warningIssues)
                {
                    appendLog($"    ⚠️ [{issue.Category}] {issue.Message}");
                    addResult(FormatDryRunIssueTitle(issue), FormatDryRunIssueMessage(issue));
                }

                string overflow = dryRunWarnings > warningIssues.Count
                    ? $" Showing first {warningIssues.Count} of {dryRunWarnings}."
                    : string.Empty;
                addResult(
                    "⚠️ Instruction Execution",
                    $"Dry-run validation passed with {dryRunWarnings} warning(s).{overflow}");
            }
        }

        private static string FormatDryRunIssueTitle(ValidationIssue issue)
        {
            string modName = issue.AffectedComponent?.Name ?? "Unknown";
            string category = issue.Category ?? "Validation";
            string prefix = issue.Severity == ValidationSeverity.Warning ? "⚠️" : "❌";
            return $"{prefix} {modName} ({category})";
        }

        private static string FormatDryRunIssueMessage(ValidationIssue issue)
        {
            string message = issue.Message ?? "No description available";
            string solution = ValidationPipelineDialogMapper.GetSolutionForIssue(issue);
            return string.IsNullOrEmpty(solution) ? message : $"{message} — {solution}";
        }
    }
}
