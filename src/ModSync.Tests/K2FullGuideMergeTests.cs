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
    /// Neocities K2 Full fixture merged with golden <c>mod-builds/KOTOR2_Full.toml</c> should
    /// inherit download URLs and authored instructions for round-trip download workflows.
    /// </summary>
    [TestFixture]
    public sealed class K2FullGuideMergeTests
    {
        private static readonly string GoldenTomlRelative = Path.Combine("mod-builds", "TOMLs", "KOTOR2_Full.toml");

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

        private static List<ModComponent> MergeNeocitiesWithGolden(string fixturePath, string goldenTomlPath)
        {
            var mergeOptions = new MergeOptions
            {
                UseExistingOrder = true,
                PreferExistingInstructions = true,
                PreferExistingOptions = true,
                PreferExistingResourceRegistry = true,
            };

            return ComponentMergeService.MergeInstructionSets(
                goldenTomlPath,
                fixturePath,
                mergeOptions);
        }

        [Test]
        public void K2FullGuideFixture_MergeWithGoldenToml_InheritsDownloadUrls()
        {
            (string fixturePath, string goldenTomlPath) = ResolveInputs();

            List<ModComponent> merged = MergeNeocitiesWithGolden(fixturePath, goldenTomlPath);

            Assert.That(merged.Count, Is.GreaterThanOrEqualTo(100),
                "Merged set should retain the golden build component count");

            ModComponent silentSion = merged.FirstOrDefault(c =>
                c.Name.IndexOf("Silent Sion Restoration", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(silentSion, Is.Not.Null, "Silent Sion should match between neocities fixture and golden TOML");

            bool hasDownloadLink = silentSion.ResourceRegistry?.Count > 0;
            Assert.That(hasDownloadLink, Is.True,
                "Merged Silent Sion should keep golden download URLs in ResourceRegistry");

            int withUrls = merged.Count(c => c.ResourceRegistry?.Count > 0);
            Assert.That(withUrls, Is.GreaterThanOrEqualTo(100),
                "Most golden K2 components should retain download URLs after neocities merge");
        }

        [Test]
        public void K2FullGuideFixture_CliMergeWithGolden_ProducesMergedToml()
        {
            (string fixturePath, string goldenTomlPath) = ResolveInputs();

            string outputToml = Path.Combine(Path.GetTempPath(), "ModSync_K2Merge_" + Guid.NewGuid() + ".toml");
            try
            {
                int exit = ModBuildConverter.Run(new[]
                {
                    "merge",
                    "--existing", goldenTomlPath,
                    "--incoming", fixturePath,
                    "--use-existing-order",
                    "--prefer-existing-instructions",
                    "--prefer-existing-options",
                    "--prefer-existing-modlinks",
                    "-f", "toml",
                    "-o", outputToml,
                    "--plaintext",
                });

                Assert.That(exit, Is.EqualTo(0), "CLI merge neocities fixture + golden TOML should succeed");
                Assert.That(File.Exists(outputToml), Is.True);

                List<ModComponent> merged = FileLoadingService.LoadFromFile(outputToml).ToList();
                ModComponent silentSion = merged.First(c =>
                    c.Name.IndexOf("Silent Sion Restoration", StringComparison.OrdinalIgnoreCase) >= 0);

                Assert.That(silentSion.ResourceRegistry?.Count, Is.GreaterThan(0),
                    "CLI-merged Silent Sion should retain golden download metadata");
            }
            finally
            {
                if (File.Exists(outputToml))
                {
                    File.Delete(outputToml);
                }
            }
        }
    }
}
