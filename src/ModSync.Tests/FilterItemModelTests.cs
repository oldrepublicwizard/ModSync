// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Models;
using Xunit;

namespace ModSync.Tests
{
    public sealed class FilterItemModelTests
    {
        [Fact(DisplayName = "TierFilterItem DisplayText formats name and count")]
        public void TierFilterItem_DisplayText_FormatsNameAndCount()
        {
            var item = new TierFilterItem
            {
                Name = "1 - Essential",
                Count = 3,
            };

            Assert.Equal("1 - Essential (3)", item.DisplayText);
        }

        [Fact(DisplayName = "TierFilterItem EffectiveSelection reflects selected or included state")]
        public void TierFilterItem_EffectiveSelection_ReflectsSelectionState()
        {
            var item = new TierFilterItem();

            item.IsIncluded = true;
            Assert.True(item.EffectiveSelection);

            item.IsIncluded = false;
            item.IsSelected = true;
            Assert.True(item.EffectiveSelection);

            item.IsSelected = false;
            Assert.False(item.EffectiveSelection);
        }

        [Fact(DisplayName = "TierFilterItem count change updates DisplayText")]
        public void TierFilterItem_CountChange_UpdatesDisplayText()
        {
            var item = new TierFilterItem
            {
                Name = "2 - Recommended",
                Count = 1,
            };

            item.Count = 5;

            Assert.Equal("2 - Recommended (5)", item.DisplayText);
        }

        [Fact(DisplayName = "SelectionFilterItem DisplayText formats name and count")]
        public void SelectionFilterItem_DisplayText_FormatsNameAndCount()
        {
            var item = new SelectionFilterItem
            {
                Name = "Patch",
                Count = 2,
            };

            Assert.Equal("Patch (2)", item.DisplayText);
        }

        [Fact(DisplayName = "SelectionFilterItem IsSelected toggles independently")]
        public void SelectionFilterItem_IsSelected_Toggles()
        {
            var item = new SelectionFilterItem { IsSelected = false };

            item.IsSelected = true;
            Assert.True(item.IsSelected);

            item.IsSelected = false;
            Assert.False(item.IsSelected);
        }
    }
}
