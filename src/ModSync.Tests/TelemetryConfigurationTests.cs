// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using ModSync.Core.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class TelemetryConfigurationTests
    {
        private string _appDataRoot = string.Empty;
        private string _configDirectory = string.Empty;
        private string _configFilePath = string.Empty;
        private string _keyFilePath = string.Empty;
        private string _originalSigningSecret = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _appDataRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModSync");
            _configDirectory = _appDataRoot;
            _configFilePath = Path.Combine(_configDirectory, "telemetry_config.json");
            _keyFilePath = Path.Combine(_configDirectory, "telemetry.key");
            _originalSigningSecret = Environment.GetEnvironmentVariable("MODSYNC_SIGNING_SECRET")
                ?? Environment.GetEnvironmentVariable("KOTORMODSYNC_SIGNING_SECRET");
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("MODSYNC_SIGNING_SECRET", null);
            Environment.SetEnvironmentVariable("KOTORMODSYNC_SIGNING_SECRET", _originalSigningSecret);

            try
            {
                if (File.Exists(_configFilePath))
                {
                    File.Delete(_configFilePath);
                }

                if (File.Exists(_keyFilePath))
                {
                    File.Delete(_keyFilePath);
                }
            }
            catch
            {
                // Best effort cleanup for local test config.
            }
        }

        [Test]
        public void SaveThenLoad_PreservesKeyConfigurationFields()
        {
            var config = new TelemetryConfiguration
            {
                IsEnabled = true,
                UserConsented = true,
                CollectUsageData = false,
                EnableOtlpExporter = false,
                EnablePrometheusExporter = true,
                PrometheusPort = 9555,
            };

            config.Save();
            TelemetryConfiguration loaded = TelemetryConfiguration.Load();

            Assert.Multiple(() =>
            {
                Assert.That(loaded.IsEnabled, Is.True);
                Assert.That(loaded.UserConsented, Is.True);
                Assert.That(loaded.CollectUsageData, Is.False);
                Assert.That(loaded.EnableOtlpExporter, Is.False);
                Assert.That(loaded.EnablePrometheusExporter, Is.True);
                Assert.That(loaded.PrometheusPort, Is.EqualTo(9555));
                Assert.That(loaded.SessionId, Is.Not.Null.And.Not.Empty);
                Assert.That(loaded.AnonymousUserId, Is.Not.Null.And.Not.Empty);
            });
        }

        [Test]
        public void Load_UsesEnvironmentSigningSecretWhenPresent()
        {
            Environment.SetEnvironmentVariable("KOTORMODSYNC_SIGNING_SECRET", "env-secret-value");

            TelemetryConfiguration loaded = TelemetryConfiguration.Load();

            Assert.That(loaded.SigningSecret, Is.EqualTo("env-secret-value"));
        }

        [Test]
        public void Load_PrefersModSyncSigningSecretOverLegacyName()
        {
            Environment.SetEnvironmentVariable("KOTORMODSYNC_SIGNING_SECRET", "legacy-secret");
            Environment.SetEnvironmentVariable("MODSYNC_SIGNING_SECRET", "modsync-secret");

            TelemetryConfiguration loaded = TelemetryConfiguration.Load();

            Assert.That(loaded.SigningSecret, Is.EqualTo("modsync-secret"));
        }

        [Test]
        public void Load_UsesLegacyTelemetryConfigPath_WhenModSyncConfigMissing()
        {
            string legacyDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KOTORModSync");
            string legacyConfigPath = Path.Combine(legacyDirectory, "telemetry_config.json");

            try
            {
                if (File.Exists(_configFilePath))
                {
                    File.Delete(_configFilePath);
                }

                Directory.CreateDirectory(legacyDirectory);
                File.WriteAllText(
                    legacyConfigPath,
                    """
                    {
                      "enabled": true,
                      "user_consented": true,
                      "enable_prometheus_exporter": true,
                      "prometheus_port": 9555
                    }
                    """);

                TelemetryConfiguration loaded = TelemetryConfiguration.Load();

                Assert.Multiple(() =>
                {
                    Assert.That(loaded.IsEnabled, Is.True);
                    Assert.That(loaded.UserConsented, Is.True);
                    Assert.That(loaded.EnablePrometheusExporter, Is.True);
                    Assert.That(loaded.PrometheusPort, Is.EqualTo(9555));
                });
            }
            finally
            {
                if (File.Exists(legacyConfigPath))
                {
                    File.Delete(legacyConfigPath);
                }
            }
        }

        [Test]
        public void Load_UsesLegacyTelemetryKeyPath_WhenModSyncKeyMissing()
        {
            string legacyDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KOTORModSync");
            string legacyKeyPath = Path.Combine(legacyDirectory, "telemetry.key");

            try
            {
                if (File.Exists(_keyFilePath))
                {
                    File.Delete(_keyFilePath);
                }

                Directory.CreateDirectory(legacyDirectory);
                File.WriteAllText(legacyKeyPath, "legacy-key-value");

                TelemetryConfiguration loaded = TelemetryConfiguration.Load();

                Assert.That(loaded.SigningSecret, Is.EqualTo("legacy-key-value"));
            }
            finally
            {
                if (File.Exists(legacyKeyPath))
                {
                    File.Delete(legacyKeyPath);
                }
            }
        }

        [Test]
        public void SetUserConsent_DisablesTelemetryAndUpdatesSummary()
        {
            var config = new TelemetryConfiguration
            {
                IsEnabled = true,
                UserConsented = true,
            };

            config.SetUserConsent(false);

            Assert.Multiple(() =>
            {
                Assert.That(config.IsEnabled, Is.False);
                Assert.That(config.UserConsented, Is.False);
                Assert.That(config.GetPrivacySummary(), Does.Contain("Telemetry is disabled"));
            });
        }
    }
}
