// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;

using ModSync.Models;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class SettingsManagerLegacyPathTests
    {
        private string _appDataRoot = string.Empty;
        private string _modSyncSettingsPath = string.Empty;
        private string _legacySettingsPath = string.Empty;
        private string _modSyncSettingsBackup = string.Empty;
        private string _legacySettingsBackup = string.Empty;
        private bool _hadModSyncSettings;
        private bool _hadLegacySettings;

        [SetUp]
        public void SetUp()
        {
            _appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _modSyncSettingsPath = Path.Combine(_appDataRoot, "ModSync", "settings.json");
            _legacySettingsPath = Path.Combine(_appDataRoot, "KOTORModSync", "settings.json");
            _modSyncSettingsBackup = _modSyncSettingsPath + ".modsync-test-bak";
            _legacySettingsBackup = _legacySettingsPath + ".modsync-test-bak";

            _hadModSyncSettings = File.Exists(_modSyncSettingsPath);
            if (_hadModSyncSettings)
            {
                File.Copy(_modSyncSettingsPath, _modSyncSettingsBackup, overwrite: true);
                File.Delete(_modSyncSettingsPath);
            }

            _hadLegacySettings = File.Exists(_legacySettingsPath);
            if (_hadLegacySettings)
            {
                File.Copy(_legacySettingsPath, _legacySettingsBackup, overwrite: true);
                File.Delete(_legacySettingsPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            TryDelete(_modSyncSettingsPath);
            TryDelete(_legacySettingsPath);

            if (_hadModSyncSettings && File.Exists(_modSyncSettingsBackup))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_modSyncSettingsPath)!);
                File.Copy(_modSyncSettingsBackup, _modSyncSettingsPath, overwrite: true);
                File.Delete(_modSyncSettingsBackup);
            }

            if (_hadLegacySettings && File.Exists(_legacySettingsBackup))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_legacySettingsPath)!);
                File.Copy(_legacySettingsBackup, _legacySettingsPath, overwrite: true);
                File.Delete(_legacySettingsBackup);
            }
        }

        [Test]
        public void LoadSettings_ReadsLegacyPath_WhenModSyncSettingsMissing()
        {
            WriteSettings(_legacySettingsPath, "/legacy/mods/path");

            AppSettings settings = SettingsManager.LoadSettings();

            Assert.That(settings.SourcePath, Is.EqualTo("/legacy/mods/path"));
        }

        [Test]
        public void LoadSettings_PrefersModSyncPath_WhenBothExist()
        {
            WriteSettings(_legacySettingsPath, "/legacy/mods/path");
            WriteSettings(_modSyncSettingsPath, "/modsync/mods/path");

            AppSettings settings = SettingsManager.LoadSettings();

            Assert.That(settings.SourcePath, Is.EqualTo("/modsync/mods/path"));
        }

        private static void WriteSettings(string path, string sourcePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string json = JsonSerializer.Serialize(new { sourcePath });
            File.WriteAllText(path, json);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best effort cleanup for local test config.
            }
        }
    }
}
