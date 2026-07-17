// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

using ModSync.Core;
using ModSync.Core.Utility;
using ModSync.Core.Services;
using ModSync.Core.Services.Download;
using ModSync.Dialogs;

using ReactiveUI;

using static ModSync.Core.Services.ModManagementService;

namespace ModSync.Services
{

    public sealed class TopMenuCallbacks
    {
        public bool EditorMode { get; set; }
        public Action ToggleEditorMode { get; set; }
        public INotifyPropertyChanged EditorModePropertySource { get; set; }
        public Func<bool> GetEditorMode { get; set; }
        public Action OnOpenFile { get; set; }
        public Action OnCloseToml { get; set; }
        public Action OnSave { get; set; }
        public Action OnExit { get; set; }
        public Action OnFixIosCaseSensitivity { get; set; }
        public Action OnFixPathPermissions { get; set; }
        public Action OnOpenCheckpoints { get; set; }
        public Action OnRunHolopatcher { get; set; }
        public Action OnOpenSettings { get; set; }
        public Action OnOpenOutputLog { get; set; }
        public Action OnResolveDuplicateFilesAndFolders { get; set; }
    }

    public sealed class TopMenuBuildResult
    {
        public Menu RootMenu { get; set; }
        public MenuItem CloseFileMenuItem { get; set; }
        public MenuItem SaveMenuItem { get; set; }
        public MenuItem EditorModeMenuItem { get; set; }
    }


    public class MenuBuilderService
    {
        private readonly ModManagementService _modManagementService;
        private readonly Window _parentWindow;

        public MenuBuilderService(ModManagementService modManagementService, Window parentWindow)
        {
            _modManagementService = modManagementService ?? throw new ArgumentNullException(nameof(modManagementService));
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
        }

        public ContextMenu BuildContextMenuForComponent(
            ModComponent component,
            bool editorMode,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            TabItem rawEditTab,
            Action<ModComponent> onComponentSelectionChanged,
            Action<ModComponent, int> onMoveRelative,
            Action<object, object> onRemoveComponent,
            Action<object, object> onInstallSingle)
        {
            var contextMenu = new ContextMenu();

            if (component is null)
            {
                return contextMenu;
            }

            contextMenu.Items.Add(new MenuItem
            {
                Header = component.IsSelected ? "☑️ Deselect Mod" : "☐ Select Mod",
                Command = ReactiveCommand.Create(() =>
                {
                    component.IsSelected = !component.IsSelected;
                    onComponentSelectionChanged?.Invoke(component);
                }),
            });

            if (editorMode)
            {
                AddEditorModeMenuItems(contextMenu, component, setCurrentComponent, setTab, tabControl, guiEditTab, rawEditTab, onMoveRelative, onRemoveComponent, onInstallSingle);
            }

            return contextMenu;
        }

        public void BuildGlobalActionsFlyout(
            MenuFlyout menu,
            bool editorMode,
            Action onRefresh,
            Func<Task> onGenerateInstructions,
            Func<Task> onLockInstallOrder,
            Func<Task> onRemoveAllDependencies,
            Func<ModComponent> onCreate,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            Func<Task> onShowModManagement,
            Func<Task> onShowStats,
            Action<object, object> onSave,
            Action<object, object> onClose)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => BuildGlobalActionsFlyout(menu, editorMode, onRefresh, onGenerateInstructions, onLockInstallOrder, onRemoveAllDependencies, onCreate, setCurrentComponent, setTab, tabControl, guiEditTab, onShowModManagement, onShowStats, onSave, onClose), DispatcherPriority.Normal);
                return;
            }
            menu.Items.Clear();

            AddCommonMenuItems(menu.Items, onRefresh);

            menu.Items.Add(new MenuItem
            {
                Header = "🤖 Generate Instructions from ModLinks",
                Command = ReactiveCommand.CreateFromTask(onGenerateInstructions),
            });

            menu.Items.Add(new MenuItem
            {
                Header = "🔒 Lock Install Order",
                Command = ReactiveCommand.CreateFromTask(onLockInstallOrder),
            });

            menu.Items.Add(new MenuItem
            {
                Header = "🗑️ Remove All Dependencies",
                Command = ReactiveCommand.CreateFromTask(onRemoveAllDependencies),
            });

            menu.Items.Add(new Separator());

            if (editorMode)
            {
                AddEditorModeFlyoutItems(menu, onCreate, setCurrentComponent, setTab, tabControl, guiEditTab, onShowModManagement, onShowStats, onSave, onClose);
            }
        }

        public void BuildGlobalActionsContextMenu(
            ContextMenu menu,
            bool editorMode,
            Action onRefresh,
            Func<Task> onValidateAll,
            Func<ModComponent> onCreate,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            Func<Task> onShowModManagement,
            Func<Task> onShowStats,
            Action<object, object> onSave,
            Action<object, object> onClose)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => BuildGlobalActionsContextMenu(menu, editorMode, onRefresh, onValidateAll, onCreate, setCurrentComponent, setTab, tabControl, guiEditTab, onShowModManagement, onShowStats, onSave, onClose), DispatcherPriority.Normal);
                return;
            }
            menu.Items.Clear();

            AddCommonMenuItems(menu.Items, onRefresh);

            menu.Items.Add(new Separator());

            if (editorMode)
            {
                AddEditorModeContextMenuItems(menu.Items, onCreate, setCurrentComponent, setTab, tabControl, guiEditTab, onShowModManagement, onSave, onClose);
            }
        }

        #region Private Helper Methods

        private void AddEditorModeMenuItems(
            ContextMenu contextMenu,
            ModComponent component,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            TabItem rawEditTab,
            Action<ModComponent, int> onMoveRelative,
            Action<object, object> onRemoveComponent,
            Action<object, object> onInstallSingle)
        {
            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem
            {
                Header = "⬆️ Move Up",
                Command = ReactiveCommand.Create(() => onMoveRelative(component, -1)),
                InputGesture = new KeyGesture(Key.Up, KeyModifiers.Control),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "⬇️ Move Down",
                Command = ReactiveCommand.Create(() => onMoveRelative(component, 1)),
                InputGesture = new KeyGesture(Key.Down, KeyModifiers.Control),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "📊 Move to Top",
                Command = ReactiveCommand.Create(() => _modManagementService.MoveModToPosition(component, 0)),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "📊 Move to Bottom",
                Command = ReactiveCommand.Create(() => _modManagementService.MoveModToPosition(component, MainConfig.AllComponents.Count - 1)),
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem
            {
                Header = "🗑️ Delete Mod",
                Command = ReactiveCommand.CreateFromTask(async () =>
                {
                    setCurrentComponent(component);
                    bool? confirm = await ConfirmationDialog.ShowConfirmationDialogAsync(
                        _parentWindow,
                        confirmText: $"Are you sure you want to delete the mod '{component.Name}'? This action cannot be undone.",
                        yesButtonText: "Delete",
                        noButtonText: "Cancel",
                        yesButtonTooltip: "Delete the mod.",
                        noButtonTooltip: "Cancel the deletion of the mod.",
                        closeButtonTooltip: "Cancel the deletion of the mod."
                    );

                    if (confirm == true)
                    {
                        onRemoveComponent(arg1: null, arg2: null);
                    }
                }),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "🔄 Duplicate Mod",
                Command = ReactiveCommand.Create(() =>
                {
                    ModComponent duplicated = _modManagementService.DuplicateMod(component);
                    if (duplicated != null)
                    {
                        setCurrentComponent(duplicated);
                        setTab(tabControl, guiEditTab);
                    }
                }),
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem
            {
                Header = "📝 Edit Instructions",
                Command = ReactiveCommand.Create(() =>
                {
                    setCurrentComponent(component);
                    setTab(tabControl, guiEditTab);
                }),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "📄 Edit Raw TOML",
                Command = ReactiveCommand.Create(() =>
                {
                    setCurrentComponent(component);
                    setTab(tabControl, rawEditTab);
                }),
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem
            {
                Header = "🧪 Test Install This Mod",
                Command = ReactiveCommand.Create(() =>
                {
                    setCurrentComponent(component);
                    onInstallSingle(arg1: null, arg2: null);
                }),
            });

            contextMenu.Items.Add(new MenuItem
            {
                Header = "🔍 Validate Mod Files",
                Command = ReactiveCommand.Create((Func<Task>)(async () =>
                {
                    ModValidationResult validation = _modManagementService.ValidateMod(component);
                    if (!validation.IsValid)
                    {
                        await InformationDialog.ShowInformationDialogAsync(_parentWindow,
                            $"Validation failed for '{component.Name}':{Environment.NewLine}{Environment.NewLine}" +
                            string.Join("\n", validation.Errors.Take(5)));
                    }
                    else
                    {
                        await InformationDialog.ShowInformationDialogAsync(_parentWindow, $"✅ '{component.Name}' validation passed!");
                    }
                })),
            });
        }

        private void AddCommonMenuItems(ItemCollection items, Action onRefresh)
        {
            _ = items.Add(new MenuItem
            {
                Header = "🔄 Refresh List",
                Command = ReactiveCommand.Create(onRefresh),
                InputGesture = new KeyGesture(Key.F5),
            });

            _ = items.Add(new MenuItem
            {
                Header = "Profiles...",
                Command = ReactiveCommand.CreateFromTask(() => ProfileManagerDialog.ShowProfileManagerDialogAsync(_parentWindow)),
            });

            _ = items.Add(new MenuItem
            {
                Header = "🔄 Validate All Mods",
                Command = ReactiveCommand.CreateFromTask((Func<Task>)(async () =>
                {
                    Dictionary<ModComponent, ModValidationResult> results = _modManagementService.ValidateAllMods();
                    int errorCount = results.Count(r => !r.Value.IsValid);
                    int warningCount = results.Sum(r => r.Value.Warnings.Count);

                    await InformationDialog.ShowInformationDialogAsync(_parentWindow,
                        "Validation complete!\n\n" +
                        $"Errors: {errorCount}\n" +
                        $"Warnings: {warningCount}\n\n" +


                        $"Valid mods: {results.Count(r => r.Value.IsValid)}/{results.Count}");
                })),
            });

            _ = items.Add(new MenuItem
            {
                Header = "⚔️ Analyze File Conflicts",
                Command = ReactiveCommand.CreateFromTask(async () =>
                {
                    try
                    {
                        List<ModComponent> allMods = _modManagementService.SearchMods(string.Empty);
                        (_, List<ModComponent> installOrder) = ModComponent.ConfirmComponentsInstallOrder(allMods);
                        await ConflictsDialog.ShowAnalysisAsync(_parentWindow, installOrder);
                    }
                    catch (KeyNotFoundException ex) when (ex.Message.IndexOf("Circular dependency", StringComparison.Ordinal) >= 0)
                    {
                        await InformationDialog.ShowInformationDialogAsync(
                            _parentWindow,
                            "Cannot analyze file conflicts: circular dependencies prevent a valid install order.\n\n" +
                            "Resolve dependency cycles first, then try again.");
                    }
                }),
            });

            _ = items.Add(new MenuItem
            {
                Header = "🔔 Check for Nexus Updates",
                Command = ReactiveCommand.CreateFromTask(RunNexusUpdateCheckAsync),
            });
        }

        private async Task RunNexusUpdateCheckAsync()
        {
            if (string.IsNullOrWhiteSpace(MainConfig.NexusModsApiKey))
            {
                await InformationDialog.ShowInformationDialogAsync(
                    _parentWindow,
                    "Set a Nexus Mods API key in Settings before checking for updates.");
                return;
            }

            List<ModComponent> mods = _modManagementService.SearchMods(string.Empty);
            using (var httpClient = new HttpClient())
            {
                var apiClient = new NexusApiClient(httpClient, MainConfig.NexusModsApiKey);
                var updateService = new ModUpdateCheckService(apiClient);
                ModUpdateCheckResult result = await updateService.CheckForUpdatesAsync(mods).ConfigureAwait(true);

                foreach (ModComponent component in mods)
                {
                    component.NotifyNexusUpdateStateChanged();
                }

                string summary = string.Format(
                    CultureInfo.InvariantCulture,
                    "Nexus update check complete.\n\nChecked: {0}\nSkipped (non-Nexus): {1}\nUpdates found: {2}",
                    result.CheckedCount,
                    result.SkippedCount,
                    result.UpdatesFound.Count);

                if (result.RateLimitReached)
                {
                    summary += "\n\nStopped early: Nexus API rate limit reached. Unchecked mods keep their previous update badges.";
                }

                if (result.Errors.Count > 0)
                {
                    summary += string.Format(
                        CultureInfo.InvariantCulture,
                        "\n\nErrors ({0}):\n{1}",
                        result.Errors.Count,
                        string.Join("\n", result.Errors.Take(5)));
                    if (result.Errors.Count > 5)
                    {
                        summary += string.Format(CultureInfo.InvariantCulture, "\n… and {0} more.", result.Errors.Count - 5);
                    }
                }

                await InformationDialog.ShowInformationDialogAsync(_parentWindow, summary);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "<Pending>")]
        private void AddEditorModeFlyoutItems(
            MenuFlyout menu,
            Func<ModComponent> onCreate,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            Func<Task> onShowModManagement,
            Func<Task> onShowStats,
            Action<object, object> onSave,
            Action<object, object> onClose)
        {

            menu.Items.Add(new MenuItem
            {
                Header = "➕ Add New Mod",
                Command = ReactiveCommand.Create(() =>
                {
                    ModComponent newMod = onCreate();
                    if (newMod is null)
                    {
                        return;
                    }

                    setCurrentComponent(newMod);
                    setTab(tabControl, guiEditTab);
                }),
            });

            menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem
            {
                Header = "⚙️ Mod Management Tools",
                Command = ReactiveCommand.CreateFromTask(onShowModManagement),
            });

            menu.Items.Add(new MenuItem
            {
                Header = "📈 Mod Statistics",
                Command = ReactiveCommand.CreateFromTask(onShowStats),
            });

            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem
            {
                Header = "💾 Save Config",
                Command = ReactiveCommand.Create(() => onSave(arg1: null, arg2: null)),
                InputGesture = new KeyGesture(Key.S, KeyModifiers.Control),
            });

            menu.Items.Add(new MenuItem
            {
                Header = "❌ Close TOML",
                Command = ReactiveCommand.Create(() => onClose(arg1: null, arg2: null)),
            });
        }

        private void AddEditorModeContextMenuItems(
            ItemCollection items,
            Func<ModComponent> onCreate,
            Action<ModComponent> setCurrentComponent,
            Action<TabControl, TabItem> setTab,
            TabControl tabControl,
            TabItem guiEditTab,
            Func<Task> onShowModManagement,
                        Action<object, object> onSave,
            Action<object, object> onClose)
        {

            _ = items.Add(new MenuItem
            {
                Header = "➕ Add New Mod",
                Command = ReactiveCommand.Create(() =>
                {
                    ModComponent newMod = onCreate();
                    if (newMod is null)
                    {
                        return;
                    }

                    setCurrentComponent(newMod);
                    setTab(tabControl, guiEditTab);
                }),
            });

            items.Add(new Separator());

            items.Add(new MenuItem
            {
                Header = "⚙️ Mod Management Tools",
                Command = ReactiveCommand.CreateFromTask(onShowModManagement),
            });

            items.Add(new MenuItem
            {
                Header = "📈 Mod Statistics",
                Command = ReactiveCommand.CreateFromTask((Func<Task>)(async () =>
                {
                    ModStatistics stats = _modManagementService.GetModStatistics();
                    string statsText = "📊 Mod Statistics\n\n" +
                                       $"Total Mods: {stats.TotalMods}\n" +
                                       $"Selected: {stats.SelectedMods}\n" +
                                       $"Downloaded: {stats.DownloadedMods}\n\n" +
                                       $"Categories:\n{string.Join("\n", stats.Categories.Select(c => $"  • {c.Key}: {c.Value}"))}\n\n" +
                                       $"Tiers:\n{string.Join("\n", stats.Tiers.Select(t => $"  • {t.Key}: {t.Value}"))}\n\n" +
                                       $"Average Instructions/Mod: {stats.AverageInstructionsPerMod:F1}\n" +
                                       $"Average Options/Mod: {stats.AverageOptionsPerMod:F1}";



                    await InformationDialog.ShowInformationDialogAsync(_parentWindow, statsText);
                })),
            });

            items.Add(new Separator());

            items.Add(new MenuItem
            {
                Header = "💾 Save Config",
                Command = ReactiveCommand.Create(() => onSave(arg1: null, arg2: null)),
                InputGesture = new KeyGesture(Key.S, KeyModifiers.Control),
            });

            items.Add(new MenuItem
            {
                Header = "❌ Close TOML",
                Command = ReactiveCommand.Create(() => onClose(arg1: null, arg2: null)),
            });
        }

        public TopMenuBuildResult BuildTopMenu(TopMenuCallbacks callbacks)
        {
            if (callbacks is null)
            {
                throw new ArgumentNullException(nameof(callbacks));
            }

            var menu = new Menu();
            var fileMenu = new MenuItem { Header = "File" };

            var saveMenuItem = new MenuItem
            {
                Header = "Save",
                IsVisible = callbacks.EditorMode,
                Command = ReactiveCommand.Create(() => callbacks.OnSave),
            };

            var fileItems = new List<MenuItem>
        {
            new MenuItem
            {
                Header = "Open File",
                Command = ReactiveCommand.Create( () => callbacks.OnOpenFile ),
            },
            new MenuItem
            {
                Header = "Close",
                Command = ReactiveCommand.Create( () => callbacks.OnCloseToml ),
                IsVisible = callbacks.EditorMode,
            },
            saveMenuItem,
            new MenuItem
            {
                Header = "Exit",
                Command = ReactiveCommand.Create( () => callbacks.OnExit ),
            },
        };
            fileMenu.ItemsSource = fileItems;

            var toolsMenu = new MenuItem { Header = "Tools" };

            // Editor Mode checkbox menu item
            var editorModeMenuItem = new MenuItem
            {
                Header = callbacks.EditorMode ? "✓ Editor Mode" : "Editor Mode",
            };
            editorModeMenuItem.Click += (s, e) =>
            {
                callbacks.ToggleEditorMode();
            };

            // Subscribe to EditorMode changes to keep menu item in sync
            callbacks.EditorModePropertySource.PropertyChanged += (s, e) =>
            {
                if (string.Equals(e.PropertyName, "EditorMode", StringComparison.Ordinal))
                {
                    editorModeMenuItem.Header = callbacks.EditorMode ? "✓ Editor Mode" : "Editor Mode";
                }
            };

            var toolItems = new List<MenuItem>
            {
                editorModeMenuItem,
                new MenuItem { Header = "-" }, // Separator
                new MenuItem
                {
                    Header = "Fix iOS case sensitivity.",
                    Command = ReactiveCommand.Create( () => callbacks.OnFixIosCaseSensitivity ),
                },
                new MenuItem
                {
                    Header = "Fix file/folder permissions.",
                    Command = ReactiveCommand.Create( () => callbacks.OnFixPathPermissions ),
                },
                new MenuItem
                {
                    Header = "Manage Checkpoints",
                    Command = ReactiveCommand.Create( () => callbacks.OnOpenCheckpoints ),
                },
                new MenuItem
                {
                    Header = "Run HoloPatcher",
                    Command = ReactiveCommand.Create( () => callbacks.OnRunHolopatcher ),
                },
                new MenuItem
                {
                    Header = "Settings",
                    Command = ReactiveCommand.Create( () => callbacks.OnOpenSettings ),
                },
                new MenuItem
                {
                    Header = "Show Output Log",
                    Command = ReactiveCommand.Create( () => callbacks.OnOpenOutputLog ),
                },
            };
            ToolTip.SetTip(
                editorModeMenuItem,
                value:
                "Toggle to enable Editor Mode: exposes Raw/Editor tabs, editing buttons, and creation tools. When off, the UI is simplified for end users installing mods."
            );
            ToolTip.SetTip(
                toolItems[2],
                value:
                "Lowercase all files/folders recursively at the given path. Necessary for iOS installs."
            );
            ToolTip.SetTip(
                toolItems[3],
                value:
                "Fixes various file/folder permissions. On Unix, this will also find case-insensitive duplicate file/folder names."
            );
            if (UtilityHelper.GetOperatingSystem() != OSPlatform.Windows)
            {
                var filePermFixTool = new MenuItem
                {
                    Header = "Fix file and folder permissions",
                    Command = ReactiveCommand.Create(() => callbacks.OnResolveDuplicateFilesAndFolders),
                };
                ToolTip.SetTip(
                    filePermFixTool,
                    "(Linux/Mac only) This will acquire a list of any case-insensitive duplicates in the mod workspace or"
                    + " the kotor directory, including subfolders, and resolve them."
                );
                toolItems.Add(filePermFixTool);
            }
            toolsMenu.ItemsSource = toolItems;

            var helpMenu = new MenuItem { Header = "Help" };
            var deadlystreamMenu = new MenuItem
            {
                Header = "DeadlyStream",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "Discord",
                        Command = ReactiveCommand.Create(() => UrlUtilities.OpenUrl("https://discord.gg/nDkHXfc36s")),
                    },
                    new MenuItem
                    {
                        Header = "Website",
                        Command = ReactiveCommand.Create(() => UrlUtilities.OpenUrl("https://deadlystream.com")),
                    },
                },
            };
            var neocitiesMenu = new MenuItem
            {
                Header = "KOTOR Community Portal",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "Discord",
                        Command = ReactiveCommand.Create(() => UrlUtilities.OpenUrl("https://discord.com/invite/kotor")),
                    },
                    new MenuItem
                    {
                        Header = "Website",
                        Command = ReactiveCommand.Create(() => UrlUtilities.OpenUrl("https://kotor.neocities.org")),
                    },
                },
            };
            var pcgamingwikiMenu = new MenuItem
            {
                Header = "PCGamingWiki",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "KOTOR 1",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl( "https://www.pcgamingwiki.com/wiki/Star_Wars:_Knights_of_the_Old_Republic" ) ),
                    },
                    new MenuItem
                    {
                        Header = "KOTOR 2: TSL",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl( "https://www.pcgamingwiki.com/wiki/Star_Wars:_Knights_of_the_Old_Republic_II_-_The_Sith_Lords" ) ),
                    },
                },
            };
            helpMenu.ItemsSource = new[] { deadlystreamMenu, neocitiesMenu, pcgamingwikiMenu };

            var engineRewritesMenu = new MenuItem
            {
                Header = "Open-Source Odyssey/Aurora Engines",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "KotOR.js",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/KobaltBlu/KotOR.js") ),
                    },
                    new MenuItem
                    {
                        Header = "NorthernLights",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/lachjames/NorthernLights") ),
                    },
                    new MenuItem
                    {
                        Header = "reone",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/seedhartha/reone") ),
                    },
                },
            };
            var otherProjectsMenu = new MenuItem
            {
                Header = "Other Projects",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "PyKotor Library",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/NickHugi/PyKotor") ),
                        ItemsSource = new []
                        {
                            new MenuItem
                            {
                                Header = "HoloPatcher",
                                ItemsSource = new []
                                {
                                    new MenuItem
                                    {
                                        Header = "DeadlyStream",
                                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/2243-holopatcher") ),
                                    },
                                    new MenuItem
                                    {
                                        Header = "GitHub",
                                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/NickHugi/PyKotor") ),
                                    },
                                },
                            },
                            new MenuItem
                            {
                                Header = "Holocron Toolset",
                                ItemsSource = new []
                                {
                                    new MenuItem
                                    {
                                        Header = "GitHub",
                                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/NickHugi/PyKotor/blob/master/Tools/HolocronToolset") ),
                                    },
                                    new MenuItem
                                    {
                                        Header = "DeadlyStream",
                                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/1982-holocron-toolset") ),
                                    },
                                    new MenuItem
                                    {
                                        Header = "Discord",
                                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://discord.gg/hfAqtkVEzQ") ),
                                    },
                                },
                            },
                            new MenuItem
                            {
                                Header = "Auto-Translate / Font Creator",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/2375-kotor-autotranslate-tool") ),
                            },
                            new MenuItem
                            {
                                Header = "KotorDiff",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/2364-kotordiff") ),
                            },
                        },
                    },
                    new MenuItem
                    {
                        Header = "LIP Composer / reone toolkit",
                        ItemsSource = new []
                        {
                            new MenuItem
                            {
                                Header = "DeadlyStream",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/1862-reone-toolkit") ),
                            },
                            new MenuItem
                            {
                                Header = "GitHub",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/seedhartha/reone/wiki/Tooling") ),
                            },
                        },
                    },
                    engineRewritesMenu,
                },
            };
            var aboutMenu = new MenuItem
            {
                Header = "About",
                ItemsSource = new[]
                {
                    new MenuItem
                    {
                        Header = "The ModSync Project",
                        ItemsSource = new []
                        {
                            new MenuItem
                            {
                                Header = "DeadlyStream",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/2317-kotormodsync/") ),
                            },
                            new MenuItem
                            {
                                Header = "GitHub",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/th3w1zard1/ModSync") ),
                            },
                        },
                    },
                    new MenuItem
                    {
                        Header = "HoloPatcher",
                        ItemsSource = new []
                        {
                            new MenuItem
                            {
                                Header = "DeadlyStream",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://deadlystream.com/files/file/2243-holopatcher") ),
                            },
                            new MenuItem
                            {
                                Header = "GitHub",
                                Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl("https://github.com/NickHugi/PyKotor") ),
                            },
                        },
                    },
                },
            };
            var moreMenu = new MenuItem
            {
                Header = "More",
                ItemsSource = new[]
                {
                    otherProjectsMenu,
                    new MenuItem
                    {
                        Header = "Modding Tools",
                        Command = ReactiveCommand.Create( () => UrlUtilities.OpenUrl(url: "https://deadlystream.github.io/ds-kotor-modding-wiki/en/#!pages/tools_overview.md") ),
                    },
                },
            };
            menu.ItemsSource = new[] { fileMenu, toolsMenu, helpMenu, aboutMenu, moreMenu };
            return new TopMenuBuildResult
            {
                RootMenu = menu,
                CloseFileMenuItem = (MenuItem)fileItems[1],
                SaveMenuItem = saveMenuItem,
                EditorModeMenuItem = editorModeMenuItem,
            };

        }

        public void UpdateTopMenuVisibility(TopMenuBuildResult handles, bool editorMode)
        {
            if (handles is null)
            {
                throw new ArgumentNullException(nameof(handles));
            }

            handles.CloseFileMenuItem.IsVisible = editorMode;
            handles.SaveMenuItem.IsVisible = editorMode;
            handles.EditorModeMenuItem.IsVisible = editorMode;
        }


        #endregion
    }
}
