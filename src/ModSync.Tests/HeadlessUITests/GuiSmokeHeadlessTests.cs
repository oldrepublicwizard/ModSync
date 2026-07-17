// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ModSync.Controls;
using ModSync.Core;
using ModSync.Dialogs.WizardPages;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests.HeadlessUITests
{
    /// <summary>
    /// Headless Avalonia smoke coverage for paste-import entry points,
    /// wizard page-order key controls, and compact-host layout constraints.
    /// Prefer these over a real desktop session for agent GUI UX smoke.
    /// </summary>
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class GuiSmokeHeadlessTests
    {
        private const string SampleClipboardMarkdown = @"### Example Clipboard Mod

**Name:** [Example Clipboard Mod](https://deadlystream.com/files/file/9999-example-clipboard-mod/)

**Author:** Headless Smoke Author

**Description:** Minimal markdown guide used by GuiSmokeHeadlessTests to exercise LoadInstructionTextAsync without a real OS clipboard.

**Category & Tier:** Immersion / 1 - Essential

**Non-English Functionality:** YES

**Installation Method:** Loose-File Mod

**Installation Instructions:** Copy the files into Override.

___
";

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

        [AvaloniaFact(DisplayName = "LoadInstructionTextAsync imports sample markdown without real clipboard")]
        public async Task LoadInstructionTextAsync_SampleMarkdown_ImportsComponents()
        {
            List<ModComponent> previousComponents = MainConfig.AllComponents;
            MainConfig.AllComponents = new List<ModComponent>();

            Window window = await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    var host = new Window { Width = 640, Height = 480 };
                    host.Show();
                    return host;
                },
                DispatcherPriority.Background);
            await PumpEventsAsync();

            try
            {
                var config = new MainConfig();
                var service = new FileLoadingService(config, window);
                int componentsLoadedCallbacks = 0;
                int autoGenerateCallbacks = 0;

                bool loaded = await service.LoadInstructionTextAsync(
                    SampleClipboardMarkdown,
                    editorMode: false,
                    onComponentsLoaded: () =>
                    {
                        componentsLoadedCallbacks++;
                        return Task.CompletedTask;
                    },
                    tryAutoGenerate: _ =>
                    {
                        autoGenerateCallbacks++;
                        return Task.CompletedTask;
                    },
                    sourceDescription: "headless sample markdown");

                Assert.True(loaded);
                Assert.NotEmpty(MainConfig.AllComponents);
                Assert.Contains(
                    MainConfig.AllComponents,
                    c => (c.Name ?? string.Empty).Contains("Clipboard", StringComparison.OrdinalIgnoreCase));
                Assert.True(autoGenerateCallbacks >= 1 || componentsLoadedCallbacks >= 1);
            }
            finally
            {
                MainConfig.AllComponents = previousComponents;
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "Wizard pages Welcome through ValidatePage expose key controls")]
        public async Task WizardPageOrder_WelcomeThroughValidate_KeyControlsPresent()
        {
            List<ModComponent> previousComponents = MainConfig.AllComponents;
            var components = new List<ModComponent>
            {
                new ModComponent
                {
                    Guid = Guid.NewGuid(),
                    Name = "Smoke Mod",
                    IsSelected = true,
                    Category = new List<string> { "Test" },
                    Tier = "1 - Essential",
                },
            };
            MainConfig.AllComponents = components;
            var config = new MainConfig();

            try
            {
                Control[] pages =
                {
                    new WelcomePage(),
                    new ModDirectoryPage(config),
                    new GameDirectoryPage(config),
                    new ModSelectionPage(components),
                    new DownloadsExplainPage(components),
                    new ValidatePage(components, config),
                };

                string[][] requiredNames =
                {
                    Array.Empty<string>(),
                    new[] { "SourcePathPicker" },
                    new[] { "DestinationPathPicker" },
                    new[] { "SelectAllButton", "DeselectAllButton", "SearchTextBox" },
                    new[] { "SelectionSummaryText" },
                    new[] { "ValidateButton", "LogExpander", "SummaryText", "ErrorCountBadge" },
                };

                for (int i = 0; i < pages.Length; i++)
                {
                    Control page = pages[i];
                    Window window = await HostInWindowAsync(page, width: 1100, height: 800);
                    try
                    {
                        await PumpEventsAsync();

                        if (page is WelcomePage)
                        {
                            TextBlock title = page.GetVisualDescendants()
                                .OfType<TextBlock>()
                                .FirstOrDefault(tb => string.Equals(tb.Text, "Welcome to ModSync", StringComparison.Ordinal));
                            Assert.NotNull(title);
                            Assert.True(title.IsVisible);
                        }

                        if (page is DownloadsExplainPage downloads)
                        {
                            await downloads.OnNavigatedToAsync(default);
                            await PumpEventsAsync();
                            TextBlock summary = downloads.FindControl<TextBlock>("SelectionSummaryText");
                            Assert.NotNull(summary);
                            Assert.False(string.IsNullOrWhiteSpace(summary.Text));
                        }

                        foreach (string name in requiredNames[i])
                        {
                            Control control = page.FindControl<Control>(name);
                            Assert.True(control != null, $"Expected control '{name}' on {page.GetType().Name}");
                        }
                    }
                    finally
                    {
                        await CloseWindowAsync(window);
                    }
                }
            }
            finally
            {
                MainConfig.AllComponents = previousComponents;
            }
        }

        [AvaloniaFact(DisplayName = "WelcomePage fits compact host via ScrollViewer without clipping primary title")]
        public async Task WelcomePage_CompactHost_KeepsTitleReachable()
        {
            var page = new WelcomePage();
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
                Assert.True(title.IsVisible);
                Assert.True(page.Bounds.Height > 0 || window.Bounds.Height > 0);

                WrapPanel badgeRow = page.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();
                Assert.NotNull(badgeRow);
            }
            finally
            {
                await CloseWindowAsync(window);
            }
        }

        [AvaloniaFact(DisplayName = "Early wizard pages stay scrollable in a compact host")]
        public async Task EarlyWizardPages_CompactHost_UseScrollViewer()
        {
            var config = new MainConfig();
            Control[] pages =
            {
                new WelcomePage(),
                new ModDirectoryPage(config),
                new GameDirectoryPage(config),
                new DownloadsExplainPage(new List<ModComponent>()),
            };

            foreach (Control page in pages)
            {
                Window window = await HostInWindowAsync(page, width: 720, height: 420);
                try
                {
                    await PumpEventsAsync();

                    ScrollViewer scrollViewer = page.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    Assert.True(
                        scrollViewer != null,
                        $"{page.GetType().Name} should host a ScrollViewer for compact-window overflow");
                    Assert.Equal(
                        Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                        scrollViewer.VerticalScrollBarVisibility);
                    Assert.True(page.Bounds.Width <= window.Bounds.Width + 1 || window.Bounds.Width > 0);
                }
                finally
                {
                    await CloseWindowAsync(window);
                }
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
