// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

using ModSync;

namespace ModSync.Services
{
    public static class DownloadIndicatorUiHelper
    {
        public static string FormatGettingStartedProgressText(int completedComponents, int totalComponents) =>
            $"Downloaded: {completedComponents} / {totalComponents} mods";

        public static string FormatWizardSidebarStatusText(bool downloadInProgress) =>
            downloadInProgress ? "Downloads in progress" : "Downloads idle";

        public static string FormatWizardSidebarProgressText(bool downloadInProgress, int completedDownloads, int totalDownloads)
        {
            if (totalDownloads > 0)
            {
                return FormatGettingStartedProgressText(completedDownloads, totalDownloads);
            }

            return downloadInProgress ? "Preparing downloads…" : "No downloads queued.";
        }

        public static IBrush GetLedBrush(bool downloadActive) =>
            downloadActive
                ? ThemeResourceHelper.DownloadLedActiveBrush
                : ThemeResourceHelper.DownloadLedInactiveBrush;

        public static void ApplyGettingStartedTabIndicators(
            Ellipse ledIndicator,
            TextBlock runningText,
            Button stopButton,
            TextBlock progressText,
            bool isDownloadInProgress,
            int completedComponents,
            int totalComponents)
        {
            if (ledIndicator != null)
            {
                ledIndicator.Fill = GetLedBrush(isDownloadInProgress);
            }

            if (runningText != null)
            {
                runningText.IsVisible = isDownloadInProgress;
            }

            if (stopButton != null)
            {
                stopButton.IsVisible = isDownloadInProgress;
            }

            if (progressText != null)
            {
                progressText.IsVisible = isDownloadInProgress;
                if (isDownloadInProgress)
                {
                    progressText.Text = FormatGettingStartedProgressText(completedComponents, totalComponents);
                }
            }
        }
    }
}
