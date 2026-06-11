// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class DialogServiceTests
    {
        [Fact(DisplayName = "Constructor rejects null parent window")]
        public void Constructor_NullParentWindow_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DialogService(null));
        }

        [AvaloniaFact(DisplayName = "GetStorageFolderFromPathAsync returns null for empty path")]
        public async Task GetStorageFolderFromPathAsync_EmptyPath_ReturnsNull()
        {
            var service = new DialogService(new Window());

            Assert.Null(await service.GetStorageFolderFromPathAsync(string.Empty));
            Assert.Null(await service.GetStorageFolderFromPathAsync(null));
        }
    }
}
