// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class GuiPathServiceTests
    {
        [Fact(DisplayName = "AddToRecentDirectories inserts path at front")]
        public void AddToRecentDirectories_InsertsAtFront()
        {
            var existing = new List<string> { "a", "b" };
            GuiPathService.AddToRecentDirectories("c", existing);

            Assert.Equal(new[] { "c", "a", "b" }, existing);
        }

        [Fact(DisplayName = "AddToRecentDirectories moves duplicate to front")]
        public void AddToRecentDirectories_MovesDuplicateToFront()
        {
            var existing = new List<string> { "a", "b", "c" };
            GuiPathService.AddToRecentDirectories("b", existing);

            Assert.Equal(new[] { "b", "a", "c" }, existing);
        }

        [Fact(DisplayName = "AddToRecentDirectories trims list to maxCount")]
        public void AddToRecentDirectories_TrimsToMaxCount()
        {
            var existing = new List<string> { "1", "2", "3" };
            GuiPathService.AddToRecentDirectories("4", existing, maxCount: 3);

            Assert.Equal(3, existing.Count);
            Assert.Equal(new[] { "4", "1", "2" }, existing);
        }

        [Fact(DisplayName = "LoadRecentModDirectoriesAsync returns empty when file missing")]
        public async Task LoadRecentModDirectoriesAsync_MissingFile_ReturnsEmpty()
        {
            string filePath = Path.Combine(Path.GetTempPath(), $"mods_recent_{Guid.NewGuid():N}.txt");

            List<string> loaded = await GuiPathService.LoadRecentModDirectoriesAsync(filePath);

            Assert.Empty(loaded);
        }

        [Fact(DisplayName = "Save and load recent mod directories round-trip")]
        public async Task SaveAndLoadRecentModDirectories_RoundTrip()
        {
            string dir1 = CreateTempDirectory();
            string dir2 = CreateTempDirectory();
            string filePath = Path.Combine(Path.GetTempPath(), $"mods_recent_{Guid.NewGuid():N}.txt");

            try
            {
                var directories = new List<string> { dir1, dir2 };
                await GuiPathService.SaveRecentModDirectoriesAsync(directories, filePath);

                List<string> loaded = await GuiPathService.LoadRecentModDirectoriesAsync(filePath);

                Assert.Equal(directories, loaded);
            }
            finally
            {
                TryDeleteFile(filePath);
                TryDeleteDirectory(dir1);
                TryDeleteDirectory(dir2);
            }
        }

        [Fact(DisplayName = "TryApplySourcePath sets config when directory exists")]
        public void TryApplySourcePath_ValidDirectory_UpdatesConfig()
        {
            string modPath = CreateTempDirectory();

            try
            {
                var config = new MainConfig();
                var service = new GuiPathService(config);

                bool applied = service.TryApplySourcePath(modPath);

                Assert.True(applied);
                Assert.Equal(modPath, config.sourcePath.FullName);
            }
            finally
            {
                TryDeleteDirectory(modPath);
            }
        }

        [Fact(DisplayName = "TryApplySourcePath returns false for missing directory")]
        public void TryApplySourcePath_MissingDirectory_ReturnsFalse()
        {
            var config = new MainConfig();
            var service = new GuiPathService(config);

            bool applied = service.TryApplySourcePath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

            Assert.False(applied);
        }

        [Fact(DisplayName = "TryApplyDestinationPath sets config when directory exists")]
        public void TryApplyDestinationPath_ValidDirectory_UpdatesConfig()
        {
            string gamePath = CreateTempDirectory();

            try
            {
                var config = new MainConfig();
                var service = new GuiPathService(config);

                bool applied = service.TryApplyDestinationPath(gamePath);

                Assert.True(applied);
                Assert.Equal(gamePath, config.destinationPath.FullName);
            }
            finally
            {
                TryDeleteDirectory(gamePath);
            }
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "ModSync_GuiPathTests_" + Guid.NewGuid().ToString("N"));
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

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
