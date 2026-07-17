// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using ModSync.Core;
using ModSync.Core.Services.FileSystem;
using ModSync.Core.Services.Fomod;
using ModSync.Core.Services.Validation;
using ModSync.Dialogs;
using ModSync.Services;

using NUnit.Framework;

using CoreValidationIssue = ModSync.Core.Services.FileSystem.ValidationIssue;
using DialogValidationIssue = ModSync.Dialogs.ValidationIssue;

namespace ModSync.Tests
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
        public void AddPipelineStageIssues_EnvironmentFailure_WithPrefixedMessage_AddsParsedIssueOnly()
        {
            var pipelineResult = new ValidationPipelineResult();
            var environment = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Environment,
                Passed = false,
                Summary = "HoloPatcher missing",
            };
            environment.Messages.Add("ERROR: HoloPatcher missing");
            pipelineResult.Stages.Add(environment);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(1));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Unknown"));
            Assert.That(modIssues[0].IssueType, Is.EqualTo("Environment"));
            Assert.That(modIssues[0].Description, Is.EqualTo("HoloPatcher missing"));
        }

        [Test]
        public void AddPipelineStageIssues_InstallOrderFailure_AddsPrefixedAndAggregateIssues()
        {
            var pipelineResult = new ValidationPipelineResult();
            var order = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.InstallOrder,
                Passed = false,
                Summary = "Circular dependency: Mod A -> Mod B",
            };
            order.Messages.Add("ERROR: Circular dependency detected");
            pipelineResult.Stages.Add(order);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(2));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Unknown"));
            Assert.That(modIssues[0].IssueType, Is.EqualTo("InstallOrder"));
            Assert.That(modIssues[1].ModName, Is.EqualTo("Install Order"));
        }

        [Test]
        public void AddPipelineStageIssues_ArchiveError_ParsesModName()
        {
            var pipelineResult = new ValidationPipelineResult();
            var archives = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = false,
                Summary = "1 component error(s)",
            };
            archives.Messages.Add("ERROR: Test Mod: missing archive");
            pipelineResult.Stages.Add(archives);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(2));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Test Mod"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("✗"));
            Assert.That(modIssues[0].IssueType, Is.EqualTo("ArchiveValidation"));
            Assert.That(modIssues[1].ModName, Is.EqualTo("Archive Validation"));
            Assert.That(modIssues[1].Description, Is.EqualTo("1 component error(s)"));
        }

        [Test]
        public void AddPipelineStageIssues_ArchiveWarning_ParsesModName()
        {
            var pipelineResult = new ValidationPipelineResult();
            var archives = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = true,
                HasWarnings = true,
                Summary = "1 warning(s)",
            };
            archives.Messages.Add("WARNING: Mod B: stale archive");
            pipelineResult.Stages.Add(archives);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(2));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Mod B"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("⚠"));
            Assert.That(modIssues[1].ModName, Is.EqualTo("Archive Validation"));
            Assert.That(modIssues[1].Description, Is.EqualTo("1 warning(s)"));
        }

        [Test]
        public void AddPipelineStageIssues_ConflictWarning_ParsesModName()
        {
            var pipelineResult = new ValidationPipelineResult();
            var conflicts = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Conflicts,
                Passed = true,
                HasWarnings = true,
                Summary = "1 dependency warning(s)",
            };
            conflicts.Messages.Add("WARNING: Mod A: conflicts with Mod B");
            pipelineResult.Stages.Add(conflicts);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(2));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Mod A"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("⚠"));
            Assert.That(modIssues[1].ModName, Is.EqualTo("Conflicts"));
            Assert.That(modIssues[1].Description, Is.EqualTo("1 dependency warning(s)"));
        }

        [Test]
        public void AddPipelineStageIssues_ConflictFailure_AddsModAndAggregateIssues()
        {
            var pipelineResult = new ValidationPipelineResult();
            var conflicts = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.Conflicts,
                Passed = false,
                Summary = "1 restriction conflict(s)",
            };
            conflicts.Messages.Add("ERROR: Mod X: incompatible with: Mod Y");
            pipelineResult.Stages.Add(conflicts);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(2));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Mod X"));
            Assert.That(modIssues[0].Icon, Is.EqualTo("✗"));
            Assert.That(modIssues[1].ModName, Is.EqualTo("Conflicts"));
            Assert.That(modIssues[1].IssueType, Is.EqualTo("Conflict"));
        }

        [Test]
        public void AddPipelineStageIssues_FomodConfigurationFailure_MapsPrefixedRows()
        {
            var pipelineResult = new ValidationPipelineResult();
            var fomod = new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.FomodConfiguration,
                Passed = false,
                Summary = "1 unconfigured FOMOD archive(s).",
            };
            fomod.Messages.Add("ERROR: Sample Mod: FOMOD archive 'pack.zip' is not configured.");
            pipelineResult.Stages.Add(fomod);

            var modIssues = new List<DialogValidationIssue>();
            ValidationPipelineDialogMapper.AddPipelineStageIssues(pipelineResult, modIssues);

            Assert.That(modIssues, Has.Count.EqualTo(1));
            Assert.That(modIssues[0].ModName, Is.EqualTo("Sample Mod"));
            Assert.That(modIssues[0].IssueType, Is.EqualTo(FomodConfigurationGate.IssueCategory));
            Assert.That(modIssues[0].Solution, Is.EqualTo(FomodConfigurationGate.RecoveryHint));
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
