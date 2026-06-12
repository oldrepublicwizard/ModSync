// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModSync.Core;
using ModSync.Services;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class ArchiveEnumerationServiceTests
    {
        [Fact(DisplayName = "BuildFileTreeFromComponentAsync rejects null component")]
        public async Task BuildFileTreeFromComponentAsync_NullComponent_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                new ArchiveEnumerationService().BuildFileTreeFromComponentAsync(null));
        }

        [Fact(DisplayName = "BuildFileTreeFromComponentAsync returns empty tree for empty registry")]
        public async Task BuildFileTreeFromComponentAsync_EmptyRegistry_ReturnsEmpty()
        {
            ModComponent component = CreateComponent();
            component.ResourceRegistry.Clear();

            var tree = await new ArchiveEnumerationService().BuildFileTreeFromComponentAsync(component);

            Assert.Empty(tree);
        }

        [Fact(DisplayName = "BuildFileTreeFromComponentAsync returns empty tree when source path is unset")]
        public async Task BuildFileTreeFromComponentAsync_NoSourcePath_ReturnsEmpty()
        {
            DirectoryInfo previousSource = MainConfig.SourcePath;
            try
            {
                MainConfig.Instance.sourcePath = null;
                ModComponent component = CreateComponentWithFiles("readme.txt");

                var tree = await new ArchiveEnumerationService().BuildFileTreeFromComponentAsync(component);

                Assert.Empty(tree);
            }
            finally
            {
                MainConfig.Instance.sourcePath = previousSource;
            }
        }

        [Fact(DisplayName = "BuildFileTreeFromComponentAsync builds nodes for plain files and archives")]
        public async Task BuildFileTreeFromComponentAsync_FilesAndArchives_BuildsTree()
        {
            string modDirectory = CreateTempDirectory();
            DirectoryInfo previousSource = MainConfig.SourcePath;

            try
            {
                File.WriteAllText(Path.Combine(modDirectory, "readme.txt"), "hello");
                CreateTestZip(modDirectory, "mod.zip", new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["inner/data.txt"] = "data",
                });

                MainConfig.Instance.sourcePath = new DirectoryInfo(modDirectory);
                ModComponent component = CreateComponentWithFiles("readme.txt", "mod.zip");

                var tree = await new ArchiveEnumerationService().BuildFileTreeFromComponentAsync(component);

                Assert.Equal(2, tree.Count);
                Assert.Contains(tree, node => node.Name == "readme.txt" && !node.IsArchive);
                var archiveNode = tree.First(node => node.IsArchive);
                Assert.Equal("mod.zip", archiveNode.Name);
                Assert.NotEmpty(archiveNode.Children);
            }
            finally
            {
                MainConfig.Instance.sourcePath = previousSource;
                TryDeleteDirectory(modDirectory);
            }
        }

        private static ModComponent CreateComponent()
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Archive Host",
                ResourceRegistry = new Dictionary<string, ResourceMetadata>(StringComparer.Ordinal),
            };
        }

        private static ModComponent CreateComponentWithFiles(params string[] fileNames)
        {
            var files = new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);
            foreach (string fileName in fileNames)
            {
                files[fileName] = null;
            }

            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Archive Host",
                ResourceRegistry = new Dictionary<string, ResourceMetadata>(StringComparer.Ordinal)
                {
                    ["http://example.com/mod"] = new ResourceMetadata
                    {
                        Files = files,
                    },
                },
            };
        }

        private static string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), "ModSync_ArchiveEnumTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void CreateTestZip(string directory, string fileName, Dictionary<string, string> files)
        {
            string zipPath = Path.Combine(directory, fileName);
            using (var archive = ZipArchive.CreateArchive())
            {
                foreach (KeyValuePair<string, string> entry in files)
                {
                    archive.AddEntry(
                        entry.Key,
                        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(entry.Value)),
                        closeStream: true);
                }

                using FileStream stream = File.OpenWrite(zipPath);
                archive.SaveTo(stream, new SharpCompress.Writers.Zip.ZipWriterOptions(CompressionType.None));
            }
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
    }
}
