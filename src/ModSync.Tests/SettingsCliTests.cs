// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;

using ModSync.Core.CLI;
using ModSync.Core.Services.Settings;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class SettingsCliTests
    {
        private string _settingsDirectory;

        [SetUp]
        public void SetUp()
        {
            _settingsDirectory = Path.Combine(Path.GetTempPath(), "ModSync_SettingsCli_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_settingsDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_settingsDirectory))
            {
                Directory.Delete(_settingsDirectory, recursive: true);
            }
        }

        [Test]
        public void SettingsCli_SetGetList_PreservesUnrelatedKeys()
        {
            string settingsPath = Path.Combine(_settingsDirectory, "settings.json");
            File.WriteAllText(settingsPath, "{\"theme\":\"/Styles/LightStyle.axaml\",\"debugLogging\":true}");

            int setExit = ModBuildConverter.Run(new[]
            {
                "settings",
                "--action", "set",
                "--key", "managedDeploymentEnabled",
                "--value", "true",
                "--settings-dir", _settingsDirectory,
            });
            Assert.That(setExit, Is.EqualTo(0));

            int getExit = ModBuildConverter.Run(new[]
            {
                "settings",
                "--action", "get",
                "--key", "managedDeploymentEnabled",
                "--settings-dir", _settingsDirectory,
            });
            Assert.That(getExit, Is.EqualTo(0));

            string json = File.ReadAllText(settingsPath);
            Assert.That(json, Does.Contain("theme"));
            Assert.That(json, Does.Contain("debugLogging"));
            Assert.That(json, Does.Contain("managedDeploymentEnabled"));

            ModSyncSettings loaded = ModSyncSettings.LoadFromDirectory(_settingsDirectory);
            Assert.That(loaded.ManagedDeploymentEnabled, Is.True);
        }

        [Test]
        public void SettingsCli_List_RedactsNexusApiKeyByDefault()
        {
            File.WriteAllText(
                Path.Combine(_settingsDirectory, "settings.json"),
                "{\"nexusModsApiKey\":\"super-secret\",\"sourcePath\":\"/tmp/mods\"}");

            int exitCode = RunWithCapturedStdout(new[]
            {
                "settings",
                "--action", "list",
                "--json",
                "--settings-dir", _settingsDirectory,
            }, out string stdout);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(stdout, Does.Not.Contain("super-secret"));
                Assert.That(stdout, Does.Contain("***"));
            });
        }

        [Test]
        public void SettingsCli_Set_RemovesKeyWhenValueEmpty()
        {
            File.WriteAllText(
                Path.Combine(_settingsDirectory, "settings.json"),
                "{\"activeProfileName\":\"Old Profile\"}");

            int exitCode = ModBuildConverter.Run(new[]
            {
                "settings",
                "--action", "set",
                "--key", "activeProfileName",
                "--value", "",
                "--settings-dir", _settingsDirectory,
            });

            Assert.That(exitCode, Is.EqualTo(0));
            ModSyncSettings loaded = ModSyncSettings.LoadFromDirectory(_settingsDirectory);
            Assert.That(loaded.ActiveProfileName, Is.Null.Or.Empty);
        }

        [Test]
        public void SettingsFileStore_ParseCliValue_ParsesBooleansAndNumbers()
        {
            Assert.Multiple(() =>
            {
                Assert.That(SettingsFileStore.ParseCliValue("true").GetValue<bool>(), Is.True);
                Assert.That(SettingsFileStore.ParseCliValue("42").GetValue<long>(), Is.EqualTo(42));
                Assert.That(SettingsFileStore.ParseCliValue("hello").GetValue<string>(), Is.EqualTo("hello"));
            });
        }

        private static int RunWithCapturedStdout(string[] args, out string stdout)
        {
            var writer = new StringWriter();
            TextWriter previousOut = Console.Out;
            Console.SetOut(writer);
            try
            {
                return ModBuildConverter.Run(args);
            }
            finally
            {
                Console.SetOut(previousOut);
                stdout = writer.ToString();
            }
        }
    }
}
