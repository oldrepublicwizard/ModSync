// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using ModSync.Core;
using ModSync.Core.CLI;
using ModSync.Core.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    [Category("Integration")]
    public sealed class FomodCliPostDownloadIntegrationTests
    {
        private string _testDirectory;
        private MainConfig _previousMainConfig;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ModSync_FomodCli_" + Guid.NewGuid());
            Directory.CreateDirectory(_testDirectory);
            _previousMainConfig = MainConfig.Instance;
            MainConfig.Instance = new MainConfig();
        }

        [TearDown]
        public void TearDown()
        {
            MainConfig.Instance = _previousMainConfig;

            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Test]
        public void Merge_WithDownloadAndFomodSkip_CompletesSuccessfully()
        {
            const string archiveName = "cli-fomod-mod.zip";
            string sourceDir = Path.Combine(_testDirectory, "mods");
            Directory.CreateDirectory(sourceDir);
            CreateFomodArchive(Path.Combine(sourceDir, archiveName));

            string existingToml = Path.Combine(_testDirectory, "existing.toml");
            string incomingMd = Path.Combine(_testDirectory, "incoming.md");
            string outputToml = Path.Combine(_testDirectory, "merged.toml");

            File.WriteAllText(existingToml, @"[[thisMod]]
Guid = ""a1b2c3d4-e5f6-7890-abcd-ef1234567890""
Name = ""CLI FOMOD Mod""
IsSelected = true
ModLinkFilenames = { ""https://example.com/cli-fomod-mod.zip"" = {  } }
");

            File.WriteAllText(incomingMd, @"### CLI FOMOD Mod

**Name:** [CLI FOMOD Mod](https://example.com/cli-fomod-mod.zip)

**Author:** Test Author

**Description:** Synthetic FOMOD mod for CLI post-download integration.

**Category & Tier:** Immersion / 1 - Essential

**Installation Method:** FOMOD

___
");

            string[] args =
            {
                "merge",
                "--existing", existingToml,
                "--incoming", incomingMd,
                "--use-existing-order",
                "--prefer-existing-instructions",
                "-f", "toml",
                "-o", outputToml,
                "--source-path", sourceDir,
                "--auto-generate-local",
                "-d",
                "--fomod-skip",
            };

            int exitCode = ModBuildConverter.Run(args);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0), "merge -d --fomod-skip should succeed with local FOMOD archive");
                Assert.That(File.Exists(outputToml), Is.True);
            });

            var merged = ModComponentSerializationService
                .DeserializeModComponentFromString(File.ReadAllText(outputToml), "toml")
                .ToList();

            Assert.That(merged, Has.Count.EqualTo(1));
        }

        private static void CreateFomodArchive(string archivePath)
        {
            using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry("CLI-FOMOD-1.0/fomod/ModuleConfig.xml");
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write("<config><moduleName>CLI FOMOD Mod</moduleName></config>");
                }
            }
        }
    }
}
