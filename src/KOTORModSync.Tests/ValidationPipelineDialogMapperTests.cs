// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using KOTORModSync.Core;
using KOTORModSync.Core.Services.FileSystem;
using KOTORModSync.Core.Services.Validation;
using KOTORModSync.Dialogs;
using KOTORModSync.Services;

using NUnit.Framework;

using CoreValidationIssue = KOTORModSync.Core.Services.FileSystem.ValidationIssue;
using DialogValidationIssue = KOTORModSync.Dialogs.ValidationIssue;

namespace KOTORModSync.Tests
{
    [TestFixture]
    public sealed class ValidationPipelineDialogMapperTests
    {
        [Test]
        public void ParseModNameAndDescription_SplitsOnFirstColon()
        {
            ValidationPipelineDialogMapper.ParseModNameAndDescription(
                "Test Mod: missing archive",
                out string modName,
                out string description);

            Assert.That(modName, Is.EqualTo("Test Mod"));
            Assert.That(description, Is.EqualTo("missing archive"));
        }

        [Test]
        public void AddPipelineStageIssues_EnvironmentFailure_AddsEnvironmentIssue()
        {
            var pipelineResult = new ValidationPipelineResult();
            pipelineResult.Stages.Add(new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Environment,
                Passed = false,
                Summary = "HoloPatcher missing",
            });

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(1));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Environment"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("✗"));
            Assert.That(modIssues[0].Description, Does.Contain("HoloPatcher"));
        }

        [Test]
        public void AddPipelineStageIssues_ConflictWarning_ParsesModName()
        {
            var pipelineResult = new ValidationPipelineResult();
            var conflicts = new ValidationPipelineStageResult { Stage = ValidationPipelineStage.Conflicts, Passed = true };
            conflicts.Messages.Add("WARNING: Mod A: conflicts with Mod B");
            pipelineResult.Stages.Add(conflicts);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(1));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Mod A"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("⚠"));
            Assert.That(modIssues[0].IssueType, Is.EqualTo("Conflict"));
        }

        [Test]
        public void AddDryRunIssues_Warning_IncludesWarningIconAndSolution()
        {
            var dryRun = new DryRunValidationResult();
            dryRun.Issues.Add(new CoreValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Category = "MoveFile",
                Message = "target already exists",
                AffectedComponent = new ModComponent { Name = "Sample Mod" },
            });

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddDryRunIssues(dryRun, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(1));
            Assert.That(modIssues[0].Icon, Is.EqualTo("⚠"));
            Assert.That(modIssues[0].Solution, Does.Contain("installation order"));
        }

        [Test]
        public void GetSolutionForIssue_ArchiveValidation_ReturnsDownloadHint()
        {
            var coreIssue = new CoreValidationIssue
            {
                Category = "ArchiveValidation",
                Message = "Archive not found",
            };

            string solution = ValidationPipelineDialogMapper.GetSolutionForIssue(coreIssue);

            Assert.That(solution, Does.Contain("re-downloading"));
        }
    }
}
