// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class DownloadIndicatorUiHelperHeadlessTests
    {
        [AvaloniaFact(DisplayName = "ApplyGettingStartedTabIndicators shows progress when download is active")]
        public void ApplyGettingStartedTabIndicators_ActiveDownload_UpdatesControls()
        {
            var ledIndicator = new Ellipse();
            var runningText = new TextBlock();
            var stopButton = new Button();
            var progressText = new TextBlock();

            DownloadIndicatorUiHelper.ApplyGettingStartedTabIndicators(
                ledIndicator,
                runningText,
                stopButton,
                progressText,
                isDownloadInProgress: true,
                completedComponents: 2,
                totalComponents: 5);

            AssertBrushColor(ThemeResourceHelper.DownloadLedActiveBrush, ledIndicator.Fill);
            Assert.True(runningText.IsVisible);
            Assert.True(stopButton.IsVisible);
            Assert.True(progressText.IsVisible);
            Assert.Equal("Downloaded: 2 / 5 mods", progressText.Text);
        }

        [AvaloniaFact(DisplayName = "ApplyGettingStartedTabIndicators hides controls when download is idle")]
        public void ApplyGettingStartedTabIndicators_IdleDownload_HidesControls()
        {
            var ledIndicator = new Ellipse();
            var runningText = new TextBlock { IsVisible = true };
            var stopButton = new Button { IsVisible = true };
            var progressText = new TextBlock { IsVisible = true, Text = "stale" };

            DownloadIndicatorUiHelper.ApplyGettingStartedTabIndicators(
                ledIndicator,
                runningText,
                stopButton,
                progressText,
                isDownloadInProgress: false,
                completedComponents: 0,
                totalComponents: 0);

            AssertBrushColor(ThemeResourceHelper.DownloadLedInactiveBrush, ledIndicator.Fill);
            Assert.False(runningText.IsVisible);
            Assert.False(stopButton.IsVisible);
            Assert.False(progressText.IsVisible);
            Assert.Equal("stale", progressText.Text);
        }

        [AvaloniaFact(DisplayName = "GetLedBrush returns theme brush colors for active and idle states")]
        public void GetLedBrush_ReturnsThemeBrushColors()
        {
            IBrush active = DownloadIndicatorUiHelper.GetLedBrush(downloadActive: true);
            IBrush idle = DownloadIndicatorUiHelper.GetLedBrush(downloadActive: false);

            AssertBrushColor(ThemeResourceHelper.DownloadLedActiveBrush, active);
            AssertBrushColor(ThemeResourceHelper.DownloadLedInactiveBrush, idle);
        }

        private static void AssertBrushColor(IBrush expected, IBrush actual)
        {
            Assert.NotNull(expected);
            Assert.NotNull(actual);
            var expectedBrush = Assert.IsAssignableFrom<SolidColorBrush>(expected);
            var actualBrush = Assert.IsAssignableFrom<SolidColorBrush>(actual);
            Assert.Equal(expectedBrush.Color, actualBrush.Color);
        }
    }
}
