// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class ModListServiceTests
    {
        [Fact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ModListService(null));
        }

        [Fact(DisplayName = "FilterModList returns all mods when search text is blank")]
        public void FilterModList_BlankSearch_ReturnsAllMods()
        {
            ModComponent first = CreateComponent("Alpha");
            ModComponent second = CreateComponent("Beta");
            var service = new ModListService(CreateConfig(first, second));

            List<ModComponent> results = service.FilterModList(string.Empty);

            Assert.Equal(2, results.Count);
        }

        [Fact(DisplayName = "FilterModList matches name, author, and category by default")]
        public void FilterModList_DefaultOptions_MatchesMultipleFields()
        {
            ModComponent byName = CreateComponent("UniqueName");
            ModComponent byAuthor = CreateComponent("Other");
            byAuthor.Author = "UniqueAuthor";
            ModComponent byCategory = CreateComponent("Another");
            byCategory.Category = new List<string> { "UniqueCategory" };

            var service = new ModListService(CreateConfig(byName, byAuthor, byCategory));

            Assert.Single(service.FilterModList("UniqueName"));
            Assert.Single(service.FilterModList("UniqueAuthor"));
            Assert.Single(service.FilterModList("UniqueCategory"));
        }

        [Fact(DisplayName = "FilterModList honors ModSearchOptions field toggles")]
        public void FilterModList_SearchOptions_LimitsFields()
        {
            ModComponent component = CreateComponent("HiddenName");
            component.Author = "VisibleAuthor";

            var service = new ModListService(CreateConfig(component));
            var options = new ModManagementService.ModSearchOptions
            {
                SearchInName = false,
                SearchInAuthor = true,
                SearchInCategory = false,
                SearchInDescription = false,
            };

            Assert.Empty(service.FilterModList("HiddenName", options));
            Assert.Single(service.FilterModList("VisibleAuthor", options));
        }

        [AvaloniaFact(DisplayName = "PopulateModList fills list box and invokes count callback")]
        public void PopulateModList_FillsListBox()
        {
            var listBox = new ListBox();
            var components = new List<ModComponent> { CreateComponent("One"), CreateComponent("Two") };
            int callbackCount = 0;

            ModListService.PopulateModList(listBox, components, () => callbackCount++);

            Assert.Equal(2, listBox.Items.Count);
            Assert.Equal(1, callbackCount);
        }

        [AvaloniaFact(DisplayName = "UpdateModCounts updates labels and select-all state")]
        public void UpdateModCounts_UpdatesLabelsAndSelectAll()
        {
            ModComponent selected = CreateComponent("Selected");
            selected.IsSelected = true;
            ModComponent unselected = CreateComponent("Unselected");
            var service = new ModListService(CreateConfig(selected, unselected));

            var modCountText = new TextBlock();
            var selectedCountText = new TextBlock();
            var selectAllCheckBox = new CheckBox();
            bool suppressEvents = false;

            service.UpdateModCounts(
                modCountText,
                selectedCountText,
                selectAllCheckBox,
                value => suppressEvents = value);

            Assert.Equal("2 mods", modCountText.Text);
            Assert.Equal("1 selected", selectedCountText.Text);
            Assert.Null(selectAllCheckBox.IsChecked);
            Assert.False(suppressEvents);
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
