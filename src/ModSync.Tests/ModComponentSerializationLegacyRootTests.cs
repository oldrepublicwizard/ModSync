// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using ModSync.Core;
using ModSync.Core.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ModComponentSerializationLegacyRootTests
    {
        [Test]
        public void DeserializeModComponentFromXmlString_AcceptsLegacyKOTORModSyncRoot()
        {
            var component = new ModComponent
            {
                Name = "Legacy XML Root",
                Guid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            };

            string xml = ModComponentSerializationService.SerializeModComponentAsXmlString(
                new List<ModComponent> { component });
            string legacyXml = xml
                .Replace("<ModSync>", "<KOTORModSync>", StringComparison.Ordinal)
                .Replace("</ModSync>", "</KOTORModSync>", StringComparison.Ordinal);

            IReadOnlyList<ModComponent> loaded =
                ModComponentSerializationService.DeserializeModComponentFromXmlString(legacyXml);

            Assert.Multiple(() =>
            {
                Assert.That(loaded, Has.Count.EqualTo(1));
                Assert.That(loaded[0].Name, Is.EqualTo("Legacy XML Root"));
                Assert.That(loaded[0].Guid, Is.EqualTo(component.Guid));
            });
        }
    }
}
