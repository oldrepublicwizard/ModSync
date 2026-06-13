// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using ModSync.Core;
using ModSync.Core.Services.Fomod;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class FomodArchiveProbeTests
    {
        [Test]
        public void TryDetectInArchive_FindsNestedModuleConfig()
        {
            string workDir = Path.Combine(Path.GetTempPath(), "modsync-fomod-probe-" + Path.GetRandomFileName());
            Directory.CreateDirectory(workDir);
            string archivePath = Path.Combine(workDir, "TestMod.zip");

            try
            {
                using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry entry = archive.CreateEntry("TestMod-1.0/fomod/ModuleConfig.xml");
                    using (StreamWriter writer = new StreamWriter(entry.Open()))
                    {
                        writer.Write("<config><moduleName>Test</moduleName></config>");
                    }
                }

                Assert.That(
                    FomodArchiveProbe.TryDetectInArchive(archivePath, out string moduleConfigEntryPath),
                    Is.True);
                Assert.That(moduleConfigEntryPath, Does.Contain("fomod/ModuleConfig.xml").IgnoreCase);
            }
            finally
            {
                Directory.Delete(workDir, recursive: true);
            }
        }

        [Test]
        public void TryDetectInArchive_ReturnsFalseForPlainZip()
        {
            string workDir = Path.Combine(Path.GetTempPath(), "modsync-fomod-probe-" + Path.GetRandomFileName());
            Directory.CreateDirectory(workDir);
            string archivePath = Path.Combine(workDir, "Plain.zip");

            try
            {
                using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry entry = archive.CreateEntry("readme.txt");
                    using (StreamWriter writer = new StreamWriter(entry.Open()))
                    {
                        writer.Write("hello");
                    }
                }

                Assert.That(
                    FomodArchiveProbe.TryDetectInArchive(archivePath, out string moduleConfigEntryPath),
                    Is.False);
                Assert.That(moduleConfigEntryPath, Is.Null);
            }
            finally
            {
                Directory.Delete(workDir, recursive: true);
            }
        }
    }

    [TestFixture]
    public sealed class FomodDownloadPromptStateTests
    {
        [Test]
        public void ShouldPrompt_ReturnsFalseAfterDismissedOrConfigured()
        {
            var component = new ModComponent
            {
                ResourceRegistry = new Dictionary<string, ResourceMetadata>
                {
                    ["https://example.test/mod.zip"] = new ResourceMetadata
                    {
                        Files = new Dictionary<string, bool?>
                        {
                            ["ExampleMod.zip"] = true,
                        },
                        HandlerMetadata = new Dictionary<string, object>(),
                    },
                },
            };

            Assert.That(FomodDownloadPromptState.ShouldPrompt(component, "ExampleMod.zip"), Is.True);

            FomodDownloadPromptState.MarkDismissed(component, "ExampleMod.zip");
            Assert.That(FomodDownloadPromptState.ShouldPrompt(component, "ExampleMod.zip"), Is.False);

            component.ResourceRegistry["https://example.test/mod2.zip"] = new ResourceMetadata
            {
                Files = new Dictionary<string, bool?> { ["OtherMod.zip"] = true },
                HandlerMetadata = new Dictionary<string, object>(),
            };
            FomodDownloadPromptState.MarkConfigured(component, "OtherMod.zip");
            Assert.That(FomodDownloadPromptState.ShouldPrompt(component, "OtherMod.zip"), Is.False);
        }
    }
}
