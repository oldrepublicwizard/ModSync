// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
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
    public sealed class DownloadOrchestrationServiceTests
    {
        [AvaloniaFact(DisplayName = "Constructor rejects null DownloadCacheService")]
        public void Constructor_NullCacheService_Throws()
        {
            Window window = new Window();
            var config = new MainConfig();

            Assert.Throws<ArgumentNullException>(() =>
                new DownloadOrchestrationService(null, config, window));
        }

        [AvaloniaFact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Window window = new Window();
            var cacheService = new DownloadCacheService();

            Assert.Throws<ArgumentNullException>(() =>
                new DownloadOrchestrationService(cacheService, null, window));
        }

        [AvaloniaFact(DisplayName = "Constructor rejects null parent window")]
        public void Constructor_NullParentWindow_Throws()
        {
            var cacheService = new DownloadCacheService();
            var config = new MainConfig();

            Assert.Throws<ArgumentNullException>(() =>
                new DownloadOrchestrationService(cacheService, config, null));
        }

        [AvaloniaFact(DisplayName = "Initial download state is idle with zero counters")]
        public void InitialState_IsIdleWithZeroCounters()
        {
            DownloadOrchestrationService service = CreateService(out Window window);

            try
            {
                Assert.False(service.IsDownloadInProgress);
                Assert.Equal(0, service.TotalComponentsToDownload);
                Assert.Equal(0, service.CompletedComponents);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "CancelAllDownloadsAsync raises DownloadStateChanged and stays idle")]
        public async Task CancelAllDownloadsAsync_RaisesStateChangedAndClearsInProgress()
        {
            DownloadOrchestrationService service = CreateService(out Window window);
            int stateChangeCount = 0;
            service.DownloadStateChanged += (_, _) => stateChangeCount++;

            try
            {
                await service.CancelAllDownloadsAsync();
                await PumpEventsAsync();

                Assert.False(service.IsDownloadInProgress);
                Assert.Equal(1, stateChangeCount);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "DownloadSingleComponentAsync without URLs does not mark download in progress")]
        public async Task DownloadSingleComponentAsync_NoUrls_DoesNotStartDownload()
        {
            DownloadOrchestrationService service = CreateService(out Window window);
            DownloadProgressWindow progressWindow = await CreateProgressWindowAsync();
            var component = new ModComponent { Name = "No Urls Mod" };

            try
            {
                await service.DownloadSingleComponentAsync(component, progressWindow);
                await PumpEventsAsync();

                Assert.False(service.IsDownloadInProgress);
            }
            finally
            {
                progressWindow.Close();
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "DownloadSingleComponentAsync ignores null component")]
        public async Task DownloadSingleComponentAsync_NullComponent_ReturnsWithoutThrow()
        {
            DownloadOrchestrationService service = CreateService(out Window window);
            DownloadProgressWindow progressWindow = await CreateProgressWindowAsync();

            try
            {
                await service.DownloadSingleComponentAsync(null, progressWindow);
                await PumpEventsAsync();
            }
            finally
            {
                progressWindow.Close();
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "DownloadSingleComponentAsync ignores null progress window")]
        public async Task DownloadSingleComponentAsync_NullProgressWindow_ReturnsWithoutThrow()
        {
            DownloadOrchestrationService service = CreateService(out Window window);
            var component = new ModComponent { Name = "Test Mod" };

            try
            {
                await service.DownloadSingleComponentAsync(component, null);
                await PumpEventsAsync();
            }
            finally
            {
                window.Close();
            }
        }

        private static DownloadOrchestrationService CreateService(out Window window)
        {
            window = new Window { Width = 400, Height = 300 };
            var config = new MainConfig();
            var cacheService = new DownloadCacheService();
            cacheService.SetDownloadManager();
            return new DownloadOrchestrationService(cacheService, config, window);
        }

        private static async Task<DownloadProgressWindow> CreateProgressWindowAsync()
        {
            DownloadProgressWindow window = await Dispatcher.UIThread.InvokeAsync(
                () => new DownloadProgressWindow(),
                DispatcherPriority.Background);
            await PumpEventsAsync();
            return window;
        }

        private static async Task PumpEventsAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }
    }
}
