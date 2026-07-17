// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using ModSync.Core.Services.Settings;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ModSyncSettingsTests
    {
        private string _settingsDir;

        [SetUp]
        public void SetUp()
        {
            _settingsDir = Path.Combine(Path.GetTempPath(), "ModSync_SettingsTests_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(_settingsDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_settingsDir))
            {
                Directory.Delete(_settingsDir, recursive: true);
            }
        }

        [Test]
        public void SaveManagedDeploymentFields_RoundTripsManagedAndActiveProfile()
        {
            var settings = new ModSyncSettings
            {
                ManagedDeploymentEnabled = true,
                ActiveProfileName = "Full Build",
            };

            settings.SaveManagedDeploymentFieldsToDirectory(_settingsDir);

            ModSyncSettings loaded = ModSyncSettings.LoadFromDirectory(_settingsDir);
            Assert.That(loaded.ManagedDeploymentEnabled, Is.True);
            Assert.That(loaded.ActiveProfileName, Is.EqualTo("Full Build"));
        }

        [Test]
        public void SaveManagedDeploymentFields_PreservesUnrelatedKeys()
        {
            string settingsPath = Path.Combine(_settingsDir, "settings.json");
            File.WriteAllText(settingsPath, "{\"debugLogging\":true,\"theme\":\"/Styles/LightStyle.axaml\"}");

            var settings = new ModSyncSettings
            {
                ManagedDeploymentEnabled = true,
                ActiveProfileName = "Test",
            };
            settings.SaveManagedDeploymentFieldsToDirectory(_settingsDir);

            string json = File.ReadAllText(settingsPath);
            Assert.That(json, Does.Contain("debugLogging"));
            Assert.That(json, Does.Contain("managedDeploymentEnabled"));
            Assert.That(json, Does.Contain("activeProfileName"));
        }
    }
}
