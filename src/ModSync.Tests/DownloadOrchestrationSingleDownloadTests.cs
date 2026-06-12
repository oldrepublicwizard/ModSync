// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class DownloadOrchestrationSingleDownloadTests
    {
        [AvaloniaFact(DisplayName = "Single-download tracking toggles orchestration counters and in-progress flag")]
        public async Task SingleDownloadTracking_UpdatesCountersAndInProgressFlag()
        {
            var window = new Window();
            var config = new MainConfig();
            var cache = new DownloadCacheService();
            var orchestration = new DownloadOrchestrationService(cache, config, window);

            int changeCount = 0;
            orchestration.DownloadStateChanged += (_, __) => changeCount++;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                orchestration.BeginSingleDownloadTracking();
            });

            Assert.True(orchestration.IsDownloadInProgress);
            Assert.Equal(1, orchestration.TotalComponentsToDownload);
            Assert.Equal(0, orchestration.CompletedComponents);
            Assert.Equal(1, changeCount);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                orchestration.EndSingleDownloadTracking(succeeded: true);
            });

            Assert.False(orchestration.IsDownloadInProgress);
            Assert.Equal(1, orchestration.CompletedComponents);
            Assert.Equal(2, changeCount);
        }
    }
}
