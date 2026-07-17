// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Ports.Installation;
using ModSync.Core.Services.Deployment;
using ModSync.Core.Services.Installation;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Settings;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ManagedInstallSessionTests
    {
        private string _storageDir;
        private string _gameDir;
        private ProfileService _profileService;

        [SetUp]
        public void SetUp()
        {
            _storageDir = Path.Combine(Path.GetTempPath(), "ModSync_ManagedSession_" + Guid.NewGuid().ToString("N"));
            _gameDir = Path.Combine(_storageDir, "game");
            _ = Directory.CreateDirectory(_gameDir);
            _profileService = new ProfileService(_storageDir);
            MainConfig.Instance.destinationPath = new DirectoryInfo(_gameDir);
        }

        [TearDown]
        public void TearDown()
        {
            ManagedInstallSession.Current = null;
            if (Directory.Exists(_storageDir))
            {
                Directory.Delete(_storageDir, recursive: true);
            }
        }

        [Test]
        public void TryCreate_ReturnsNullWhenManagedDisabled()
        {
            _profileService.CreateProfile("Alpha");
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = false, ActiveProfileName = "Alpha" };

            ManagedInstallSession session = ManagedInstallSession.TryCreate(settings, _profileService);
            Assert.That(session, Is.Null);
        }

        [Test]
        public void TryCreate_ThrowsWhenManagedEnabledWithoutActiveProfile()
        {
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = true };

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => ManagedInstallSession.TryCreate(settings, _profileService));
            Assert.That(ex.Message, Does.Contain("no active profile"));
        }

        [Test]
        public void TryCreate_BuildsArtifactPathsUnderProfileDirectory()
        {
            _profileService.CreateProfile("Alpha");
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = true, ActiveProfileName = "Alpha" };

            ManagedInstallSession session = ManagedInstallSession.TryCreate(settings, _profileService);
            Assert.That(session, Is.Not.Null);
            Assert.That(session.BackendKind, Is.EqualTo(InstallBackendKind.ManagedDeployment));

            string artifactDir = _profileService.GetProfileArtifactDirectory("Alpha");
            Assert.That(Directory.Exists(Path.Combine(artifactDir, "staging")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(artifactDir, "deployment")), Is.True);
        }

        [Test]
        public void ShouldStageAction_OnlyIncludesFileOperationActions()
        {
            Assert.That(ManagedInstallSession.ShouldStageAction(Instruction.ActionType.Extract), Is.True);
            Assert.That(ManagedInstallSession.ShouldStageAction(Instruction.ActionType.Patcher), Is.False);
            Assert.That(ManagedInstallSession.ShouldStageAction(Instruction.ActionType.Delete), Is.False);
        }

        [Test]
        public void MapGamePathToStaging_PlacesFilesUnderComponentGuid()
        {
            _profileService.CreateProfile("Alpha");
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = true, ActiveProfileName = "Alpha" };
            ManagedInstallSession session = ManagedInstallSession.TryCreate(settings, _profileService);

            Guid componentGuid = Guid.NewGuid();
            string gameOverride = Path.Combine(_gameDir, "Override");
            string staged = session.MapGamePathToStaging(componentGuid, gameOverride);

            Assert.That(staged, Does.Contain(componentGuid.ToString()));
            Assert.That(staged, Does.EndWith(Path.Combine("Override")));
        }

        [Test]
        public void TryCreate_UsesInstallBackendSelectorForManagedKind()
        {
            _profileService.CreateProfile("Alpha");
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = true, ActiveProfileName = "Alpha" };
            var fakeBackend = new RecordingInstallBackend();
            var selector = new FixedInstallBackendSelector(fakeBackend);

            ManagedInstallSession session = ManagedInstallSession.TryCreate(
                settings,
                _profileService,
                backendSelector: selector);

            Assert.That(session.BackendKind, Is.EqualTo(InstallBackendKind.ManagedDeployment));
            Assert.That(session.InstallBackend, Is.SameAs(fakeBackend));
            Assert.That(selector.SelectCallCount, Is.EqualTo(1));
            Assert.That(selector.LastManagedEnabled, Is.True);
        }

        [Test]
        public async Task DeployComponentAsync_InvokesManagedInstallBackend()
        {
            _profileService.CreateProfile("Alpha");
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = true, ActiveProfileName = "Alpha" };
            var fakeBackend = new RecordingInstallBackend();
            var selector = new FixedInstallBackendSelector(fakeBackend);

            ManagedInstallSession session = ManagedInstallSession.TryCreate(
                settings,
                _profileService,
                backendSelector: selector);

            Assert.That(session, Is.Not.Null);

            Guid componentGuid = Guid.NewGuid();
            var component = new ModComponent
            {
                Guid = componentGuid,
                Name = "RecordingMod",
            };

            var instruction = new Instruction
            {
                Action = Instruction.ActionType.Copy,
                Destination = "<<kotorDirectory>>/Override",
            };
            instruction.SetParentComponent(component);
            instruction.RedirectResolvedDestination(Path.Combine(_gameDir, "Override"));
            session.ApplyStagingRedirect(instruction, componentGuid);

            Assert.That(session.ComponentHadStagedOperations(componentGuid), Is.True);

            // Leave at least one staged entry so deploy is attempted.
            string stagedDir = session.GetComponentStagingDirectory(componentGuid);
            Directory.CreateDirectory(Path.Combine(stagedDir, "Override"));
            File.WriteAllText(Path.Combine(stagedDir, "Override", "file.txt"), "payload");

            ModComponent.InstallExitCode exitCode = await session.DeployComponentAsync(component);

            Assert.That(exitCode, Is.EqualTo(ModComponent.InstallExitCode.Success));
            Assert.That(fakeBackend.DeployCallCount, Is.EqualTo(1));
            Assert.That(fakeBackend.LastComponentGuid, Is.EqualTo(componentGuid));
            Assert.That(fakeBackend.LastStagedDirectory, Is.EqualTo(stagedDir));
            Assert.That(session.ManifestsWritten, Is.EqualTo(1));
        }

        private sealed class RecordingInstallBackend : IInstallBackend
        {
            public InstallBackendKind Kind => InstallBackendKind.ManagedDeployment;

            public int DeployCallCount { get; private set; }

            public Guid LastComponentGuid { get; private set; }

            [CanBeNull]
            public string LastStagedDirectory { get; private set; }

            public Task<DeploymentManifest> DeployComponentAsync(
                Guid componentGuid,
                string componentName,
                string stagedDirectory,
                CancellationToken cancellationToken = default)
            {
                DeployCallCount++;
                LastComponentGuid = componentGuid;
                LastStagedDirectory = stagedDirectory;
                return Task.FromResult(new DeploymentManifest
                {
                    ComponentGuid = componentGuid,
                    ComponentName = componentName,
                });
            }

            public Task UninstallComponentAsync(Guid componentGuid, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;

            public Task PurgeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class FixedInstallBackendSelector : IInstallBackendSelector
        {
            [NotNull]
            private readonly IInstallBackend _backend;

            public FixedInstallBackendSelector([NotNull] IInstallBackend backend)
            {
                _backend = backend;
            }

            public int SelectCallCount { get; private set; }

            public bool LastManagedEnabled { get; private set; }

            public IInstallBackend Select(
                bool managedDeploymentEnabled,
                string gameDirectory,
                string stagingRoot,
                string manifestRoot)
            {
                SelectCallCount++;
                LastManagedEnabled = managedDeploymentEnabled;
                return _backend;
            }
        }
    }
}
