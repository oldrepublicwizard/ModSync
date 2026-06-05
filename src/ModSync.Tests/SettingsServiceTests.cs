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

                    Assert.Equal(modPath, harness.ModPicker.GetCurrentPath());
                    Assert.Equal(modPath, harness.Step1ModPicker.GetCurrentPath());
                    Assert.Equal(kotorPath, harness.KotorPicker.GetCurrentPath());
                    Assert.Equal(kotorPath, harness.Step1KotorPicker.GetCurrentPath());
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

                    Assert.Equal(updatedPath, harness.ModPicker.GetCurrentPath());
                    Assert.Equal(updatedPath, harness.Step1ModPicker.GetCurrentPath());
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

                    Assert.Equal(modPath, harness.ModPicker.GetCurrentPath());
                    Assert.Equal(modPath, harness.Step1ModPicker.GetCurrentPath());
                    Assert.Equal(kotorPath, harness.KotorPicker.GetCurrentPath());
                    Assert.Equal(kotorPath, harness.Step1KotorPicker.GetCurrentPath());
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
                ModPicker = new DirectoryPickerControl { Name = "ModDirectoryPicker" };
                KotorPicker = new DirectoryPickerControl { Name = "KotorDirectoryPicker" };
                Step1ModPicker = new DirectoryPickerControl { Name = "Step1ModDirectoryPicker" };
                Step1KotorPicker = new DirectoryPickerControl { Name = "Step1KotorDirectoryPicker" };

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
