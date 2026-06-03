// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KOTORModSync.Controls;
using KOTORModSync.Core;
using Xunit;

namespace KOTORModSync.Tests.HeadlessUITests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class EmbeddedLogPanelHeadlessTests
    {
        private static async Task PumpEventsAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }

        [AvaloniaFact(DisplayName = "EmbeddedLogPanel line count updates after logger messages")]
        public async Task LineCount_UpdatesAfterLoggerMessages()
        {
            Logger.ClearLogHistory();
            EmbeddedLogPanel panel = null;
            Window window = null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                panel = new EmbeddedLogPanel();
                window = new Window
                {
                    Content = panel,
                    Width = 800,
                    Height = 400,
                };
                window.Show();
            }, DispatcherPriority.Background);

            await PumpEventsAsync();

            Logger.Log("headless test line 1");
            Logger.Log("headless test line 2");
            Logger.Log("headless test line 3");

            await PumpEventsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Assert.NotNull(panel);
                TextBlock lineCount = panel.FindControl<TextBlock>("LineCountText");
                Assert.NotNull(lineCount);
                Assert.Equal("3 lines", lineCount.Text);
                Assert.Equal(3, panel.ViewModel.LogLines.Count);
            }, DispatcherPriority.Background);

            await Dispatcher.UIThread.InvokeAsync(() => window?.Close(), DispatcherPriority.Background);
            await PumpEventsAsync();
        }

        [AvaloniaFact(DisplayName = "EmbeddedLogPanel line count shows singular after one line")]
        public async Task LineCount_SingularAfterOneLine()
        {
            Logger.ClearLogHistory();
            EmbeddedLogPanel panel = null;
            Window window = null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                panel = new EmbeddedLogPanel();
                window = new Window { Content = panel, Width = 800, Height = 400 };
                window.Show();
            }, DispatcherPriority.Background);

            await PumpEventsAsync();

            Logger.Log("only line");

            await PumpEventsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TextBlock lineCount = panel.FindControl<TextBlock>("LineCountText");
                Assert.NotNull(lineCount);
                Assert.Equal("1 line", lineCount.Text);
            }, DispatcherPriority.Background);

            await Dispatcher.UIThread.InvokeAsync(() => window?.Close(), DispatcherPriority.Background);
            await PumpEventsAsync();
        }

        [AvaloniaFact(DisplayName = "EmbeddedLogPanel line count resets after clear")]
        public async Task LineCount_ResetsAfterClear()
        {
            Logger.ClearLogHistory();
            EmbeddedLogPanel panel = null;
            Window window = null;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                panel = new EmbeddedLogPanel();
                window = new Window { Content = panel, Width = 800, Height = 400 };
                window.Show();
            }, DispatcherPriority.Background);

            await PumpEventsAsync();

            Logger.Log("before clear");

            await PumpEventsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Button clearButton = panel.GetVisualDescendants().OfType<Button>()
                    .FirstOrDefault(b => b.Content?.ToString() == "Clear");
                Assert.NotNull(clearButton);
                clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }, DispatcherPriority.Background);

            await PumpEventsAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TextBlock lineCount = panel.FindControl<TextBlock>("LineCountText");
                Assert.NotNull(lineCount);
                Assert.Equal("0 lines", lineCount.Text);
                Assert.Empty(panel.ViewModel.LogLines);
            }, DispatcherPriority.Background);

            await Dispatcher.UIThread.InvokeAsync(() => window?.Close(), DispatcherPriority.Background);
            await PumpEventsAsync();
        }
    }
}
