// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core;

namespace ModSync.Services
{
    public static class ValidationDisplayUiHelper
    {
        public static string FormatAllValidSummary(int selectedCount) =>
            $"✅ All {selectedCount} mods validated successfully!";

        public static string FormatPartialValidSummary(int validCount, int selectedCount) =>
            $"⚠️ {validCount}/{selectedCount} mods validated successfully";

        public static string FormatErrorCounter(int currentIndex, int totalErrors) =>
            $"Error {currentIndex + 1} of {totalErrors}";

        public static List<ModComponent> CollectInvalidSelectedComponents(
            IEnumerable<ModComponent> selectedComponents,
            Func<ModComponent, bool> isComponentValid)
        {
            if (selectedComponents is null)
            {
                throw new ArgumentNullException(nameof(selectedComponents));
            }

            if (isComponentValid is null)
            {
                throw new ArgumentNullException(nameof(isComponentValid));
            }

            var validationErrors = new List<ModComponent>();
            foreach (ModComponent component in selectedComponents)
            {
                if (!isComponentValid(component))
                {
                    validationErrors.Add(component);
                }
            }

            return validationErrors;
        }

        public static bool CanNavigateToPreviousError(int currentErrorIndex) => currentErrorIndex > 0;

        public static bool CanNavigateToNextError(int currentErrorIndex, int errorCount) =>
            currentErrorIndex < errorCount - 1;
    }
}
