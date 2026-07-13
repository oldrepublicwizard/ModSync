// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Core.Services.Fomod;
using ModSync.Core.Services.Validation;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class FomodConfigurationGateTests
    {
        private string _modDir;

        [SetUp]
        public void SetUp()
        {
            _modDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_modDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_modDir))
            {
                Directory.Delete(_modDir, recursive: true);
            }
        }

        [Test]
        public void Validate_UnconfiguredFomodArchive_Fails()
        {
            string archiveName = "needs-config.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(1));
            Assert.That(result.Issues[0].ArchiveFileName, Is.EqualTo(archiveName));
        }

        [Test]
        public void Validate_ConfiguredArchive_Passes()
        {
            string archiveName = "configured.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);
            FomodDownloadPromptState.MarkConfigured(component, archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.True);
        }

        [Test]
        public void Validate_DismissedArchive_StillFails()
        {
            string archiveName = "dismissed.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);
            FomodDownloadPromptState.MarkDismissed(component, archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.Issues[0].PromptStatus, Is.EqualTo(FomodDownloadPromptState.StatusDismissed));
        }

        [Test]
        public void Validate_WarnedArchive_StillFails()
        {
            string archiveName = "warned.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);
            FomodDownloadPromptState.MarkWarned(component, archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.Issues[0].PromptStatus, Is.EqualTo(FomodDownloadPromptState.StatusWarned));
        }

        [Test]
        public void ExpandWithHardDependencies_IncludesDependencyChain()
        {
            var dependency = new ModComponent { Guid = Guid.NewGuid(), Name = "Dep" };
            var selected = new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Selected",
                IsSelected = true,
                Dependencies = new List<Guid> { dependency.Guid },
            };

            List<ModComponent> expanded = FomodConfigurationGate.ExpandWithHardDependencies(
                new[] { selected },
                new[] { dependency, selected });

            Assert.That(expanded, Has.Count.EqualTo(2));
            Assert.That(expanded, Does.Contain(dependency));
            Assert.That(expanded, Does.Contain(selected));
        }

        [Test]
        public async Task Pipeline_FomodGate_BlocksWhenArchiveUnconfigured()
        {
            string archiveName = "pipeline-gate.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);

            MainConfig.Instance = new MainConfig
            {
                sourcePath = new DirectoryInfo(_modDir),
                destinationPath = new DirectoryInfo(_modDir),
            };

            var options = ValidationPipelineOptions.WizardFull;
            options.SkipEnvironmentValidation = true;
            options.SkipComponentArchiveValidation = true;
            options.DryRun = false;

            ValidationPipelineResult result = await InstallationValidationPipeline.RunAsync(
                new[] { component },
                options).ConfigureAwait(false);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Stages.Exists(s => s.Stage == ValidationPipelineStage.FomodConfiguration && !s.Passed),
                Is.True);
        }

        [Test]
        public async Task Pipeline_FomodGate_PassesWhenArchiveConfigured()
        {
            string archiveName = "pipeline-configured.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);
            FomodDownloadPromptState.MarkConfigured(component, archiveName);

            MainConfig.Instance = new MainConfig
            {
                sourcePath = new DirectoryInfo(_modDir),
                destinationPath = new DirectoryInfo(_modDir),
            };

            var options = ValidationPipelineOptions.WizardFull;
            options.SkipEnvironmentValidation = true;
            options.SkipComponentArchiveValidation = true;
            options.DryRun = false;

            ValidationPipelineResult result = await InstallationValidationPipeline.RunAsync(
                new[] { component },
                options).ConfigureAwait(false);

            ValidationPipelineStageResult fomodStage = result.Stages.Find(
                s => s.Stage == ValidationPipelineStage.FomodConfiguration);
            Assert.That(fomodStage, Is.Not.Null);
            Assert.That(fomodStage.Passed, Is.True);
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task InstallAllSelected_BlocksWhenArchiveUnconfigured()
        {
            string archiveName = "install-gate.zip";
            CreateZipWithFomod(Path.Combine(_modDir, archiveName));
            ModComponent component = BuildComponentWithArchive(archiveName);

            MainConfig previous = MainConfig.Instance;
            MainConfig.Instance = new MainConfig
            {
                sourcePath = new DirectoryInfo(_modDir),
                destinationPath = new DirectoryInfo(_modDir),
            };

            try
            {
                ModComponent.InstallExitCode exitCode =
                    await InstallationService.InstallAllSelectedComponentsAsync(
                        new List<ModComponent> { component },
                        progressCallback: null,
                        System.Threading.CancellationToken.None).ConfigureAwait(false);

                Assert.That(exitCode, Is.EqualTo(ModComponent.InstallExitCode.InvalidOperation));
            }
            finally
            {
                MainConfig.Instance = previous;
            }
        }

        [Test]
        public async Task Pipeline_FomodGate_FailsClosedWhenModDirectoryMissing()
        {
            ModComponent component = BuildComponentWithArchive("missing-dir.zip");

            MainConfig.Instance = new MainConfig
            {
                sourcePath = null,
                destinationPath = new DirectoryInfo(_modDir),
            };

            var options = ValidationPipelineOptions.WizardFull;
            options.SkipEnvironmentValidation = true;
            options.SkipComponentArchiveValidation = true;
            options.DryRun = false;

            ValidationPipelineResult result = await InstallationValidationPipeline.RunAsync(
                new[] { component },
                options).ConfigureAwait(false);

            Assert.That(result.IsSuccess, Is.False);
            ValidationPipelineStageResult fomodStage = result.Stages.Find(
                s => s.Stage == ValidationPipelineStage.FomodConfiguration);
            Assert.That(fomodStage, Is.Not.Null);
            Assert.That(fomodStage.Passed, Is.False);
        }

        [Test]
        public void ExpandWithHardDependencies_DuplicateGuids_DoesNotThrow()
        {
            Guid shared = Guid.NewGuid();
            var first = new ModComponent { Guid = shared, Name = "First" };
            var duplicate = new ModComponent { Guid = shared, Name = "Duplicate" };

            Assert.DoesNotThrow(() =>
            {
                List<ModComponent> expanded = FomodConfigurationGate.ExpandWithHardDependencies(
                    new[] { first },
                    new[] { first, duplicate });
                Assert.That(expanded, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void Validate_UnreadableDownloadedArchive_FailsClosed()
        {
            string archiveName = "corrupt.zip";
            File.WriteAllText(Path.Combine(_modDir, archiveName), "not a zip archive");
            ModComponent component = BuildComponentWithArchive(archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.Issues, Has.Count.EqualTo(1));
            Assert.That(result.Issues[0].ArchiveUnreadable, Is.True);
            Assert.That(
                FomodConfigurationGate.FormatIssueMessage(result.Issues[0]),
                Does.Contain("could not be inspected"));
        }

        [Test]
        public void Validate_ReadableNonFomodArchive_Passes()
        {
            string archiveName = "plain.zip";
            string staging = Path.Combine(_modDir, "plain-staging");
            Directory.CreateDirectory(staging);
            File.WriteAllText(Path.Combine(staging, "readme.txt"), "no fomod here");
            ZipFile.CreateFromDirectory(staging, Path.Combine(_modDir, archiveName));
            Directory.Delete(staging, recursive: true);

            ModComponent component = BuildComponentWithArchive(archiveName);

            FomodConfigurationGate.GateResult result = FomodConfigurationGate.Validate(
                new[] { component },
                new[] { component },
                _modDir);

            Assert.That(result.Passed, Is.True);
        }

        private static ModComponent BuildComponentWithArchive(string archiveFileName)
        {
            var component = new ModComponent { Name = "Test Mod", IsSelected = true };
            component.ResourceRegistry = new Dictionary<string, ResourceMetadata>
            {
                ["https://example.test/mod.zip"] = new ResourceMetadata
                {
                    Files = new Dictionary<string, bool?> { [archiveFileName] = true },
                    HandlerMetadata = new Dictionary<string, object>(),
                },
            };
            return component;
        }

        private static void CreateZipWithFomod(string archivePath)
        {
            string staging = Path.Combine(Path.GetDirectoryName(archivePath), "staging");
            Directory.CreateDirectory(Path.Combine(staging, "fomod"));
            File.WriteAllText(
                Path.Combine(staging, "fomod", "ModuleConfig.xml"),
                "<config><installSteps order=\"Explicit\"></installSteps></config>");

            ZipFile.CreateFromDirectory(staging, archivePath);
            Directory.Delete(staging, recursive: true);
        }
    }
}
