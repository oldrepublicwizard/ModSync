// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ModSync.Controls;
using ModSync.Dialogs.WizardPages;
using Xunit;

namespace ModSync.Tests.HeadlessUITests
{
    /// <summary>
    /// Headless Avalonia smoke coverage for paste-import entry points and
    /// wizard page-0 layout constraints. Prefer these over a real desktop
    /// session for agent GUI UX smoke.
    /// </summary>
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class GuiSmokeHeadlessTests
    {
        [AvaloniaFact(DisplayName = "GettingStarted Import from Clipboard button exists and raises event")]
        public async Task ImportFromClipboardButton_Exists_And_IsInvokable()
        {
            var tab = new GettingStartedTab();
            Window window = await HostInWindowAsync(tab, width: 900, height: 700);

            try
            {
                int fired = 0;
                tab.ImportFromClipboardRequested += (_, __) => fired++;

                Button pasteButton = tab.FindControl<Button>("ImportFromClipboardButton");
                Assert.NotNull(pasteButton);
                Assert.Contains("Clipboard", pasteButton.Content?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                Assert.True(pasteButton.IsEnabled);

                await Dispatcher.UIThread.InvokeAsync(
                    () => pasteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)),
                    DispatcherPriority.Background);
                await PumpEventsAsync();

                Assert.Equal(1, fired);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "WelcomePage fits compact host via ScrollViewer without clipping primary title")]
        public async Task WelcomePage_CompactHost_KeepsTitleReachable()
        {
            var page = new WelcomePage();
            // Compact wizard content host size (below design 1280x720).
            Window window = await HostInWindowAsync(page, width: 960, height: 540);

            try
            {
                await PumpEventsAsync();

                ScrollViewer scrollViewer = page.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                Assert.NotNull(scrollViewer);

                TextBlock title = page.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => string.Equals(tb.Text, "Welcome to ModSync", StringComparison.Ordinal));
                Assert.NotNull(title);

                // Layout should resolve without throwing; title remains in the visual tree.
                Assert.True(title.IsVisible);
                Assert.True(page.Bounds.Height > 0 || window.Bounds.Height > 0);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "LandingPageView page-0 content is scrollable in a compact host")]
        public async Task LandingPageView_CompactHost_UsesScrollViewer()
        {
            var view = new LandingPageView();
            Window window = await HostInWindowAsync(view, width: 900, height: 500);

            try
            {
                await PumpEventsAsync();

                ScrollViewer scrollViewer = view.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                Assert.NotNull(scrollViewer);
                Assert.Equal(Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);

                TextBlock header = view.FindControl<TextBlock>("HeaderText");
                Assert.NotNull(header);
                Assert.Contains("ModSync", header.Text ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "ValidatePage exposes resizable log splitter controls")]
        public async Task ValidatePage_LogSplitter_IsPresent()
        {
            var page = new ValidatePage();
            Window window = await HostInWindowAsync(page, width: 1100, height: 800);

            try
            {
                await PumpEventsAsync();

                Expander logExpander = page.FindControl<Expander>("LogExpander");
                GridSplitter splitter = page.FindControl<GridSplitter>("LogGridSplitter");
                ScrollViewer logScroll = page.FindControl<ScrollViewer>("LogScrollViewer");

                Assert.NotNull(logExpander);
                Assert.NotNull(splitter);
                Assert.NotNull(logScroll);
                Assert.Equal(GridResizeDirection.Rows, splitter.ResizeDirection);
                // Fixed MaxHeight was the old non-resizable constraint; star+splitter replaces it.
                Assert.True(double.IsNaN(logScroll.MaxHeight) || logScroll.MaxHeight > 500);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }


        [AvaloniaFact(DisplayName = "InstallStartPage mod list scrolls without fixed MaxHeight")]
        public async Task InstallStartPage_ModList_IsScrollableWithoutMaxHeight()
        {
            var page = new InstallStartPage();
            Window window = await HostInWindowAsync(page, width: 960, height: 540);
            try
            {
                await PumpEventsAsync();
                ScrollViewer modListScroll = page.FindControl<ScrollViewer>("ModListScrollViewer");
                Assert.NotNull(modListScroll);
                Assert.Equal(Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, modListScroll.VerticalScrollBarVisibility);
                Assert.True(IsUnconstrainedMaxHeight(modListScroll.MaxHeight));
                AssertNoFixedMaxHeightAncestor(modListScroll);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "WidescreenModSelectionPage mod list uses ScrollViewer")]
        public async Task WidescreenModSelectionPage_ModList_IsScrollable()
        {
            var page = new WidescreenModSelectionPage();
            Window window = await HostInWindowAsync(page, width: 900, height: 500);
            try
            {
                await PumpEventsAsync();
                ScrollViewer modListScroll = page.FindControl<ScrollViewer>("ModListScrollViewer");
                Assert.NotNull(modListScroll);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "Preamble and notice pages scroll without MaxHeight caps")]
        public async Task PreambleAndNoticePages_ScrollWithoutMaxHeight()
        {
            var preamble = new PreamblePage("## Compact host preamble");
            Window preambleWindow = await HostInWindowAsync(preamble, width: 900, height: 480);
            try
            {
                await PumpEventsAsync();
                ScrollViewer contentScroll = preamble.FindControl<ScrollViewer>("ContentScrollViewer");
                Assert.NotNull(contentScroll);
                AssertNoFixedMaxHeightAncestor(contentScroll);
            }
            finally
            {
                await CloseWindowAsync(preambleWindow);
            }

            foreach (Control page in new Control[] { new AspyrNoticePage("Aspyr"), new WidescreenNoticePage("WS") })
            {
                Window window = await HostInWindowAsync(page, width: 900, height: 480);
                try
                {
                    await PumpEventsAsync();
                    ScrollViewer scroll = page.FindControl<ScrollViewer>("ContentScrollViewer");
                    Assert.NotNull(scroll);
                    AssertNoFixedMaxHeightAncestor(scroll);
                }
                finally
                {
                    await CloseWindowAsync(window);
                }
            }
        }

        [AvaloniaFact(DisplayName = "Later wizard pages remain scrollable in compact hosts")]
        public async Task LaterWizardPages_CompactHost_UseScrollViewer()
        {
            Control[] pages =
            {
                new FinishedPage(),
                new InstallingPage(),
                new BaseInstallCompletePage(),
                new WidescreenCompletePage(),
                new WidescreenInstallingPage(),
                new DownloadsExplainPage(),
            };

            foreach (Control page in pages)
            {
                Window window = await HostInWindowAsync(page, width: 900, height: 480);
                try
                {
                    await PumpEventsAsync();
                    ScrollViewer scrollViewer = page.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    Assert.True(scrollViewer != null, page.GetType().Name + " should host a ScrollViewer");
                }
                finally
                {
                    await CloseWindowAsync(window);
                }
            }

            var modSelection = new ModSelectionPage();
            Window modWindow = await HostInWindowAsync(modSelection, width: 900, height: 500);
            try
            {
                await PumpEventsAsync();
                Assert.NotNull(modSelection.FindControl<ScrollViewer>("ModListScrollViewer"));
            }
            finally
            {
                await CloseWindowAsync(modWindow);
            }
        }

        private static bool IsUnconstrainedMaxHeight(double maxHeight) =>
            double.IsNaN(maxHeight) || double.IsInfinity(maxHeight) || maxHeight <= 0 || maxHeight >= 10_000;

        private static void AssertNoFixedMaxHeightAncestor(Control control)
        {
            Control current = control;
            while (current != null)
            {
                if (!IsUnconstrainedMaxHeight(current.MaxHeight))
                {
                    Assert.Fail($"Found fixed MaxHeight={current.MaxHeight} on {current.GetType().Name}; use star rows instead.");
                }

                current = current.Parent as Control;
            }
        }

        private static async Task<Window> HostInWindowAsync(Control control, double width, double height)
        {
            Window window = await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    var host = new Window
                    {
                        Content = control,
                        Width = width,
                        Height = height,
                    };
                    host.Show();
                    return host;
                },
                DispatcherPriority.Background);

            await PumpEventsAsync();
            return window;
        }

        private static async Task PumpEventsAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }

        private static async Task CloseWindowAsync(Window window)
        {
            if (window == null)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    if (window.IsVisible)
                    {
                        window.Close();
                    }
                },
                DispatcherPriority.Background);

            await PumpEventsAsync();
        }
    }
}
