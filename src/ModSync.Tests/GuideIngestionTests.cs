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
using ModSync.Core.Ports.Guides;
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

        private const string DeleteBeforeMoveProse =
            "Make sure to delete LSI_win01.tpc and LSI_box01.tpc **before** moving to override.";

        private const string BeforeMovingDeleteProse =
            "Before moving the files to the override folder, be sure to delete the following: PFBI01 through PFBI04, and PMBI01 through PMBI04.";

        private const string RerunPatcherProse =
            "Install the main mod, then re-run the patcher and select the K1CP compatibility install option and install it as well, if using K1CP.";

        private const string MoveExceptProse =
            "The file has the wrong readme; move all the files in the Creatures folder, except for the readme and Gizka.jpg (any .jpg/.png files are always previews and can be deleted), to the override.";

        // K2 Full / neocities phrases that previously yielded 0 drafts.
        private const string K2OverrideFolderProse =
            "Install the files within the Override folder.";

        private const string K2IncludedOverrideProse =
            "Install the files from the included Override directory only.";

        private const string K2HoloPatcherSelectProse =
            "Run the HoloPatcher executable. Select the default install, not M4-78.";

        private const string K2InstallQuotedOptionProse =
            "If you would like to have Visas's class as Sith Assassin, install the \"Standard + Sith Assassin Visas\" option. Otherwise, simply install \"Standard.\"";

        private const string K2MoviesFolderProse =
            "Bear in mind that the files from this mod go in your movies folder, not override.";

        private const string K2TpcVariantMoveProse =
            "Download the .tpc variant of the mod. For this mod only, do not overwrite if prompted!";

        private const string K2GoIntoFolderMoveProse =
            "Ignore the \"Player Bodies\" folder. Go into the NPC Replacement folder and move all the loose files to the override directory. Ignore the optional folder.";

        private const string K2CommunityPatchFoldersProse =
            "If you are using the K2 Community Patch, install the contents of every folder but Straight Fixes (that was already in the K2CP).";

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
        public void DraftInstructions_DeleteBeforeMoveProse_ProducesSandboxedDelete()
        {
            ModComponent component = CreateComponent(DeleteBeforeMoveProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1), "Delete-before-move prose from mod-builds should draft");
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Delete), Is.True,
                "Prose that deletes files before moving should draft a Delete instruction");

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_BeforeMovingDeleteProse_ProducesSandboxedDelete()
        {
            ModComponent component = CreateComponent(BeforeMovingDeleteProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Delete), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_RerunPatcherProse_ProducesSandboxedPatcher()
        {
            ModComponent component = CreateComponent(RerunPatcherProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Patcher), Is.True,
                "Re-run patcher / compatibility option prose should draft a Patcher instruction");

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_MoveExceptProse_ProducesSandboxedMove()
        {
            ModComponent component = CreateComponent(MoveExceptProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Move), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2OverrideFolderProse_ProducesSandboxedMove()
        {
            ModComponent component = CreateComponent(K2OverrideFolderProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i =>
                i.Action == Instruction.ActionType.Move
                && i.Source.Any(s => s.IndexOf("Override", StringComparison.OrdinalIgnoreCase) >= 0)
                && i.Destination.IndexOf("Override", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2IncludedOverrideProse_ProducesSandboxedMove()
        {
            ModComponent component = CreateComponent(K2IncludedOverrideProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Move), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2HoloPatcherSelectProse_ProducesSandboxedPatcher()
        {
            ModComponent component = CreateComponent(K2HoloPatcherSelectProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i =>
                i.Action == Instruction.ActionType.Patcher
                && i.Source.Count > 0
                && i.Source[0].StartsWith(ModDirectoryPlaceholder, StringComparison.Ordinal)), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2InstallQuotedOptionProse_ProducesSandboxedPatcher()
        {
            ModComponent component = CreateComponent(K2InstallQuotedOptionProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Patcher), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2MoviesFolderProse_ProducesMoviesDestination()
        {
            ModComponent component = CreateComponent(K2MoviesFolderProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i =>
                i.Action == Instruction.ActionType.Move
                && i.Destination.IndexOf("Movies", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2TpcVariantMoveProse_ProducesSandboxedMoveWithoutOverwrite()
        {
            ModComponent component = CreateComponent(K2TpcVariantMoveProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i =>
                i.Action == Instruction.ActionType.Move
                && i.Overwrite == false), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2GoIntoFolderMoveProse_ProducesSandboxedMove()
        {
            ModComponent component = CreateComponent(K2GoIntoFolderMoveProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i =>
                i.Action == Instruction.ActionType.Move
                && i.Source.Any(s => s.IndexOf("NPC Replacement", StringComparison.OrdinalIgnoreCase) >= 0)), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_K2CommunityPatchFoldersProse_ProducesSandboxedMove()
        {
            ModComponent component = CreateComponent(K2CommunityPatchFoldersProse);

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions.Any(i => i.Action == Instruction.ActionType.Move), Is.True);

            foreach (Instruction instruction in component.Instructions)
            {
                AssertInstructionIsSandboxed(instruction);
            }
        }

        [Test]
        public void DraftInstructions_PatcherInstallationMethodFallback_WhenPreferenceOnlyProse()
        {
            var component = new ModComponent
            {
                Name = "Preference-Only Patcher Mod",
                Guid = Guid.NewGuid(),
                InstallationMethod = "HoloPatcher Mod",
                Directions = "Recommend Drew's fix, as it preserves more of the original dialogue.",
            };

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions, Has.Count.EqualTo(1));
            Assert.That(component.Instructions[0].Action, Is.EqualTo(Instruction.ActionType.Patcher));
            AssertInstructionIsSandboxed(component.Instructions[0]);
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
        public void DraftInstructions_EmptyDirectionsLooseFileMod_UsesInstallationMethodFallback()
        {
            ModComponent component = CreateComponent(string.Empty);
            component.InstallationMethod = "Loose-File Mod";

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions, Has.Count.EqualTo(1));
            Assert.That(component.Instructions[0].Action, Is.EqualTo(Instruction.ActionType.Move));
            AssertInstructionIsSandboxed(component.Instructions[0]);
        }

        [Test]
        public void DraftInstructions_EmptyDirectionsTslpatcherMod_UsesPatcherFallback()
        {
            ModComponent component = CreateComponent(string.Empty);
            component.InstallationMethod = "TSLPatcher Mod";

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions[0].Action, Is.EqualTo(Instruction.ActionType.Patcher));
            AssertInstructionIsSandboxed(component.Instructions[0]);
        }

        [Test]
        public void DraftInstructions_RecommendationOnlyLooseFileProse_UsesLooseFileFallback()
        {
            ModComponent component = CreateComponent(
                "Recommend the version without overlays, but it's personal preference.");
            component.InstallationMethod = "Loose-File Mod";

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(component.Instructions[0].Action, Is.EqualTo(Instruction.ActionType.Move));
            AssertInstructionIsSandboxed(component.Instructions[0]);
        }

        [Test]
        public void DraftInstructions_K2SiteProsePatterns_DraftSandboxedInstructions()
        {
            var cases = new (string Prose, Instruction.ActionType Expected)[]
            {
                ("Install the files within the Override folder.", Instruction.ActionType.Move),
                ("Download the .tpc variant of the mod. For this mod only, do not overwrite if prompted!", Instruction.ActionType.Move),
                ("Run the HoloPatcher executable. Select the default install, not M4-78.", Instruction.ActionType.Patcher),
                ("If you would like to have Visas's class as Sith Assassin, install the \"Standard + Sith Assassin Visas\" option. Otherwise, simply install \"Standard.\"", Instruction.ActionType.Patcher),
            };

            foreach ((string prose, Instruction.ActionType expected) in cases)
            {
                ModComponent component = CreateComponent(prose);
                component.InstallationMethod = expected == Instruction.ActionType.Patcher ? "HoloPatcher Mod" : "Loose-File Mod";

                IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(new[] { component });

                Assert.That(results, Has.Count.EqualTo(1), $"Expected draft for prose: {prose}");
                Assert.That(component.Instructions.Any(i => i.Action == expected), Is.True, $"Expected {expected} for: {prose}");
                foreach (Instruction instruction in component.Instructions)
                {
                    AssertInstructionIsSandboxed(instruction);
                }
            }
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

        [TestCase("k1", "full.md")]
        [TestCase("k2", "full.md")]
        [TestCase("k1", "spoiler-free.md")]
        [TestCase("k2", "spoiler-free.md")]
        [TestCase("k1", "full_mobile.md")]
        public void RealGuide_ModBuildsMarkdown_DraftedInstructionsAreAllSandboxed(string gameFolder, string guideFile)
        {
            string repoRoot = ResolveRepoRoot();
            string markdownPath = Path.Combine(repoRoot, "mod-builds", "content", gameFolder, guideFile);
            if (!File.Exists(markdownPath))
            {
                Assert.Ignore($"mod-builds guide not found: {markdownPath}");
            }

            List<ModComponent> components;
            try
            {
                components = FileLoadingService.LoadFromFile(markdownPath).ToList();
            }
            catch (InvalidDataException ex)
            {
                Assert.Ignore($"Guide is not a component markdown list ({gameFolder}/{guideFile}): {ex.Message}");
                return;
            }

            Assert.That(components, Is.Not.Empty, $"Expected components from {gameFolder}/{guideFile}");

            IReadOnlyList<DraftInstructionResult> results = DraftInstructionService.GenerateDraftInstructions(components);

            Assert.That(results, Is.Not.Empty, $"Real guide prose ({gameFolder}/{guideFile}) should draft instructions for at least one component");

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

        #region K2 Full site fixture (neocities plain-field markdown)

        [Test]
        public void K2FullGuideFixture_PlainFieldMarkdown_ParsesManyComponents()
        {
            string fixturePath = Path.Combine(ResolveRepoRoot(), "src", "ModSync.Tests", "Fixtures", "k2_full_guide.md");
            Assert.That(File.Exists(fixturePath), Is.True, $"Expected fixture at {fixturePath}");

            string markdown = File.ReadAllText(fixturePath);
            IReadOnlyList<ModComponent> components = ModComponentSerializationService.DeserializeModComponentFromString(markdown, "markdown");

            // Fixture has ~124 plain "Name:" lines and ~169 ### headings; Mod List sections must not collapse to 1.
            Assert.That(components.Count, Is.GreaterThanOrEqualTo(100),
                $"Expected >=100 components from site-scraped K2 Full guide, got {components.Count}");

            Assert.That(components.Any(c => c.Name.IndexOf("Silent Sion", StringComparison.OrdinalIgnoreCase) >= 0), Is.True);
            Assert.That(components.Any(c =>
                !string.IsNullOrWhiteSpace(c.Directions)
                && c.Directions.IndexOf("153sion.dlg", StringComparison.OrdinalIgnoreCase) >= 0), Is.True,
                "Plain 'Installation Instructions' blocks should populate Directions");

            ModComponent withAuthor = components.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.Author)
                && c.Name.IndexOf("TSLRCM", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.That(withAuthor, Is.Not.Null, "Plain Author: fields should populate Author");
        }

        [Test]
        public void K2FullGuideFixture_ParseDirections_DraftsSandboxedInstructions()
        {
            string fixturePath = Path.Combine(ResolveRepoRoot(), "src", "ModSync.Tests", "Fixtures", "k2_full_guide.md");
            Assert.That(File.Exists(fixturePath), Is.True);

            string markdown = File.ReadAllText(fixturePath);
            GuideIngestResult ingested = GuideIngestService.Instance.IngestFromText(markdown, formatHint: "markdown", parseDirections: true);

            Assert.That(ingested.Components.Count, Is.GreaterThanOrEqualTo(100));
            Assert.That(ingested.DraftResults.Count, Is.GreaterThanOrEqualTo(130),
                "K2 Full fixture should draft instructions for the majority of real mod entries");

            foreach (DraftInstructionResult draft in ingested.DraftResults)
            {
                Assert.That(draft.Component.InstallationWarning, Does.Contain(DraftInstructionService.ReviewFlagMessage));
                foreach (Instruction instruction in draft.Component.Instructions)
                {
                    AssertInstructionIsSandboxed(instruction);
                }
            }

            Assert.That(
                ingested.DraftResults.Any(d => d.Component.Instructions.Any(i =>
                    i.Action == Instruction.ActionType.Move
                    || i.Action == Instruction.ActionType.Delete
                    || i.Action == Instruction.ActionType.Extract
                    || i.Action == Instruction.ActionType.Patcher)),
                Is.True,
                "Expected Move/Delete/Extract/Patcher drafts from K2 Full installation prose");
        }

        [Test]
        public void ModBuildsK2Full_BoldFieldMarkdown_StillParsesHighComponentCount()
        {
            string repoRoot = ResolveRepoRoot();
            string markdownPath = Path.Combine(repoRoot, "mod-builds", "content", "k2", "full.md");
            if (!File.Exists(markdownPath))
            {
                // Sibling checkout used by local agents when worktree lacks mod-builds submodule content.
                string sibling = Path.GetFullPath(Path.Combine(repoRoot, "..", "ModSync", "mod-builds", "content", "k2", "full.md"));
                if (File.Exists(sibling))
                {
                    markdownPath = sibling;
                }
                else
                {
                    Assert.Ignore($"mod-builds K2 full.md not found at {markdownPath}");
                }
            }

            IReadOnlyList<ModComponent> components =
                ModComponentSerializationService.DeserializeModComponentFromString(File.ReadAllText(markdownPath), "markdown");

            Assert.That(components.Count, Is.GreaterThanOrEqualTo(100),
                $"Bold **Name:** path must keep working for mod-builds K2 full (got {components.Count})");
        }

        #endregion

        #region Guide emission (GenerateModDocumentation)

        [Test]
        public void GenerateModDocumentation_AfterDraftingGuide_RoundTripsComponentNameAndDirections()
        {
            IReadOnlyList<ModComponent> components = ModComponentSerializationService.DeserializeModComponentFromString(MarkdownGuide);
            Assert.That(components, Has.Count.EqualTo(1));

            DraftInstructionService.GenerateDraftInstructions(components);

            string emitted = ModComponentSerializationService.GenerateModDocumentation(components.ToList());
            Assert.That(emitted, Does.Contain("Guide Ingestion Test Mod"));
            Assert.That(emitted, Does.Contain("Move everything from the Straight Fixes"));

            IReadOnlyList<ModComponent> reparsed = ModComponentSerializationService.DeserializeModComponentFromString(emitted);
            Assert.That(reparsed, Has.Count.EqualTo(1));
            Assert.That(reparsed[0].Name, Is.EqualTo("Guide Ingestion Test Mod"));
            Assert.That(reparsed[0].Directions, Does.Contain("Move everything from the Straight Fixes"));
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
        public void CliConvert_FileInputWithParseDirections_EmitsReviewFlaggedToml()
        {
            string inputMd = Path.Combine(_testDirectory, "guide.md");
            string outputToml = Path.Combine(_testDirectory, "from-file.toml");
            File.WriteAllText(inputMd, MarkdownGuide);

            int exitCode = ModBuildConverter.Run(new[]
            {
                "convert",
                "--input", inputMd,
                "--parse-directions",
                "-f", "toml",
                "-o", outputToml,
                "--plaintext",
            });

            Assert.That(exitCode, Is.EqualTo(0), "convert -i guide.md --parse-directions should succeed");
            Assert.That(File.Exists(outputToml), Is.True);
            string tomlOutput = File.ReadAllText(outputToml);
            Assert.That(tomlOutput, Does.Contain(DraftInstructionService.ReviewFlagMessage));

            var reloaded = ModComponentSerializationService
                .DeserializeModComponentFromString(tomlOutput, "toml")
                .ToList();
            Assert.That(reloaded[0].Instructions, Is.Not.Empty);
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

            // Walk up from the test directory (covers git worktrees and nested bin layouts).
            DirectoryInfo dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "ModSync.sln")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root containing ModSync.sln");
        }
    }
}
