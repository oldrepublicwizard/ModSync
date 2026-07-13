// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ModSync.Core;
using ModSync.Core.CLI;
using ModSync.Core.Parsing;
using ModSync.Core.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    /// <summary>
    /// Tests for the guide-ingestion slice: natural-language draft instructions
    /// (<see cref="DraftInstructionService"/>), paste-cascade format sniffing
    /// (<see cref="ModComponentSerializationService.DetectFormatFromContent"/>), and CLI parity
    /// (convert --stdin --parse-directions).
    /// </summary>
    [TestFixture]
    public sealed class GuideIngestionTests
    {
        private const string ModDirectoryPlaceholder = "<<modDirectory>>";
        private const string KotorDirectoryPlaceholder = "<<kotorDirectory>>";

        // Real prose taken from mod-builds/content/k1/full.md (KOTOR 1 Community Patch section style).
        private const string MoveFoldersProse =
            "Move everything from the Straight Fixes, Resolution Fixes, and Aesthetic Improvements folders to your Override.";

        private const string PatcherProse =
            "Run the installer, then move the files from the patch to your override.";

        private const string MarkdownGuide = @"### Guide Ingestion Test Mod

**Name:** [Guide Ingestion Test Mod](https://example.com/guide-ingestion-test-mod.zip)

**Author:** Test Author

**Description:** Synthetic mod for guide ingestion tests.

**Category & Tier:** Immersion / 1 - Essential

**Installation Method:** Loose-File Mod

**Installation Instructions:** Move everything from the Straight Fixes, Resolution Fixes, and Aesthetic Improvements folders to your Override.

___
";

        private string _testDirectory;
        private MainConfig _previousMainConfig;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "ModSync_GuideIngestion_" + Guid.NewGuid());
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

        private static ModComponent CreateComponent(string directions)
        {
            return new ModComponent
            {
                Name = "Guide Ingestion Component",
                Guid = Guid.NewGuid(),
                Directions = directions,
            };
        }

        private static void AssertInstructionIsSandboxed(Instruction instruction)
        {
            foreach (string source in instruction.Source)
            {
                Assert.That(DraftInstructionService.IsSandboxedPath(source), Is.True,
                    $"Source '{source}' must be confined to a placeholder root without '..' / rooted escapes");
                Assert.That(source, Does.Not.Contain("<<gameDirectory>>"), "Legacy placeholder must be normalized away");
                Assert.That(source.Replace('\\', '/').Split('/'), Does.Not.Contain(".."),
                    $"Source '{source}' must not contain '..' segments");
            }

            if (!string.IsNullOrEmpty(instruction.Destination))
            {
                Assert.That(DraftInstructionService.IsSandboxedPath(instruction.Destination), Is.True,
                    $"Destination '{instruction.Destination}' must be confined to a placeholder root without '..' / rooted escapes");
                Assert.That(instruction.Destination, Does.Not.Contain("<<gameDirectory>>"), "Legacy placeholder must be normalized away");
                Assert.That(instruction.Destination.Replace('\\', '/').Split('/'), Does.Not.Contain(".."),
                    $"Destination '{instruction.Destination}' must not contain '..' segments");
            }
        }

        #region NL parser / draft service

        [Test]
        public void DraftInstructions_MoveFoldersProse_ProducesSandboxedMoveInstructions()
        {
            ModComponent component = CreateComponent(MoveFoldersProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.EqualTo(1), "Component with parseable prose should receive drafts");
                Assert.That(component.Instructions, Is.Not.Empty, "Draft instructions should be attached to the component");
                Assert.That(results[0].DraftInstructionCount, Is.EqualTo(component.Instructions.Count));
            });

            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Move), Is.True,
                "Prose describing a folder move should draft at least one Move instruction");

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
                Assert.That(instruction.GetParentComponent(), Is.SameAs(component));
            }
        }

        [Test]
        public void DraftInstructions_PatcherProse_ProducesSandboxedDrafts()
        {
            ModComponent component = CreateComponent(PatcherProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions, Is.Not.Empty);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_ComponentWithExistingInstructions_IsNeverTouched()
        {
            ModComponent component = CreateComponent(MoveFoldersProse);
            var authored = new Instruction { Action = Instruction.ActionType.Patcher };
            authored.SetParentComponent(component);
            component.Instructions.Add(authored);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.Empty, "Authored instructions must never be overwritten or extended by drafts");
                Assert.That(component.Instructions, Has.Count.EqualTo(1));
                Assert.That(component.Instructions[0], Is.SameAs(authored));
            });
        }

        [Test]
        public void DraftInstructions_UnparseableProse_DegradesGracefullyToNoDrafts()
        {
            ModComponent component = CreateComponent("A fan favorite retexture bundle. Many enjoy this excellent work.");

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(results, Is.Empty, "Unparseable prose should degrade to today's behavior");
                Assert.That(component.Instructions, Is.Empty);
            });
        }

        [Test]
        public void DraftInstructions_EmptyDirections_ProducesNoDrafts()
        {
            ModComponent component = CreateComponent(string.Empty);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Is.Empty);
            Assert.That(component.Instructions, Is.Empty);
        }

        [Test]
        public void TrySanitizeInstruction_NormalizesLegacyGameDirectoryPlaceholder()
        {
            var instruction = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { @"<<modDirectory>>\textures\*" },
                Destination = @"<<gameDirectory>>\Override",
            };

            bool kept = DraftInstructionService.TrySanitizeInstruction(instruction);

            Assert.Multiple(() =>
            {
                Assert.That(kept, Is.True);
                Assert.That(instruction.Destination, Is.EqualTo(@"<<kotorDirectory>>\Override"));
                Assert.That(instruction.Source[0], Is.EqualTo(@"<<modDirectory>>\textures\*"));
            });
        }

        [Test]
        public void TrySanitizeInstruction_RejectsNonSandboxedPaths()
        {
            var absoluteSource = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { @"C:\Windows\System32\evil.dll" },
                Destination = @"<<kotorDirectory>>\Override",
            };

            var absoluteDestination = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { @"<<modDirectory>>\file.tga" },
                Destination = @"C:\Windows\System32",
            };

            Assert.Multiple(() =>
            {
                Assert.That(DraftInstructionService.TrySanitizeInstruction(absoluteSource), Is.False,
                    "A Move whose only source escapes the sandbox must be dropped");
                Assert.That(DraftInstructionService.TrySanitizeInstruction(absoluteDestination), Is.False,
                    "A Move whose destination escapes the sandbox must be dropped");
            });
        }

        [Test]
        public void TrySanitizeInstruction_RejectsParentDirectoryTraversalAfterPlaceholder()
        {
            var traversalSource = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { "<<modDirectory>>/../outside" },
                Destination = "<<kotorDirectory>>/Override",
            };

            var traversalDestination = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { @"<<modDirectory>>\textures\*" },
                Destination = @"<<kotorDirectory>>\..\Windows",
            };

            var gluedTraversal = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { "<<modDirectory>>../outside" },
                Destination = "<<kotorDirectory>>/Override",
            };

            var driveAfterPlaceholder = new Instruction
            {
                Action = Instruction.ActionType.Move,
                Source = new List<string> { @"<<modDirectory>>\C:\Windows\evil.dll" },
                Destination = @"<<kotorDirectory>>\Override",
            };

            Assert.Multiple(() =>
            {
                Assert.That(DraftInstructionService.IsSandboxedPath("<<modDirectory>>/../outside"), Is.False);
                Assert.That(DraftInstructionService.IsSandboxedPath(@"<<kotorDirectory>>\..\Windows"), Is.False);
                Assert.That(DraftInstructionService.IsSandboxedPath("<<modDirectory>>../outside"), Is.False);
                Assert.That(DraftInstructionService.IsSandboxedPath(@"<<modDirectory>>\C:\Windows\evil.dll"), Is.False);
                Assert.That(DraftInstructionService.IsSandboxedPath("<<modDirectory>>/textures/*"), Is.True);
                Assert.That(DraftInstructionService.TrySanitizeInstruction(traversalSource), Is.False,
                    "Source with '..' after placeholder must be dropped");
                Assert.That(DraftInstructionService.TrySanitizeInstruction(traversalDestination), Is.False,
                    "Destination with '..' after placeholder must be dropped");
                Assert.That(DraftInstructionService.TrySanitizeInstruction(gluedTraversal), Is.False,
                    "Missing separator before '..' must be dropped");
                Assert.That(DraftInstructionService.TrySanitizeInstruction(driveAfterPlaceholder), Is.False,
                    "Drive letter segment after placeholder must be dropped");
            });
        }

        [Test]
        public void DraftInstructions_TraversalProse_DoesNotKeepEscapingPaths()
        {
            ModComponent escapeComponent = CreateComponent(
                "Move everything from <<modDirectory>>/../outside to your Override.");
            ModComponent bareDotDot = CreateComponent(
                "Move .. to override.");

            IReadOnlyList<DraftInstructionResult> escapeResults =
                DraftInstructionService.GenerateDraftInstructions(new[] { escapeComponent });
            IReadOnlyList<DraftInstructionResult> bareResults =
                DraftInstructionService.GenerateDraftInstructions(new[] { bareDotDot });

            Assert.Multiple(() =>
            {
                foreach (Instruction instruction in escapeComponent.Instructions)
                {
                    AssertInstructionIsSandboxed(instruction);
                }

                foreach (Instruction instruction in bareDotDot.Instructions)
                {
                    AssertInstructionIsSandboxed(instruction);
                }

                if (escapeResults.Count > 0)
                {
                    Assert.That(escapeComponent.InstallationWarning, Does.Contain(DraftInstructionService.ReviewFlagMessage));
                }

                if (bareResults.Count > 0)
                {
                    Assert.That(bareDotDot.InstallationWarning, Does.Contain(DraftInstructionService.ReviewFlagMessage));
                }
            });
        }

        [Test]
        public void DraftInstructions_SuccessfulDraft_AppliesReviewFlagMessage()
        {
            ModComponent component = CreateComponent(MoveFoldersProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.InstallationWarning, Is.EqualTo(DraftInstructionService.ReviewFlagMessage));
        }

        #endregion

        #region Paste cascade format sniffing

        [Test]
        public void DetectFormatFromContent_TomlContent_ReturnsToml()
        {
            const string toml = @"[[thisMod]]
Guid = ""a1b2c3d4-e5f6-7890-abcd-ef1234567890""
Name = ""Paste Cascade Toml Mod""
";

            Assert.That(ModComponentSerializationService.DetectFormatFromContent(toml), Is.EqualTo("toml"));
        }

        [Test]
        public void DetectFormatFromContent_MarkdownGuide_ReturnsMarkdown()
        {
            Assert.That(ModComponentSerializationService.DetectFormatFromContent(MarkdownGuide), Is.EqualTo("markdown"));
        }

        [Test]
        public void DetectFormatFromContent_UnrecognizedProse_ReturnsNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(ModComponentSerializationService.DetectFormatFromContent("just some random prose about nothing in particular"), Is.Null);
                Assert.That(ModComponentSerializationService.DetectFormatFromContent("   \r\n\t  "), Is.Null);
            });
        }

        [Test]
        public void PasteCascade_MarkdownGuideString_DeserializesComponentWithDirections()
        {
            IReadOnlyList<ModComponent> components = ModComponentSerializationService.DeserializeModComponentFromString(MarkdownGuide);

            Assert.That(components, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(components[0].Name, Is.EqualTo("Guide Ingestion Test Mod"));
                Assert.That(components[0].Directions, Does.Contain("Move everything from the Straight Fixes"));
            });
        }

        #endregion

        #region Real guide (mod-builds)

        [Test]
        public void RealGuide_K1FullMarkdown_DraftedInstructionsAreAllSandboxed()
        {
            string repoRoot = ResolveRepoRoot();
            string markdownPath = Path.Combine(repoRoot, "mod-builds", "content", "k1", "full.md");
            if (!File.Exists(markdownPath))
            {
                Assert.Ignore($"mod-builds guide not found: {markdownPath}");
            }

            List<ModComponent> components = FileLoadingService.LoadFromFile(markdownPath).ToList();
            Assert.That(components, Is.Not.Empty);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(components);

            Assert.That(results, Is.Not.Empty, "Real guide prose should draft instructions for at least one component");

            foreach (DraftInstructionResult result in results)
            {
                Assert.That(result.Component.Instructions, Is.Not.Empty);
                foreach (Instruction instruction in result.Component.Instructions)
                {
                    AssertInstructionIsSandboxed(instruction);
                }
            }
        }

        #endregion

        #region CLI parity (convert --stdin --parse-directions)

        [Test]
        public void CliConvert_StdinWithParseDirections_EmitsReviewFlaggedTomlWithDraftInstructions()
        {
            string outputToml = Path.Combine(_testDirectory, "ingested.toml");

            TextReader previousIn = Console.In;
            try
            {
                Console.SetIn(new StringReader(MarkdownGuide));

                int exitCode = ModBuildConverter.Run(new[]
                {
                    "convert",
                    "--stdin",
                    "--parse-directions",
                    "-f", "toml",
                    "-o", outputToml,
                    "--plaintext",
                });

                Assert.That(exitCode, Is.EqualTo(0), "convert --stdin --parse-directions should succeed");
            }
            finally
            {
                Console.SetIn(previousIn);
            }

            Assert.That(File.Exists(outputToml), Is.True);
            string tomlOutput = File.ReadAllText(outputToml);

            Assert.Multiple(() =>
            {
                Assert.That(tomlOutput, Does.Contain("# VALIDATION ISSUES:"), "Drafted components must be flagged for review in the output");
                Assert.That(tomlOutput, Does.Contain(DraftInstructionService.ReviewFlagMessage));
            });

            var reloaded = ModComponentSerializationService
                .DeserializeModComponentFromString(tomlOutput, "toml")
                .ToList();

            Assert.That(reloaded, Has.Count.EqualTo(1));
            Assert.That(reloaded[0].Instructions, Is.Not.Empty, "Drafted instructions should survive TOML round-trip");

            foreach (Instruction instruction in reloaded[0].Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void CliConvert_StdinCombinedWithInput_Fails()
        {
            TextReader previousIn = Console.In;
            try
            {
                Console.SetIn(new StringReader(MarkdownGuide));

                int exitCode = ModBuildConverter.Run(new[]
                {
                    "convert",
                    "--stdin",
                    "--input", Path.Combine(_testDirectory, "does-not-matter.toml"),
                    "--plaintext",
                });

                Assert.That(exitCode, Is.EqualTo(1), "--stdin combined with --input should be rejected");
            }
            finally
            {
                Console.SetIn(previousIn);
            }
        }

        #endregion

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
    }
}
