// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using ModSync.Controls;
using ModSync.Core;
using ModSync.Models;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class SettingsServiceTests
    {
        [AvaloniaFact(DisplayName = "UpdateDirectoryPickersFromSettings applies source and destination paths")]
        public async Task UpdateDirectoryPickersFromSettings_SetsRegisteredPickerPaths()
        {
            string modPath = CreateTempDirectory();
            string kotorPath = CreateTempDirectory();

            try
            {
                PickerHarness harness = await CreatePickerHarnessAsync();
                try
                {
                    var settings = new AppSettings
                    {
                        SourcePath = modPath,
                        DestinationPath = kotorPath,
                    };

                    await RunOnUiThreadAsync(() =>
                        SettingsService.UpdateDirectoryPickersFromSettings(settings, harness.FindControl));
                    await PumpAsync();

                    AssertPickerShowsPath(harness.ModPicker, modPath);
                    AssertPickerShowsPath(harness.Step1ModPicker, modPath);
                    AssertPickerShowsPath(harness.KotorPicker, kotorPath);
                    AssertPickerShowsPath(harness.Step1KotorPicker, kotorPath);
                }
                finally
                {
                    await CloseWindowAsync(harness.Window);
                }
            }
            finally
            {
                TryDeleteDirectory(modPath);
                TryDeleteDirectory(kotorPath);
            }
        }

        [AvaloniaFact(DisplayName = "SyncDirectoryPickers updates main and Step1 mod pickers")]
        public async Task SyncDirectoryPickers_ModDirectory_UpdatesBothPickers()
        {
            string updatedPath = CreateTempDirectory();

            try
            {
                PickerHarness harness = await CreatePickerHarnessAsync();
                try
                {
                    await RunOnUiThreadAsync(() =>
                        SettingsService.SyncDirectoryPickers(
                            DirectoryPickerType.ModDirectory,
                            updatedPath,
                            harness.FindControl));
                    await PumpAsync();

                    AssertPickerShowsPath(harness.ModPicker, updatedPath);
                    AssertPickerShowsPath(harness.Step1ModPicker, updatedPath);
                }
                finally
                {
                    await CloseWindowAsync(harness.Window);
                }
            }
            finally
            {
                TryDeleteDirectory(updatedPath);
            }
        }

        [AvaloniaFact(DisplayName = "SyncDirectoryPickers updates main and Step1 kotor pickers")]
        public async Task SyncDirectoryPickers_KotorDirectory_UpdatesBothPickers()
        {
            string updatedPath = CreateTempDirectory();

            try
            {
                PickerHarness harness = await CreatePickerHarnessAsync();
                try
                {
                    await RunOnUiThreadAsync(() =>
                        SettingsService.SyncDirectoryPickers(
                            DirectoryPickerType.KotorDirectory,
                            updatedPath,
                            harness.FindControl));
                    await PumpAsync();

                    AssertPickerShowsPath(harness.KotorPicker, updatedPath);
                    AssertPickerShowsPath(harness.Step1KotorPicker, updatedPath);
                }
                finally
                {
                    await CloseWindowAsync(harness.Window);
                }
            }
            finally
            {
                TryDeleteDirectory(updatedPath);
            }
        }

        [AvaloniaFact(DisplayName = "InitializeDirectoryPickers seeds paths from MainConfig")]
        public async Task InitializeDirectoryPickers_UsesMainConfigPaths()
        {
            string modPath = CreateTempDirectory();
            string kotorPath = CreateTempDirectory();

            try
            {
                MainConfigStaticState.Reset();
                PickerHarness harness = await CreatePickerHarnessAsync();
                try
                {
                    var config = new MainConfig
                    {
                        sourcePath = new DirectoryInfo(modPath),
                        destinationPath = new DirectoryInfo(kotorPath),
                    };
                    var service = new SettingsService(config, harness.Window);

                    await RunOnUiThreadAsync(() => service.InitializeDirectoryPickers(harness.FindControl));
                    await PumpAsync();

                    AssertPickerShowsPath(harness.ModPicker, modPath);
                    AssertPickerShowsPath(harness.Step1ModPicker, modPath);
                    AssertPickerShowsPath(harness.KotorPicker, kotorPath);
                    AssertPickerShowsPath(harness.Step1KotorPicker, kotorPath);
                }
                finally
                {
                    await CloseWindowAsync(harness.Window);
                    MainConfigStaticState.Reset();
                }
            }
            finally
            {
                TryDeleteDirectory(modPath);
                TryDeleteDirectory(kotorPath);
            }
        }

        [AvaloniaFact(DisplayName = "UpdateDirectoryPickersFromSettings skips empty path fields")]
        public async Task UpdateDirectoryPickersFromSettings_EmptyPaths_LeavesPickersUnchanged()
        {
            string modPath = CreateTempDirectory();
            string kotorPath = CreateTempDirectory();

            try
            {
                PickerHarness harness = await CreatePickerHarnessAsync();
                try
                {
                    await RunOnUiThreadAsync(() =>
                    {
                        harness.ModPicker.SetCurrentPath(modPath, fireEvent: false);
                        harness.KotorPicker.SetCurrentPath(kotorPath, fireEvent: false);
                        // Avoid ComboBox suggestion virtualization during Avalonia headless reset.
                        ClearPickerSuggestions(harness.ModPicker);
                        ClearPickerSuggestions(harness.KotorPicker);
                    });
                    await PumpAsync();

                    await RunOnUiThreadAsync(() =>
                        SettingsService.UpdateDirectoryPickersFromSettings(new AppSettings(), harness.FindControl));
                    await PumpAsync();

                    AssertPickerShowsPath(harness.ModPicker, modPath);
                    AssertPickerShowsPath(harness.KotorPicker, kotorPath);
                }
                finally
                {
                    await RunOnUiThreadAsync(() =>
                    {
                        ClearPickerSuggestions(harness.ModPicker);
                        ClearPickerSuggestions(harness.KotorPicker);
                        ClearPickerSuggestions(harness.Step1ModPicker);
                        ClearPickerSuggestions(harness.Step1KotorPicker);
                    });
                    await CloseWindowAsync(harness.Window);
                }
            }
            finally
            {
                TryDeleteDirectory(modPath);
                TryDeleteDirectory(kotorPath);
            }
        }

        [AvaloniaFact(DisplayName = "UpdateDirectoryPickersFromSettings tolerates null findControl results")]
        public async Task UpdateDirectoryPickersFromSettings_NullPickers_DoesNotThrow()
        {
            PickerHarness harness = await CreatePickerHarnessAsync();
            try
            {
                var settings = new AppSettings
                {
                    SourcePath = CreateTempDirectory(),
                    DestinationPath = CreateTempDirectory(),
                };

                try
                {
                    await RunOnUiThreadAsync(() =>
                        SettingsService.UpdateDirectoryPickersFromSettings(settings, _ => null));
                    await PumpAsync();
                }
                finally
                {
                    TryDeleteDirectory(settings.SourcePath);
                    TryDeleteDirectory(settings.DestinationPath);
                }
            }
            finally
            {
                await CloseWindowAsync(harness.Window);
            }
        }


        private static void ClearPickerSuggestions(DirectoryPickerControl picker)
        {
            ComboBox suggestions = picker?.FindControl<ComboBox>("PathSuggestions");
            if (suggestions != null)
            {
                suggestions.ItemsSource = null;
                suggestions.SelectedItem = null;
            }
        }

        private static void AssertPickerShowsPath(DirectoryPickerControl picker, string expectedPath)
        {
            Assert.Equal(expectedPath, picker.GetCurrentPath());

            TextBox pathInput = picker.FindControl<TextBox>("PathInput");
            Assert.NotNull(pathInput);
            Assert.Equal(expectedPath, pathInput.Text);
        }

        private static async Task RunOnUiThreadAsync(Action action)
        {
            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    action();
                },
                DispatcherPriority.Background);
        }

        private static async Task<PickerHarness> CreatePickerHarnessAsync()
        {
            return await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    var harness = new PickerHarness();
                    harness.Window = new Window
                    {
                        Content = new Panel
                        {
                            Children =
                            {
                                harness.ModPicker,
                                harness.KotorPicker,
                                harness.Step1ModPicker,
                                harness.Step1KotorPicker,
                            },
                        },
                    };
                    harness.Window.Show();
                    return harness;
                },
                DispatcherPriority.Background);
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
            await PumpAsync();
        }

        private static async Task PumpAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            await Task.Delay(5);
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "modsync-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private sealed class PickerHarness
        {
            public PickerHarness()
            {
                ModPicker = new DirectoryPickerControl
                {
                    Name = "ModDirectoryPicker",
                    PickerType = DirectoryPickerType.ModDirectory,
                };
                KotorPicker = new DirectoryPickerControl
                {
                    Name = "KotorDirectoryPicker",
                    PickerType = DirectoryPickerType.KotorDirectory,
                };
                Step1ModPicker = new DirectoryPickerControl
                {
                    Name = "Step1ModDirectoryPicker",
                    PickerType = DirectoryPickerType.ModDirectory,
                };
                Step1KotorPicker = new DirectoryPickerControl
                {
                    Name = "Step1KotorDirectoryPicker",
                    PickerType = DirectoryPickerType.KotorDirectory,
                };

                Controls = new Dictionary<string, DirectoryPickerControl>(StringComparer.Ordinal)
                {
                    ["ModDirectoryPicker"] = ModPicker,
                    ["KotorDirectoryPicker"] = KotorPicker,
                    ["Step1ModDirectoryPicker"] = Step1ModPicker,
                    ["Step1KotorDirectoryPicker"] = Step1KotorPicker,
                };
            }

            public Window Window { get; set; }

            public DirectoryPickerControl ModPicker { get; }

            public DirectoryPickerControl KotorPicker { get; }

            public DirectoryPickerControl Step1ModPicker { get; }

            public DirectoryPickerControl Step1KotorPicker { get; }

            private Dictionary<string, DirectoryPickerControl> Controls { get; }

            public DirectoryPickerControl FindControl(string name) =>
                Controls.TryGetValue(name, out DirectoryPickerControl picker) ? picker : null;
        }
    }
}
