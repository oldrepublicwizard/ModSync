// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using ModSync.Core;
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
    }
}
