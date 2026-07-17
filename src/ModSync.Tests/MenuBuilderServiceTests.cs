// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class MenuBuilderServiceTests
    {
        [AvaloniaFact(DisplayName = "BuildContextMenuForComponent non-editor mode exposes only select toggle")]
        public void BuildContextMenuForComponent_NonEditor_HasSelectOnly()
        {
            MenuBuilderService service = CreateService(out Window window);
            var component = new ModComponent { Name = "Test Mod", IsSelected = false };

            try
            {
                ContextMenu menu = service.BuildContextMenuForComponent(
                    component,
                    editorMode: false,
                    _ => { },
                    (_, __) => { },
                    new TabControl(),
                    new TabItem(),
                    new TabItem(),
                    _ => { },
                    (_, __) => { },
                    (_, __) => { },
                    (_, __) => { });

                List<string> headers = GetMenuHeaders(menu);
                Assert.Single(headers);
                Assert.Equal("☐ Select Mod", headers[0]);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "BuildContextMenuForComponent editor mode includes move and validate actions")]
        public void BuildContextMenuForComponent_Editor_IncludesExpectedHeaders()
        {
            MenuBuilderService service = CreateService(out Window window);
            var component = new ModComponent { Name = "Editor Mod", IsSelected = true };

            try
            {
                ContextMenu menu = service.BuildContextMenuForComponent(
                    component,
                    editorMode: true,
                    _ => { },
                    (_, __) => { },
                    new TabControl(),
                    new TabItem(),
                    new TabItem(),
                    _ => { },
                    (_, __) => { },
                    (_, __) => { },
                    (_, __) => { });

                List<string> headers = GetMenuHeaders(menu);
                Assert.Contains("☑️ Deselect Mod", headers);
                Assert.Contains("⬆️ Move Up", headers);
                Assert.Contains("⬇️ Move Down", headers);
                Assert.Contains("🗑️ Delete Mod", headers);
                Assert.Contains("🔍 Validate Mod Files", headers);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "BuildGlobalActionsFlyout includes Nexus update check in common items")]
        public void BuildGlobalActionsFlyout_IncludesNexusUpdateCheckItem()
        {
            MenuBuilderService service = CreateService(out Window window);
            var flyout = new MenuFlyout();

            try
            {
                service.BuildGlobalActionsFlyout(
                    flyout,
                    editorMode: false,
                    () => { },
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    () => new ModComponent { Name = "New" },
                    _ => { },
                    (_, __) => { },
                    new TabControl(),
                    new TabItem(),
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    (_, __) => { },
                    (_, __) => { });

                List<string> headers = GetMenuHeaders(flyout.Items);
                Assert.Contains("🔔 Check for Nexus Updates", headers);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "BuildGlobalActionsFlyout editor mode includes bulk actions before editor tools")]
        public void BuildGlobalActionsFlyout_Editor_IncludesBulkActions()
        {
            MenuBuilderService service = CreateService(out Window window);
            var flyout = new MenuFlyout();

            try
            {
                service.BuildGlobalActionsFlyout(
                    flyout,
                    editorMode: true,
                    () => { },
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    () => new ModComponent { Name = "New" },
                    _ => { },
                    (_, __) => { },
                    new TabControl(),
                    new TabItem(),
                    () => Task.CompletedTask,
                    () => Task.CompletedTask,
                    (_, __) => { },
                    (_, __) => { });

                List<string> headers = GetMenuHeaders(flyout.Items);
                int generateIndex = headers.IndexOf("🤖 Generate Instructions from ModLinks");
                int lockIndex = headers.IndexOf("🔒 Lock Install Order");
                int removeDepsIndex = headers.IndexOf("🗑️ Remove All Dependencies");
                int addModIndex = headers.IndexOf("➕ Add New Mod");

                Assert.True(generateIndex >= 0);
                Assert.True(lockIndex > generateIndex);
                Assert.True(removeDepsIndex > lockIndex);
                Assert.True(addModIndex > removeDepsIndex);
                Assert.Contains("💾 Save Config", headers);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "BuildTopMenu exposes File Tools Help About and More root menus")]
        public void BuildTopMenu_IncludesExpectedRootMenus()
        {
            MenuBuilderService service = CreateService(out Window window);
            bool editorMode = false;

            try
            {
                TopMenuBuildResult result = service.BuildTopMenu(new TopMenuCallbacks
                {
                    EditorMode = editorMode,
                    ToggleEditorMode = () => editorMode = !editorMode,
                    EditorModePropertySource = window,
                    GetEditorMode = () => editorMode,
                    OnOpenFile = () => { },
                    OnCloseToml = () => { },
                    OnSave = () => { },
                    OnExit = () => { },
                    OnFixIosCaseSensitivity = () => { },
                    OnFixPathPermissions = () => { },
                    OnOpenCheckpoints = () => { },
                    OnRunHolopatcher = () => { },
                    OnOpenSettings = () => { },
                    OnOpenOutputLog = () => { },
                    OnResolveDuplicateFilesAndFolders = () => { },
                });

                List<string> rootHeaders = GetMenuHeaders(result.RootMenu.Items);
                Assert.Equal(new[] { "File", "Tools", "Help", "About", "More" }, rootHeaders);

                MenuItem toolsMenu = result.RootMenu.Items.OfType<MenuItem>().First(item => item.Header as string == "Tools");
                List<string> toolHeaders = GetMenuHeaders(toolsMenu.Items);
                Assert.Contains("Editor Mode", toolHeaders);
                Assert.Contains("Settings", toolHeaders);
                Assert.Contains("Show Output Log", toolHeaders);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "UpdateTopMenuVisibility toggles editor-only menu items")]
        public void UpdateTopMenuVisibility_TogglesEditorOnlyItems()
        {
            MenuBuilderService service = CreateService(out Window window);
            bool editorMode = false;

            try
            {
                TopMenuBuildResult result = service.BuildTopMenu(new TopMenuCallbacks
                {
                    EditorMode = editorMode,
                    ToggleEditorMode = () => editorMode = !editorMode,
                    EditorModePropertySource = window,
                    GetEditorMode = () => editorMode,
                    OnOpenFile = () => { },
                    OnCloseToml = () => { },
                    OnSave = () => { },
                    OnExit = () => { },
                    OnFixIosCaseSensitivity = () => { },
                    OnFixPathPermissions = () => { },
                    OnOpenCheckpoints = () => { },
                    OnRunHolopatcher = () => { },
                    OnOpenSettings = () => { },
                    OnOpenOutputLog = () => { },
                    OnResolveDuplicateFilesAndFolders = () => { },
                });

                service.UpdateTopMenuVisibility(result, editorMode: false);
                Assert.False(result.CloseFileMenuItem.IsVisible);
                Assert.False(result.SaveMenuItem.IsVisible);
                Assert.False(result.EditorModeMenuItem.IsVisible);

                service.UpdateTopMenuVisibility(result, editorMode: true);
                Assert.True(result.CloseFileMenuItem.IsVisible);
                Assert.True(result.SaveMenuItem.IsVisible);
                Assert.True(result.EditorModeMenuItem.IsVisible);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaFact(DisplayName = "BuildContextMenuForComponent null component returns empty menu")]
        public void BuildContextMenuForComponent_NullComponent_ReturnsEmptyMenu()
        {
            MenuBuilderService service = CreateService(out Window window);

            try
            {
                ContextMenu menu = service.BuildContextMenuForComponent(
                    null,
                    editorMode: true,
                    _ => { },
                    (_, __) => { },
                    new TabControl(),
                    new TabItem(),
                    new TabItem(),
                    _ => { },
                    (_, __) => { },
                    (_, __) => { },
                    (_, __) => { });

                Assert.Empty(menu.Items);
            }
            finally
            {
                window.Close();
            }
        }

        private static MenuBuilderService CreateService(out Window window)
        {
            window = new Window();
            var config = new MainConfig();
            var modManagementService = new ModManagementService(config);
            return new MenuBuilderService(modManagementService, window);
        }

        private static List<string> GetMenuHeaders(ContextMenu contextMenu) => GetMenuHeaders(contextMenu.Items);

        private static List<string> GetMenuHeaders(Avalonia.Controls.ItemCollection items) =>
            items
                .OfType<MenuItem>()
                .Select(item => item.Header?.ToString() ?? string.Empty)
                .Where(header => !string.IsNullOrEmpty(header))
                .ToList();
    }
}
