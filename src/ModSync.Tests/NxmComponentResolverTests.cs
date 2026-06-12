// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core;
using ModSync.Core.Services.Download;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NxmComponentResolverTests
    {
        private static ModComponent CreateComponent(string name, params string[] registryUrls)
        {
            var registry = new Dictionary<string, ResourceMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (string url in registryUrls)
            {
                registry[url] = new ResourceMetadata();
            }

            return new ModComponent
            {
                Name = name,
                Guid = Guid.NewGuid(),
                ResourceRegistry = registry,
            };
        }

        [Test]
        public void FindComponentForNxmUrl_MatchingRegistryKey_ReturnsComponent()
        {
            _ = NxmUrl.TryParse("nxm://kotor/mods/1234/files/5678?key=a&expires=99", out NxmUrl nxm);
            var components = new List<ModComponent>
            {
                CreateComponent("Other", "https://deadlystream.com/files/file/1/"),
                CreateComponent("Target", "https://www.nexusmods.com/kotor/mods/1234?tab=files"),
            };

            ModComponent match = NxmComponentResolver.FindComponentForNxmUrl(nxm, components);

            Assert.That(match, Is.Not.Null);
            Assert.That(match.Name, Is.EqualTo("Target"));
        }

        [Test]
        public void FindComponentForNxmUrl_NoMatch_ReturnsNull()
        {
            _ = NxmUrl.TryParse("nxm://kotor2/mods/999/files/1", out NxmUrl nxm);
            var components = new List<ModComponent>
            {
                CreateComponent("K1 mod", "https://www.nexusmods.com/kotor/mods/1234"),
            };

            Assert.That(NxmComponentResolver.FindComponentForNxmUrl(nxm, components), Is.Null);
        }

        [Test]
        public void FindComponentForNxmUrl_AmbiguousDifferentComponents_ReturnsNull()
        {
            _ = NxmUrl.TryParse("nxm://kotor/mods/1234/files/1", out NxmUrl nxm);
            var components = new List<ModComponent>
            {
                CreateComponent("A", "https://www.nexusmods.com/kotor/mods/1234"),
                CreateComponent("B", "https://nexusmods.com/kotor/mods/1234?tab=description"),
            };

            Assert.That(NxmComponentResolver.FindComponentForNxmUrl(nxm, components), Is.Null);
        }

        [Test]
        public void FindComponentForNxmUrl_EmptyComponents_ReturnsNull()
        {
            _ = NxmUrl.TryParse("nxm://kotor/mods/1/files/2", out NxmUrl nxm);
            Assert.That(NxmComponentResolver.FindComponentForNxmUrl(nxm, new List<ModComponent>()), Is.Null);
        }
    }
}
