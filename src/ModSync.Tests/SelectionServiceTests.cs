// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class SelectionServiceTests
    {
        [Fact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SelectionService(null));
        }

        [Fact(DisplayName = "SelectAll selects unselected components and invokes callback")]
        public void SelectAll_SelectsUnselectedComponents()
        {
            var alreadySelected = CreateComponent("Already");
            alreadySelected.IsSelected = true;
            var unselected = CreateComponent("Pending");

            var config = CreateConfig(alreadySelected, unselected);
            var service = new SelectionService(config);
            int callbackCount = 0;

            service.SelectAll((component, _) =>
            {
                callbackCount++;
                Assert.Equal(unselected, component);
            });

            Assert.True(alreadySelected.IsSelected);
            Assert.True(unselected.IsSelected);
            Assert.Equal(1, callbackCount);
        }

        [Fact(DisplayName = "DeselectAll clears selection on all components")]
        public void DeselectAll_ClearsAllSelections()
        {
            var first = CreateComponent("First");
            var second = CreateComponent("Second");
            first.IsSelected = true;
            second.IsSelected = true;

            var service = new SelectionService(CreateConfig(first, second));
            service.DeselectAll(null);

            Assert.False(first.IsSelected);
            Assert.False(second.IsSelected);
        }

        [Fact(DisplayName = "SelectByTier selects components in matching and higher-priority tiers")]
        public void SelectByTier_IncludesMatchingAndHigherPriorityTiers()
        {
            var essential = CreateComponent("Essential");
            essential.Tier = "1 - Essential";
            var recommended = CreateComponent("Recommended");
            recommended.Tier = "2 - Recommended";
            var optional = CreateComponent("Optional");
            optional.Tier = "4 - Optional";

            var service = new SelectionService(CreateConfig(essential, recommended, optional));
            int callbackCount = 0;

            service.SelectByTier(
                "2 - Recommended",
                2,
                new List<string> { "1 - Essential", "2 - Recommended", "3 - Suggested", "4 - Optional" },
                new List<int> { 1, 2, 3, 4 },
                (_, _) => callbackCount++);

            Assert.True(essential.IsSelected);
            Assert.True(recommended.IsSelected);
            Assert.False(optional.IsSelected);
            Assert.Equal(2, callbackCount);
        }

        [Fact(DisplayName = "SelectByCategories selects components matching any selected category")]
        public void SelectByCategories_SelectsMatchingCategories()
        {
            var graphics = CreateComponent("Graphics");
            graphics.Category = new List<string> { "Graphics Improvement" };
            var ui = CreateComponent("UI");
            ui.Category = new List<string> { "UI" };

            var service = new SelectionService(CreateConfig(graphics, ui));
            int callbackCount = 0;

            service.SelectByCategories(
                new List<string> { "Graphics Improvement" },
                (_, _) => callbackCount++);

            Assert.True(graphics.IsSelected);
            Assert.False(ui.IsSelected);
            Assert.Equal(1, callbackCount);
        }

        [Fact(DisplayName = "SelectByCategories with empty list does not change selection")]
        public void SelectByCategories_EmptyList_NoChange()
        {
            var component = CreateComponent("Mod");
            component.IsSelected = false;

            var service = new SelectionService(CreateConfig(component));
            service.SelectByCategories(new List<string>(), (_, _) => Assert.Fail("callback should not run"));

            Assert.False(component.IsSelected);
        }

        private static ModComponent CreateComponent(string name)
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = name,
            };
        }

        private static MainConfig CreateConfig(params ModComponent[] components)
        {
            return new MainConfig
            {
                allComponents = new List<ModComponent>(components),
            };
        }
    }
}
