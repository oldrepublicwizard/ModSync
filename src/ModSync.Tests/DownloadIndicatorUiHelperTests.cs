// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class DownloadIndicatorUiHelperTests
    {
        [Fact(DisplayName = "FormatGettingStartedProgressText uses mod count phrasing")]
        public void FormatGettingStartedProgressText_FormatsModCounts()
        {
            Assert.Equal("Downloaded: 2 / 5 mods", DownloadIndicatorUiHelper.FormatGettingStartedProgressText(2, 5));
        }

        [Fact(DisplayName = "FormatWizardSidebarStatusText reflects in-progress state")]
        public void FormatWizardSidebarStatusText_ReflectsState()
        {
            Assert.Equal("Downloads in progress", DownloadIndicatorUiHelper.FormatWizardSidebarStatusText(true));
            Assert.Equal("Downloads idle", DownloadIndicatorUiHelper.FormatWizardSidebarStatusText(false));
        }

        [Fact(DisplayName = "FormatWizardSidebarProgressText uses mod counts when total is known")]
        public void FormatWizardSidebarProgressText_WithTotal_UsesModCountPhrasing()
        {
            Assert.Equal(
                "Downloaded: 1 / 3 mods",
                DownloadIndicatorUiHelper.FormatWizardSidebarProgressText(downloadInProgress: true, completedDownloads: 1, totalDownloads: 3));
        }

        [Fact(DisplayName = "FormatWizardSidebarProgressText uses preparing text when total is zero and active")]
        public void FormatWizardSidebarProgressText_ActiveWithoutTotal_Preparing()
        {
            Assert.Equal(
                "Preparing downloads…",
                DownloadIndicatorUiHelper.FormatWizardSidebarProgressText(downloadInProgress: true, completedDownloads: 0, totalDownloads: 0));
        }

        [Fact(DisplayName = "FormatWizardSidebarProgressText uses idle text when total is zero and inactive")]
        public void FormatWizardSidebarProgressText_IdleWithoutTotal_QueuedMessage()
        {
            Assert.Equal(
                "No downloads queued.",
                DownloadIndicatorUiHelper.FormatWizardSidebarProgressText(downloadInProgress: false, completedDownloads: 0, totalDownloads: 0));
        }
    }
}
