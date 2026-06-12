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
