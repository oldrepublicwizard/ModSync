// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ModSync.Core;
using ModSync.Core.CLI;
using ModSync.Core.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    /// <summary>
    /// Network download smoke for neocities K2 Full merged with golden mod-builds TOML.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    [Category("LongRunning")]
    public sealed class K2FullGuideDownloadTests
    {
        private static readonly string GoldenTomlRelative = Path.Combine("mod-builds", "TOMLs", "KOTOR2_Full.toml");
        private const string SilentSionModName = "Silent Sion Restoration";
        private const string SilentSionArchive = "Silent Sion Restoration.zip";

        private string _testDirectory;
        private string _modDirectory;
        private string _kotorDirectory;
        private MainConfig _previousMainConfig;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ModSync_K2Download_" + Guid.NewGuid());
            _modDirectory = Path.Combine(_testDirectory, "Mods");
            _kotorDirectory = Path.Combine(_testDirectory, "KOTOR");
            Directory.CreateDirectory(_modDirectory);
            Directory.CreateDirectory(_kotorDirectory);
            Directory.CreateDirectory(Path.Combine(_kotorDirectory, "Override"));
            File.WriteAllText(Path.Combine(_kotorDirectory, "swkotor.exe"), "fake exe");
            File.WriteAllText(Path.Combine(_kotorDirectory, "dialog.tlk"), "fake dialog");

            _previousMainConfig = MainConfig.Instance;
            MainConfig.Instance = new MainConfig
            {
                sourcePath = new DirectoryInfo(_modDirectory),
                destinationPath = new DirectoryInfo(_kotorDirectory),
            };
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
        public void K2FullGuideFixture_MergedSilentSion_DownloadsViaInstallSelect_LongRunning()
        {
            (string fixturePath, string goldenTomlPath) = ResolveInputs();
            string mergedToml = Path.Combine(_testDirectory, "k2_merged.toml");

            int mergeExit = ModBuildConverter.Run(new[]
            {
                "merge",
                "--existing", goldenTomlPath,
                "--incoming", fixturePath,
                "--use-existing-order",
                "--prefer-existing-instructions",
                "--prefer-existing-options",
                "--prefer-existing-modlinks",
                "-f", "toml",
                "-o", mergedToml,
                "--plaintext",
            });

            Assert.That(mergeExit, Is.EqualTo(0), "merge neocities fixture + golden TOML should succeed");
            StripDependenciesForDownloadSmoke(mergedToml, SilentSionModName);

            int installExit = ModBuildConverter.Run(new[]
            {
                "install",
                "--input", mergedToml,
                "--game-dir", _kotorDirectory,
                "--source-dir", _modDirectory,
                "--select", "mod:" + SilentSionModName,
                "--download",
                "--skip-validation",
                "--best-effort",
                "-y",
            });

            string archivePath = Path.Combine(_modDirectory, SilentSionArchive);
            bool archiveExists = File.Exists(archivePath);
            bool anyZipInModDir = Directory.GetFiles(_modDirectory, "*.zip", SearchOption.TopDirectoryOnly).Length > 0;

            Assert.Multiple(() =>
            {
                Assert.That(installExit, Is.EqualTo(0),
                    "install --download --select mod:Silent Sion should complete in best-effort mode");
                Assert.That(archiveExists || anyZipInModDir, Is.True,
                    $"Expected {SilentSionArchive} (or another downloaded archive) under the mod workspace");
            });
        }

        private static (string fixturePath, string goldenTomlPath) ResolveInputs()
        {
            string repoRoot = ResolveRepoRoot();
            string fixturePath = Path.Combine(repoRoot, "src", "ModSync.Tests", "Fixtures", "k2_full_guide.md");
            string goldenTomlPath = Path.Combine(repoRoot, GoldenTomlRelative);

            if (!File.Exists(fixturePath))
            {
                Assert.Fail($"Expected neocities fixture at {fixturePath}");
            }

            if (!File.Exists(goldenTomlPath))
            {
                Assert.Ignore($"Golden TOML not found at {goldenTomlPath} (clone mod-builds at repo root for this test).");
            }

            return (fixturePath, goldenTomlPath);
        }

        private static string ResolveRepoRoot()
        {
            string[] candidates =
            {
                Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..")),
                Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..")),
                Path.GetFullPath(Environment.CurrentDirectory),
            };

            foreach (string candidate in candidates.Distinct(StringComparer.Ordinal))
            {
                if (File.Exists(Path.Combine(candidate, "ModSync.sln")))
                {
                    return candidate;
                }
            }

            throw new DirectoryNotFoundException("Could not locate repository root containing ModSync.sln");
        }

        private static void StripDependenciesForDownloadSmoke(string tomlPath, params string[] modNameFragments)
        {
            List<ModComponent> components = FileLoadingService.LoadFromFile(tomlPath).ToList();
            bool changed = false;

            foreach (ModComponent component in components)
            {
                if (!modNameFragments.Any(fragment =>
                        component.Name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    continue;
                }

                if (component.Dependencies.Count > 0)
                {
                    component.Dependencies.Clear();
                    changed = true;
                }

                if (component.Restrictions.Count > 0)
                {
                    component.Restrictions.Clear();
                    changed = true;
                }
            }

            if (changed)
            {
                FileLoadingService.SaveToFile(components, tomlPath);
            }
        }
    }
}
