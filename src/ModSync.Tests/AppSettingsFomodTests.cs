// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json;
using ModSync.Core.Services.Fomod;
using ModSync.Models;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class AppSettingsFomodTests
    {
        [Test]
        public void FomodPostDownloadMode_RoundTripsInJson()
        {
            var settings = new AppSettings
            {
                FomodPostDownloadMode = "skip",
            };

            string json = JsonSerializer.Serialize(settings);
            AppSettings deserialized = JsonSerializer.Deserialize<AppSettings>(json);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.FomodPostDownloadMode, Is.EqualTo("skip"));
        }

        [Test]
        public void FomodPostDownloadMode_UsesSameKeyAsCliSettings()
        {
            var settings = new AppSettings { FomodPostDownloadMode = "warn-continue" };

            string json = JsonSerializer.Serialize(settings);

            Assert.That(json, Does.Contain(FomodPostDownloadOptionsResolver.SettingsKey));
        }
    }
}
