// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Reflection;

using Avalonia.Headless.XUnit;

using KOTORModSync.Services;

using Xunit;

namespace KOTORModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class AutoUpdateClientHeadlessTests
    {
        [AvaloniaFact(DisplayName = "NetSparkle update client initializes once under the headless app")]
        public void Initialize_WithValidSettings_CreatesUpdaterAndIsIdempotent()
        {
            using var client = new NetSparkleUpdateClient();
            var settings = new AutoUpdateSettings
            {
                AppCastUrl = "https://example.com/appcast.xml",
                SignaturePublicKey = "jZSQV+2C1HL2Ufek3ekC7gtgOk5ctuDQzngh86OEdlA="
            };

            client.Initialize(settings);
            object sparkleInstance = GetSparkleInstance(client);

            Assert.NotNull(sparkleInstance);

            client.Initialize(settings);

            Assert.Same(sparkleInstance, GetSparkleInstance(client));
        }

        [Fact]
        public void Initialize_WithNullSettings_ThrowsArgumentNullException()
        {
            using var client = new NetSparkleUpdateClient();

            Assert.Throws<ArgumentNullException>(() => client.Initialize(null));
        }

        private static object GetSparkleInstance(NetSparkleUpdateClient client)
        {
            FieldInfo sparkleField = typeof(NetSparkleUpdateClient).GetField("_sparkle", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(sparkleField);

            return sparkleField.GetValue(client);
        }
    }
}
