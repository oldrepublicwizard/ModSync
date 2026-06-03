// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using KOTORModSync.Core.Services.Validation;
using KOTORModSync.Services;

using NUnit.Framework;

namespace KOTORModSync.Tests
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
    }
}
