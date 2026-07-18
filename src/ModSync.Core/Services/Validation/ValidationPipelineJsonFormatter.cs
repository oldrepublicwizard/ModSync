// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using ModSync.Core.Services.FileSystem;

namespace ModSync.Core.Services.Validation
{
    /// <summary>
    /// Serializes <see cref="ValidationPipelineResult"/> for agent/CLI machine output (<c>validate --output json</c>).
    /// </summary>
    public static class ValidationPipelineJsonFormatter
    {
        [NotNull]
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        [NotNull]
        public static string SerializeReport(
            [NotNull] ValidationPipelineResult pipelineResult,
            int componentCount,
            [CanBeNull] string inputPath = null)
        {
            var report = new ValidationPipelineJsonReport
            {
                Success = pipelineResult.IsSuccess,
                ExitCode = pipelineResult.ExitCode,
                ErrorCount = pipelineResult.ErrorCount,
                WarningCount = pipelineResult.WarningCount,
                PassedCount = pipelineResult.PassedCount,
                ComponentCount = componentCount,
                InputPath = inputPath,
                Stages = pipelineResult.Stages.Select(MapStage).ToList(),
                DryRun = pipelineResult.DryRunResult is null ? null : MapDryRun(pipelineResult.DryRunResult),
            };

            return JsonSerializer.Serialize(report, JsonOptions);
        }

        [NotNull]
        public static string SerializeError([NotNull] string error, int exitCode = 1)
        {
            var report = new ValidationPipelineJsonReport
            {
                Success = false,
                ExitCode = exitCode,
                Error = error,
            };

            return JsonSerializer.Serialize(report, JsonOptions);
        }

        [NotNull]
        private static ValidationPipelineJsonStage MapStage([NotNull] ValidationPipelineStageResult stage)
        {
            return new ValidationPipelineJsonStage
            {
                Stage = stage.Stage.ToString(),
                Passed = stage.Passed,
                HasWarnings = stage.HasWarnings,
                Summary = stage.Summary,
                Messages = stage.Messages.Count == 0 ? null : new List<string>(stage.Messages),
            };
        }

        [NotNull]
        private static ValidationPipelineJsonDryRun MapDryRun([NotNull] DryRunValidationResult dryRun)
        {
            return new ValidationPipelineJsonDryRun
            {
                IsValid = dryRun.IsValid,
                HasWarnings = dryRun.HasWarnings,
                Issues = dryRun.Issues.Select(MapIssue).ToList(),
            };
        }

        [NotNull]
        private static ValidationPipelineJsonIssue MapIssue([NotNull] ValidationIssue issue)
        {
            return new ValidationPipelineJsonIssue
            {
                Severity = issue.Severity.ToString(),
                Category = issue.Category,
                Message = issue.Message,
                AffectedPath = issue.AffectedPath,
                ComponentName = issue.AffectedComponent?.Name,
                ComponentGuid = issue.AffectedComponent?.Guid.ToString(),
                InstructionIndex = issue.InstructionIndex > 0 ? issue.InstructionIndex : (int?)null,
                Solution = issue.Solution,
            };
        }
    }

    public sealed class ValidationPipelineJsonReport
    {
        public bool Success { get; set; }

        public int ExitCode { get; set; }

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public int PassedCount { get; set; }

        public int ComponentCount { get; set; }

        [CanBeNull]
        public string InputPath { get; set; }

        [CanBeNull]
        public string Error { get; set; }

        [CanBeNull]
        public List<ValidationPipelineJsonStage> Stages { get; set; }

        [CanBeNull]
        public ValidationPipelineJsonDryRun DryRun { get; set; }
    }

    public sealed class ValidationPipelineJsonStage
    {
        public string Stage { get; set; }

        public bool Passed { get; set; }

        public bool HasWarnings { get; set; }

        [CanBeNull]
        public string Summary { get; set; }

        [CanBeNull]
        public List<string> Messages { get; set; }
    }

    public sealed class ValidationPipelineJsonDryRun
    {
        public bool IsValid { get; set; }

        public bool HasWarnings { get; set; }

        [CanBeNull]
        public List<ValidationPipelineJsonIssue> Issues { get; set; }
    }

    public sealed class ValidationPipelineJsonIssue
    {
        public string Severity { get; set; }

        [CanBeNull]
        public string Category { get; set; }

        [CanBeNull]
        public string Message { get; set; }

        [CanBeNull]
        public string AffectedPath { get; set; }

        [CanBeNull]
        public string ComponentName { get; set; }

        [CanBeNull]
        public string ComponentGuid { get; set; }

        public int? InstructionIndex { get; set; }

        [CanBeNull]
        public string Solution { get; set; }
    }
}
