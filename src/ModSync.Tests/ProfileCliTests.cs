// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

using ModSync.Core.CLI;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Settings;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ProfileCliTests
    {
        private string _settingsDirectory;

        [SetUp]
        public void SetUp()
        {
            _settingsDirectory = Path.Combine(Path.GetTempPath(), "ModSync_ProfileCli_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_settingsDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_settingsDirectory))
                {
                    Directory.Delete(_settingsDirectory, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp cleanup.
            }
        }

        [Test]
        public void ProfileCli_CreateListDelete_Succeeds()
        {
            int createExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "create",
                "--name", "Agent Smoke",
                "--settings-dir", _settingsDirectory,
            });

            Assert.That(createExit, Is.EqualTo(0));

            var service = new ProfileService(_settingsDirectory);
            Assert.That(service.ListProfiles().Select(p => p.Name), Does.Contain("Agent Smoke"));

            int deleteExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "delete",
                "--name", "Agent Smoke",
                "--settings-dir", _settingsDirectory,
            });

            Assert.That(deleteExit, Is.EqualTo(0));
            Assert.That(service.ListProfiles(), Is.Empty);
        }

        [Test]
        public void ProfileCli_Activate_PersistsActiveProfileName()
        {
            int createExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "create",
                "--name", "Full Build",
                "--settings-dir", _settingsDirectory,
            });
            Assert.That(createExit, Is.EqualTo(0));

            int activateExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "activate",
                "--name", "Full Build",
                "--settings-dir", _settingsDirectory,
            });

            Assert.That(activateExit, Is.EqualTo(0));
            ModSyncSettings settings = ModSyncSettings.LoadFromDirectory(_settingsDirectory);
            Assert.That(settings.ActiveProfileName, Is.EqualTo("Full Build"));
        }

        [Test]
        public void ProfileCli_CloneAndRename_Succeeds()
        {
            ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "create",
                "--name", "Alpha",
                "--settings-dir", _settingsDirectory,
            });

            int cloneExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "clone",
                "--from", "Alpha",
                "--to", "Beta",
                "--settings-dir", _settingsDirectory,
            });
            Assert.That(cloneExit, Is.EqualTo(0));

            int renameExit = ModBuildConverter.Run(new[]
            {
                "profile",
                "--action", "rename",
                "--from", "Beta",
                "--to", "Gamma",
                "--settings-dir", _settingsDirectory,
            });
            Assert.That(renameExit, Is.EqualTo(0));

            var service = new ProfileService(_settingsDirectory);
            Assert.That(service.LoadProfile("Gamma"), Is.Not.Null);
            Assert.That(service.LoadProfile("Beta"), Is.Null);
        }
    }
}
