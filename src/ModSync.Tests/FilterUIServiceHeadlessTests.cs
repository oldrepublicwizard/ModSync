// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using ModSync.Core;
using ModSync.Models;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class FilterUIServiceHeadlessTests
    {
        [Fact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new FilterUIService(null));
        }

        [AvaloniaFact(DisplayName = "InitializeFilters builds tier and category filter items")]
        public void InitializeFilters_BuildsTierAndCategoryItems()
        {
            ModComponent essential = CreateComponent("Essential", "1 - Essential", "Patch");
            ModComponent recommended = CreateComponent("Recommended", "2 - Recommended", "UI");
            ModComponent optional = CreateComponent("Optional", "4 - Optional", "UI");

            var config = new MainConfig
            {
                allComponents = new List<ModComponent> { essential, recommended, optional },
            };
            var service = new FilterUIService(config);
            var tierCombo = new ComboBox();
            var categoryItems = new ItemsControl();

            service.InitializeFilters(config.allComponents, tierCombo, categoryItems);

            Assert.Equal(3, service.TierItems.Count);
            Assert.Equal(2, service.CategoryItems.Count);
            Assert.Same(service.TierItems, tierCombo.ItemsSource);
            Assert.Same(service.CategoryItems, categoryItems.ItemsSource);
            Assert.Equal("1 - Essential", service.TierItems[0].Name);
            Assert.Equal("Patch (1)", service.CategoryItems.First(c => c.Name == "Patch").DisplayText);
        }

        [AvaloniaFact(DisplayName = "SelectByTier selects mods in chosen tier and higher priority tiers")]
        public async Task SelectByTier_SelectsMatchingTiers()
        {
            ModComponent essential = CreateComponent("Essential", "1 - Essential", "Patch");
            ModComponent recommended = CreateComponent("Recommended", "2 - Recommended", "UI");
            ModComponent optional = CreateComponent("Optional", "4 - Optional", "UI");

            var config = new MainConfig
            {
                allComponents = new List<ModComponent> { essential, recommended, optional },
            };
            var service = new FilterUIService(config);
            service.InitializeFilters(config.allComponents, new ComboBox(), new ItemsControl());

            TierFilterItem recommendedTier = service.TierItems.First(item => item.Name == "2 - Recommended");
            service.SelectByTier(recommendedTier, (_, _) => { }, () => { });
            await PumpEventsAsync();

            Assert.True(essential.IsSelected);
            Assert.True(recommended.IsSelected);
            Assert.False(optional.IsSelected);
        }

        [AvaloniaFact(DisplayName = "ApplyCategorySelections selects mods in chosen categories")]
        public async Task ApplyCategorySelections_SelectsMatchingCategories()
        {
            ModComponent patchMod = CreateComponent("Patch Mod", "1 - Essential", "Patch");
            ModComponent uiMod = CreateComponent("UI Mod", "2 - Recommended", "UI");

            var config = new MainConfig
            {
                allComponents = new List<ModComponent> { patchMod, uiMod },
            };
            var service = new FilterUIService(config);
            service.InitializeFilters(config.allComponents, new ComboBox(), new ItemsControl());

            SelectionFilterItem patchCategory = service.CategoryItems.First(item => item.Name == "Patch");
            patchCategory.IsSelected = true;

            service.ApplyCategorySelections((_, _) => { }, () => { });
            await PumpEventsAsync();

            Assert.True(patchMod.IsSelected);
            Assert.False(uiMod.IsSelected);
        }

        private static ModComponent CreateComponent(string name, string tier, string category)
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = name,
                Tier = tier,
                Category = new List<string> { category },
            };
        }

        private static async Task PumpEventsAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }
    }
}
