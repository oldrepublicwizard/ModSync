// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using ModSync.Core;
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
