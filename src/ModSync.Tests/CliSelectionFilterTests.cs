// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using ModSync.Core;
using ModSync.Core.CLI;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class CliSelectionFilterTests
    {
        [Test]
        public void ApplySelectionFilters_ModName_SelectsMatchingComponentOnly()
        {
            var components = new List<ModComponent>
            {
                CreateComponent("Silent Sion Restoration", "Immersion", "2 - Recommended"),
                CreateComponent("The Sith Lords Restored Content Mod", "Restored Content", "1 - Essential"),
                CreateComponent("K2 Community Patch", "Bugfix", "1 - Essential"),
            };

            ModBuildConverter.ApplySelectionFilters(components, new[] { "mod:Silent Sion Restoration" });

            Assert.Multiple(() =>
            {
                Assert.That(components.Count(c => c.IsSelected), Is.EqualTo(1));
                Assert.That(components.Single(c => c.IsSelected).Name, Is.EqualTo("Silent Sion Restoration"));
            });
        }

        [Test]
        public void ApplySelectionFilters_ModNamePartialMatch_SelectsSubstringHit()
        {
            var components = new List<ModComponent>
            {
                CreateComponent("Silent Sion Restoration", "Immersion", "2 - Recommended"),
                CreateComponent("Silent Sion Extra", "Immersion", "4 - Optional"),
            };

            ModBuildConverter.ApplySelectionFilters(components, new[] { "mod:Silent Sion" });

            Assert.That(components.Count(c => c.IsSelected), Is.EqualTo(2));
        }

        [Test]
        public void ApplySelectionFilters_TwoModNameFilters_SelectsBothComponents()
        {
            var components = new List<ModComponent>
            {
                CreateComponent("Silent Sion Restoration", "Immersion", "2 - Recommended"),
                CreateComponent("Prestige Class Saving Throw Fixes", "Mechanics Change", "2 - Recommended"),
                CreateComponent("K2 Community Patch", "Bugfix", "1 - Essential"),
            };

            ModBuildConverter.ApplySelectionFilters(components, new[]
            {
                "mod:Silent Sion Restoration",
                "mod:Prestige Class Saving Throw Fixes",
            });

            Assert.Multiple(() =>
            {
                Assert.That(components.Count(c => c.IsSelected), Is.EqualTo(2));
                Assert.That(components.Where(c => c.IsSelected).Select(c => c.Name),
                    Is.EquivalentTo(new[] { "Silent Sion Restoration", "Prestige Class Saving Throw Fixes" }));
            });
        }

        [Test]
        public void ApplySelectionFilters_CategoryStillWorksAlongsideModName()
        {
            var components = new List<ModComponent>
            {
                CreateComponent("Silent Sion Restoration", "Immersion", "2 - Recommended"),
                CreateComponent("Other Immersion Mod", "Immersion", "2 - Recommended"),
            };

            ModBuildConverter.ApplySelectionFilters(components, new[] { "mod:Silent Sion Restoration", "category:Immersion" });

            Assert.That(components.Count(c => c.IsSelected), Is.EqualTo(1));
            Assert.That(components.Single(c => c.IsSelected).Name, Is.EqualTo("Silent Sion Restoration"));
        }

        private static ModComponent CreateComponent(string name, string category, string tier)
        {
            return new ModComponent
            {
                Name = name,
                Tier = tier,
                Category = new List<string> { category },
            };
        }
    }
}
