// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Core.Services.Validation;
using ModSync.Dialogs;

namespace ModSync.Services
{
    /// <summary>
    /// Runs the legacy Getting Started validation pipeline and maps results for <see cref="ValidationDialog"/>.
    /// UI (progress dialog, final dialog) stays in <c>MainWindow</c>.
    /// </summary>
    public static class LegacyValidationRunner
    {
        [NotNull]
        public sealed class RunResult
        {
            [NotNull]
            public ValidationPipelineResult PipelineResult { get; set; }

            [NotNull]
            public List<ValidationIssue> ModIssues { get; set; } = new List<ValidationIssue>();

            public bool IsSuccess => PipelineResult.IsSuccess;

            public int ErrorCount => PipelineResult.ErrorCount;

            [NotNull]
            public string SummaryMessage { get; set; } = string.Empty;
        }

        [NotNull]
        public static async Task<RunResult> RunAsync(
            [NotNull] MainConfig mainConfig,
            CancellationToken cancellationToken = default,
            Action<string> appendLog = null)
        {
            if (mainConfig is null)
            {
                throw new ArgumentNullException(nameof(mainConfig));
            }

            ComponentValidationService.ClearValidationCache();

            var pipelineOptions = ValidationPipelineOptions.WizardFull;
            pipelineOptions.MainConfig = mainConfig;
            pipelineOptions.CancellationToken = cancellationToken;

            ValidationPipelineResult pipelineResult = await InstallationValidationPipeline.RunAsync(
                MainConfig.AllComponents,
                pipelineOptions).ConfigureAwait(false);

            return BuildRunResult(pipelineResult, appendLog);
        }

        [NotNull]
        public static RunResult BuildRunResult(
            [NotNull] ValidationPipelineResult pipelineResult,
            Action<string> appendLog = null)
        {
            if (pipelineResult is null)
            {
                throw new ArgumentNullException(nameof(pipelineResult));
            }

            var modIssues = new List<ValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues, appendLog);

            DryRunValidationResult dryRunResult = pipelineResult.DryRunResult
                ?? new DryRunValidationResult();
            ValidationPipelineDialogMapper.AddDryRunIssues(dryRunResult, modIssues, appendLog);

            string summaryMessage = pipelineResult.DryRunResult?.GetSummaryMessage()
                ?? (pipelineResult.IsSuccess
                    ? "Validation passed."
                    : $"{pipelineResult.ErrorCount} validation error(s). Check logs for details.");

            return new RunResult
            {
                PipelineResult = pipelineResult,
                ModIssues = modIssues,
                SummaryMessage = summaryMessage,
            };
        }
    }
}
