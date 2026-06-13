// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ModSync.Core;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodPostDownloadOrchestratorTests
    {
        private sealed class RecordingHost : IFomodPostDownloadHost
        {
            public List<string> WarnedArchives { get; } = new List<string>();

            public List<string> ConfiguredArchives { get; } = new List<string>();

            public FomodConfigurePromptResult NextPromptResult { get; set; } = FomodConfigurePromptResult.AlreadyHandled;

            public Task<FomodConfigurePromptResult> AskConfigureAsync(
                FomodPromptContext context,
                CancellationToken cancellationToken = default)
            {
                WarnedArchives.Add(context.ArchiveFileName);
                return Task.FromResult(NextPromptResult);
            }

            public Task<ModComponent> RunWizardAsync(
                string extractedArchiveDirectory,
                FomodPromptContext context,
                CancellationToken cancellationToken = default) =>
                Task.FromResult<ModComponent>(null);

            public Task ReportExtractFailureAsync(
                FomodPromptContext context,
                string message,
                CancellationToken cancellationToken = default) =>
                Task.CompletedTask;

            public Task ReportConfiguredAsync(
                FomodPromptContext context,
                CancellationToken cancellationToken = default)
            {
                ConfiguredArchives.Add(context.ArchiveFileName);
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task ProcessAsync_SkipsDeselectedComponents()
        {
            var host = new RecordingHost();
            var component = new ModComponent { Name = "Hidden Mod", IsSelected = false };
            component.ResourceRegistry = new Dictionary<string, ResourceMetadata>
            {
                ["https://example.test/mod.zip"] = new ResourceMetadata
                {
                    Files = new Dictionary<string, bool?> { ["Example.zip"] = true },
                },
            };

            await FomodPostDownloadOrchestrator.ProcessAsync(
                new[] { component },
                TestContext.CurrentContext.WorkDirectory,
                host).ConfigureAwait(false);

            Assert.That(host.WarnedArchives, Is.Empty);
        }

        [Test]
        public async Task ProcessAsync_Dismiss_MarksDismissed()
        {
            string tempDir = System.IO.Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tempDir);

            string archivePath = System.IO.Path.Combine(tempDir, "fomod-test.zip");
            CreateZipWithFomod(archivePath);

            var host = new RecordingHost { NextPromptResult = FomodConfigurePromptResult.Dismiss };
            ModComponent component = BuildComponentWithArchive("fomod-test.zip");

            await FomodPostDownloadOrchestrator.ProcessAsync(new[] { component }, tempDir, host).ConfigureAwait(false);

            Assert.That(FomodDownloadPromptState.GetStatus(component, "fomod-test.zip"), Is.EqualTo(FomodDownloadPromptState.StatusDismissed));
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
            string staging = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(archivePath), "staging");
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(staging, "fomod"));
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(staging, "fomod", "ModuleConfig.xml"),
                "<config><installSteps order=\"Explicit\"></installSteps></config>");

            System.IO.Compression.ZipFile.CreateFromDirectory(staging, archivePath);
            System.IO.Directory.Delete(staging, true);
        }
    }
}
