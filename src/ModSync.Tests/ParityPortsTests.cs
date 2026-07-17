// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ModSync.Core;
using ModSync.Core.Ports.Conflicts;
using ModSync.Core.Ports.Download;
using ModSync.Core.Ports.Guides;
using ModSync.Core.Ports.Installation;
using ModSync.Core.Ports.Profiles;
using ModSync.Core.Ports.Protocol;
using ModSync.Core.Ports.Updates;
using ModSync.Core.Services;
using ModSync.Core.Services.Conflicts;
using ModSync.Core.Services.Deployment;
using ModSync.Core.Services.Download;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Protocol;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ParityPortsTests
    {
        [Test]
        public void DownloadProviderRegistry_ExposesHandlersInPriorityOrder()
        {
            IDownloadProviderRegistry registry = DownloadHandlerFactory.CreateProviderRegistry();
            IReadOnlyList<string> keys = registry.GetProviderKeys();

            Assert.That(keys, Does.Contain("direct"));
            Assert.That(keys[keys.Count - 1], Is.EqualTo("direct"), "DirectDownloadHandler must remain last");
            Assert.That(registry.GetHandlerForUrl("https://example.com/file.zip"), Is.Not.Null);
            Assert.That(registry.GetHandlerForUrl("not-a-url"), Is.Null);
        }

        [Test]
        public void InstallBackendSelector_DefaultsToClassicWhenManagedOff()
        {
            IInstallBackend backend = InstallBackendSelector.Instance.Select(
                managedDeploymentEnabled: false,
                gameDirectory: "/tmp/game",
                stagingRoot: "/tmp/stage",
                manifestRoot: "/tmp/manifest");

            Assert.That(backend.Kind, Is.EqualTo(InstallBackendKind.ClassicInstructions));
        }

        [Test]
        public void InstallBackendSelector_RequiresRootsForManaged()
        {
            IInstallBackend missingRoots = InstallBackendSelector.Instance.Select(
                managedDeploymentEnabled: true,
                gameDirectory: null,
                stagingRoot: "/tmp/stage",
                manifestRoot: "/tmp/manifest");

            Assert.That(missingRoots.Kind, Is.EqualTo(InstallBackendKind.ClassicInstructions));
        }

        [Test]
        public async Task InstallBackendSelector_ManagedWrapsDeploymentService()
        {
            string root = Path.Combine(Path.GetTempPath(), "modsync-parity-backend-" + Guid.NewGuid().ToString("N"));
            string game = Path.Combine(root, "game");
            string stage = Path.Combine(root, "stage");
            string manifests = Path.Combine(root, "manifests-root");
            string stagedComponent = Path.Combine(stage, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(game);
            Directory.CreateDirectory(stagedComponent);
            File.WriteAllText(Path.Combine(stagedComponent, "override.txt"), "hello");

            try
            {
                IInstallBackend backend = InstallBackendSelector.Instance.Select(
                    managedDeploymentEnabled: true,
                    gameDirectory: game,
                    stagingRoot: stage,
                    manifestRoot: manifests);

                Assert.That(backend.Kind, Is.EqualTo(InstallBackendKind.ManagedDeployment));

                var guid = Guid.NewGuid();
                DeploymentManifest manifest = await backend.DeployComponentAsync(
                    guid,
                    "TestComponent",
                    stagedComponent);

                Assert.That(manifest, Is.Not.Null);
                Assert.That(manifest.Entries.Count, Is.EqualTo(1));
                Assert.That(File.Exists(Path.Combine(game, "override.txt")), Is.True);

                await backend.UninstallComponentAsync(guid);
                Assert.That(File.Exists(Path.Combine(game, "override.txt")), Is.False);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public async Task ProtocolHandlerRegistry_AcceptsNxmAndModSync()
        {
            IProtocolHandlerRegistry registry = ProtocolHandlerRegistry.CreateDefault();

            ProtocolHandleResult nxm = await registry.HandleAsync(
                "nxm://kotor/mods/1/files/2?key=abc&expires=9999999999&user_id=1");
            Assert.That(nxm.Accepted, Is.True);
            Assert.That(nxm.Scheme, Is.EqualTo("nxm"));
            Assert.That(nxm.Payload, Is.InstanceOf<NxmUrl>());

            ProtocolHandleResult modSync = await registry.HandleAsync(
                "modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml&game=kotor");
            Assert.That(modSync.Accepted, Is.True);
            Assert.That(modSync.Scheme, Is.EqualTo("modsync"));
            Assert.That(modSync.Payload, Is.InstanceOf<ModSyncUrl>());

            ProtocolHandleResult unknown = await registry.HandleAsync("https://example.com/");
            Assert.That(unknown.Accepted, Is.False);
        }

        [Test]
        public void ProfileStore_RoundTripsViaInterface()
        {
            string root = Path.Combine(Path.GetTempPath(), "modsync-parity-profiles-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            try
            {
                IProfileStore store = new ProfileService(root);
                Profile created = store.CreateProfile("ParityTest");
                Assert.That(created.Name, Is.EqualTo("ParityTest"));

                Profile loaded = store.LoadProfile("ParityTest");
                Assert.That(loaded, Is.Not.Null);
                Assert.That(store.ListProfiles().Select(p => p.Name), Does.Contain("ParityTest"));
                Assert.That(store.DeleteProfile("ParityTest"), Is.True);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        [Test]
        public void GuideIngestAndEmit_RoundTripTomlSmoke()
        {
            const string toml = @"
[[thisMod]]
name = ""Parity Guide Component""
guid = ""{11111111-1111-1111-1111-111111111111}""
";

            GuideIngestResult ingested = GuideIngestService.Instance.IngestFromText(toml, formatHint: "toml");
            Assert.That(ingested.Components.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(ingested.Components[0].Name, Is.EqualTo("Parity Guide Component"));

            string markdown = GuideEmitService.Instance.EmitMarkdown(ingested.Components);
            Assert.That(markdown, Does.Contain("Parity Guide Component"));
        }

        [Test]
        public void ConflictAnalyzer_IsExposedAsPort()
        {
            IConflictAnalyzer analyzer = new FileConflictAnalyzer();
            Assert.That(analyzer, Is.Not.Null);
        }

        [Test]
        public void UpdateCheckResultStore_RoundTripsSnapshot()
        {
            string root = Path.Combine(Path.GetTempPath(), "modsync-parity-updates-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            try
            {
                IUpdateCheckResultStore store = new JsonUpdateCheckResultStore(root);
                var result = new ModUpdateCheckResult
                {
                    CheckedCount = 3,
                    SkippedCount = 1,
                    RateLimitReached = false,
                };
                result.UpdatesFound.Add(new ModUpdateInfo
                {
                    ComponentName = "Demo",
                    Url = "https://www.nexusmods.com/kotor/mods/1",
                    InstalledVersion = "1.0",
                    LatestVersion = "1.1",
                });
                result.Errors.Add("kotor/2: boom");

                store.Save(result, new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc));
                PersistedUpdateCheckSnapshot loaded = store.Load();

                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded.CheckedCount, Is.EqualTo(3));
                Assert.That(loaded.SkippedCount, Is.EqualTo(1));
                Assert.That(loaded.UpdatesFound.Count, Is.EqualTo(1));
                Assert.That(loaded.UpdatesFound[0].LatestVersion, Is.EqualTo("1.1"));
                Assert.That(loaded.Errors, Does.Contain("kotor/2: boom"));
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }
    }
}
