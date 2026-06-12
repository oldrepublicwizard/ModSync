// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Text.Json;
using ModSync.Models;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class AppSettingsNxmTests
    {
        [Test]
        public void RegisterNxmProtocolHandler_RoundTripsInJson()
        {
            var settings = new AppSettings
            {
                RegisterNxmProtocolHandler = true,
            };

            string json = JsonSerializer.Serialize(settings);
            AppSettings deserialized = JsonSerializer.Deserialize<AppSettings>(json);

            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized.RegisterNxmProtocolHandler, Is.True);
        }
    }
}
