// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using ModSync.Core.Services.Validation;
using ModSync.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class LegacyValidationRunnerTests
    {
        [Test]
        public void BuildRunResult_EnvironmentFailure_ReturnsSummaryAndIssues()
        {
            var pipelineResult = new ValidationPipelineResult
            {
                IsSuccess = false,
                ErrorCount = 1,
            };
            pipelineResult.Stages.Add(new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Environment,
                Passed = false,
                Summary = "HoloPatcher missing",
            });

            LegacyValidationRunner.RunResult runResult =
                LegacyValidationRunner.BuildRunResult(pipelineResult);

            Assert.That(runResult.IsSuccess, Is.False);
            Assert.That(runResult.ModIssues, Has.Count.EqualTo(1));
            Assert.That(runResult.SummaryMessage, Does.Contain("error"));
        }

        [Test]
        public void BuildRunResult_SuccessWithoutDryRun_UsesPassedSummary()
        {
            var pipelineResult = new ValidationPipelineResult
            {
                IsSuccess = true,
                ErrorCount = 0,
            };

            LegacyValidationRunner.RunResult runResult =
                LegacyValidationRunner.BuildRunResult(pipelineResult);

            Assert.That(runResult.IsSuccess, Is.True);
            Assert.That(runResult.SummaryMessage, Is.EqualTo("Validation passed."));
            Assert.That(runResult.ModIssues, Is.Empty);
        }

        [Test]
        public void BuildRunResult_AppendLog_ReceivesMapperMessages()
        {
            var pipelineResult = new ValidationPipelineResult();
            var conflicts = new ValidationPipelineStageResult { Stage = ValidationPipelineStage.Conflicts, Passed = true };
            conflicts.Messages.Add("ERROR: Mod A: conflict detail");
            pipelineResult.Stages.Add(conflicts);

            var logLines = new List<string>();
            LegacyValidationRunner.BuildRunResult(pipelineResult, logLines.Add);

            Assert.That(logLines, Has.Count.EqualTo(1));
            Assert.That(logLines[0], Does.Contain("Conflict"));
        }

        [Test]
        public void BuildRunResult_ConflictFailure_IncludesModAndAggregateIssues()
        {
            var pipelineResult = new ValidationPipelineResult
            {
                IsSuccess = false,
                ErrorCount = 1,
            };
            var conflicts = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Conflicts,
                Passed = false,
                Summary = "1 restriction conflict(s)",
            };
            conflicts.Messages.Add("ERROR: Mod A: incompatible with: Mod B");
            pipelineResult.Stages.Add(conflicts);

            LegacyValidationRunner.RunResult runResult =
                LegacyValidationRunner.BuildRunResult(pipelineResult);

            Assert.That(runResult.ModIssues, Has.Count.EqualTo(2));
            Assert.That(runResult.ModIssues[0].ModName, Is.EqualTo("Mod A"));
            Assert.That(runResult.ModIssues[1].ModName, Is.EqualTo("Conflicts"));
        }
    }
}
