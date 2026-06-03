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
    public sealed class WizardValidationStagePresenterTests
    {
        [Test]
        public void ApplyStages_EnvironmentFailure_AddsErrorResultCard()
        {
            var pipelineResult = new ValidationPipelineResult();
            pipelineResult.Stages.Add(new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Environment,
                Passed = false,
                Summary = "HoloPatcher missing",
            });

            var results = new List<(string Title, string Message)>();
            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedModCount: 1,
                _ => { },
                (title, message) => results.Add((title, message)));

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("❌ Environment Error"));
            Assert.That(results[0].Message, Does.Contain("HoloPatcher"));
        }

        [Test]
        public void ApplyStages_ConflictError_AddsModResultCard()
        {
            var pipelineResult = new ValidationPipelineResult();
            var conflicts = new ValidationPipelineStageResult { Stage = ValidationPipelineStage.Conflicts, Passed = true };
            conflicts.Messages.Add("ERROR: Mod A: restriction conflict");
            pipelineResult.Stages.Add(conflicts);

            var results = new List<(string Title, string Message)>();
            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedModCount: 2,
                _ => { },
                (title, message) => results.Add((title, message)));

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Title, Is.EqualTo("❌ Mod A"));
            Assert.That(results[0].Message, Is.EqualTo("Mod A: restriction conflict"));
        }

        [Test]
        public void ApplyStages_ArchiveError_AddsModResultCard()
        {
            var pipelineResult = new ValidationPipelineResult();
            var archives = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = false,
            };
            archives.Messages.Add("ERROR: Test Mod: missing archive");
            archives.Summary = "1 component error(s)";
            pipelineResult.Stages.Add(archives);

            var results = new List<(string Title, string Message)>();
            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedModCount: 1,
                _ => { },
                (title, message) => results.Add((title, message)));

            Assert.That(results, Has.Count.EqualTo(2));
            Assert.That(results[0].Title, Is.EqualTo("❌ Test Mod"));
            Assert.That(results[0].Message, Is.EqualTo("Test Mod: missing archive"));
            Assert.That(results[1].Title, Is.EqualTo("❌ Archive Validation"));
        }

        [Test]
        public void ApplyStages_ArchiveWarning_AddsModResultCard()
        {
            var pipelineResult = new ValidationPipelineResult();
            var archives = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = true,
                HasWarnings = true,
                Summary = "1 warning(s)",
            };
            archives.Messages.Add("WARNING: Test Mod: checksum mismatch");
            pipelineResult.Stages.Add(archives);

            var results = new List<(string Title, string Message)>();
            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedModCount: 1,
                _ => { },
                (title, message) => results.Add((title, message)));

            Assert.That(results, Has.Count.EqualTo(2));
            Assert.That(results[0].Title, Is.EqualTo("⚠️ Test Mod"));
            Assert.That(results[0].Message, Is.EqualTo("Test Mod: checksum mismatch"));
            Assert.That(results[1].Title, Is.EqualTo("⚠️ Archive Validation"));
        }

        [Test]
        public void ApplyStages_ArchiveStageFailure_AddsSummaryCard()
        {
            var pipelineResult = new ValidationPipelineResult();
            var archives = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = false,
                Summary = "2 component error(s)",
            };
            archives.Messages.Add("ERROR: Mod A: missing archive");
            archives.Messages.Add("ERROR: Mod B: corrupt archive");
            pipelineResult.Stages.Add(archives);

            var results = new List<(string Title, string Message)>();
            WizardValidationStagePresenter.ApplyStages(
                pipelineResult,
                selectedModCount: 2,
                _ => { },
                (title, message) => results.Add((title, message)));

            Assert.That(results, Has.Count.EqualTo(3));
            Assert.That(results[2].Title, Is.EqualTo("❌ Archive Validation"));
            Assert.That(results[2].Message, Is.EqualTo("2 component error(s)"));
        }
    }
}
