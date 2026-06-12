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
    public sealed class ValidationDisplayUiHelperTests
    {
        [Fact(DisplayName = "FormatAllValidSummary uses success phrasing")]
        public void FormatAllValidSummary_UsesSuccessPhrasing()
        {
            Assert.Equal("✅ All 3 mods validated successfully!", ValidationDisplayUiHelper.FormatAllValidSummary(3));
        }

        [Fact(DisplayName = "FormatPartialValidSummary uses warning ratio phrasing")]
        public void FormatPartialValidSummary_UsesWarningRatio()
        {
            Assert.Equal(
                "⚠️ 2/5 mods validated successfully",
                ValidationDisplayUiHelper.FormatPartialValidSummary(validCount: 2, selectedCount: 5));
        }

        [Fact(DisplayName = "FormatErrorCounter uses one-based index")]
        public void FormatErrorCounter_UsesOneBasedIndex()
        {
            Assert.Equal("Error 2 of 4", ValidationDisplayUiHelper.FormatErrorCounter(currentIndex: 1, totalErrors: 4));
        }

        [Fact(DisplayName = "CollectInvalidSelectedComponents returns only failing mods")]
        public void CollectInvalidSelectedComponents_ReturnsInvalidOnly()
        {
            var components = new List<ModComponent>
            {
                new ModComponent { Name = "Valid" },
                new ModComponent { Name = "Invalid" },
            };

            List<ModComponent> errors = ValidationDisplayUiHelper.CollectInvalidSelectedComponents(
                components,
                component => string.Equals(component.Name, "Valid", StringComparison.Ordinal));

            Assert.Single(errors);
            Assert.Equal("Invalid", errors[0].Name);
        }

        [Fact(DisplayName = "Navigation helpers respect bounds")]
        public void NavigationHelpers_RespectBounds()
        {
            Assert.False(ValidationDisplayUiHelper.CanNavigateToPreviousError(0));
            Assert.True(ValidationDisplayUiHelper.CanNavigateToPreviousError(1));
            Assert.True(ValidationDisplayUiHelper.CanNavigateToNextError(0, errorCount: 3));
            Assert.False(ValidationDisplayUiHelper.CanNavigateToNextError(2, errorCount: 3));
        }

        [Fact(DisplayName = "CollectInvalidSelectedComponents rejects null inputs")]
        public void CollectInvalidSelectedComponents_RejectsNullInputs()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ValidationDisplayUiHelper.CollectInvalidSelectedComponents(null, _ => true));
            Assert.Throws<ArgumentNullException>(() =>
                ValidationDisplayUiHelper.CollectInvalidSelectedComponents(
                    new List<ModComponent>(),
                    null));
        }
    }
}
