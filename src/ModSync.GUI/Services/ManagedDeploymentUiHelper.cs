// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using JetBrains.Annotations;

using ModSync.Core.Services;
using ModSync.Core.Services.Installation;

namespace ModSync.Services
{
    public static class ManagedDeploymentUiHelper
    {
        public static void TryApplySummary([CanBeNull] TextBlock summaryTextBlock)
        {
            if (summaryTextBlock is null)
            {
                return;
            }

            string summary = ManagedInstallSummaryFormatter.FormatWizardSummary(InstallationService.LastManagedInstallResult);
            if (string.IsNullOrWhiteSpace(summary))
            {
                summaryTextBlock.IsVisible = false;
                summaryTextBlock.Text = string.Empty;
                return;
            }

            summaryTextBlock.Text = summary;
            summaryTextBlock.IsVisible = true;
        }
    }
}
