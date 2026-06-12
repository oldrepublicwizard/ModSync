// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using ModSync.Core;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodToComponentMapperTests
    {
        private const string ArchiveFileName = "UltimateOverhaul-2.1.zip";
        private const string ArchiveFolder = "UltimateOverhaul-2.1";

        private FomodModuleConfig _config;
        private FomodInfo _info;
        private ModComponent _component;

        [SetUp]
        public void SetUp()
        {
            _config = FomodParser.ParseModuleConfigXml(FomodParserTests.RealisticModuleConfigXml);
            _info = new FomodInfo
            {
                Name = "Ultimate KOTOR Overhaul",
                Author = "Revan",
                Description = "A complete visual overhaul.",
            };
            _component = FomodToComponentMapper.Map(_info, _config, ArchiveFileName);
        }

        [Test]
        public void Map_UsesInfoMetadataForComponentIdentity()
        {
            Assert.That(_component.Name, Is.EqualTo("Ultimate KOTOR Overhaul"));
            Assert.That(_component.Author, Is.EqualTo("Revan"));
            Assert.That(_component.Description, Is.EqualTo("A complete visual overhaul."));
            Assert.That(_component.InstallationMethod, Is.EqualTo("FOMOD Installer"));
            Assert.That(_component.Guid, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void Map_NullInfo_FallsBackToModuleName()
        {
            ModComponent component = FomodToComponentMapper.Map(null, _config, ArchiveFileName);

            Assert.That(component.Name, Is.EqualTo("Ultimate KOTOR Overhaul"));
        }

        [Test]
        public void Map_RequiredInstallFiles_BecomeCopyInstructionsOnComponent()
        {
            List<Instruction> copies = _component.Instructions
                .Where(i => i.Action == Instruction.ActionType.Copy)
                .ToList();

            // 2 requiredInstallFiles; the conditional pattern attaches to an option, not the component.
            Assert.That(copies, Has.Count.EqualTo(2));
            Assert.That(copies[0].Source[0], Is.EqualTo($"<<modDirectory>>/{ArchiveFolder}/core/readme.txt"));
            Assert.That(copies[0].Destination, Is.EqualTo("<<kotorDirectory>>/docs"));
            Assert.That(copies[1].Source[0], Is.EqualTo($"<<modDirectory>>/{ArchiveFolder}/core/override/*"));
            Assert.That(copies[1].Destination, Is.EqualTo("<<kotorDirectory>>/Override"));
        }

        [Test]
        public void Map_CreatesOneOptionPerPlugin()
        {
            Assert.That(_component.Options, Has.Count.EqualTo(3));
            Assert.That(
                _component.Options.Select(o => o.Name),
                Is.EqualTo(new[] { "High Resolution", "Low Resolution", "Bonus Music" })
            );
        }

        [Test]
        public void Map_RecordsGroupSemanticsOnOptions()
        {
            Option highResolution = _component.Options[0];
            Assert.That(highResolution.InstallationMethod, Is.EqualTo("SelectExactlyOne"));
            Assert.That(highResolution.Heading, Is.EqualTo("Textures / Texture Quality"));

            Option bonusMusic = _component.Options[2];
            Assert.That(bonusMusic.InstallationMethod, Is.EqualTo("SelectAny"));
            Assert.That(bonusMusic.Heading, Is.EqualTo("Textures / Extras"));
        }

        [Test]
        public void Map_CreatesOneChooseInstructionPerGroup_WiredToOptionGuids()
        {
            List<Instruction> chooseInstructions = _component.Instructions
                .Where(i => i.Action == Instruction.ActionType.Choose)
                .ToList();

            Assert.That(chooseInstructions, Has.Count.EqualTo(2));

            string[] qualityGuids = new[]
            {
                _component.Options[0].Guid.ToString(),
                _component.Options[1].Guid.ToString(),
            };
            Assert.That(chooseInstructions[0].Source, Is.EquivalentTo(qualityGuids));

            string[] extrasGuids = new[] { _component.Options[2].Guid.ToString() };
            Assert.That(chooseInstructions[1].Source, Is.EquivalentTo(extrasGuids));
        }

        [Test]
        public void Map_ChooseInstruction_ResolvesSelectedOptionsThroughNativeModel()
        {
            Instruction qualityChoose = _component.Instructions
                .First(i => i.Action == Instruction.ActionType.Choose);

            IReadOnlyList<Option> chosen = qualityChoose.GetChosenOptions();

            // High Resolution is Recommended, so it is preselected and resolvable via GetChosenOptions.
            Assert.That(chosen, Has.Count.EqualTo(1));
            Assert.That(chosen[0].Name, Is.EqualTo("High Resolution"));
        }

        [Test]
        public void Map_GroupSelectionDefaults_RecommendedPreselected_OptionalNot()
        {
            Assert.That(_component.Options[0].IsSelected, Is.True, "Recommended plugin should be preselected");
            Assert.That(_component.Options[1].IsSelected, Is.False);
            Assert.That(_component.Options[2].IsSelected, Is.False, "SelectAny optional plugin should not be preselected");
        }

        [Test]
        public void Map_PluginFiles_BecomeCopyInstructionsInsideOption()
        {
            Option lowResolution = _component.Options[1];

            Assert.That(lowResolution.Instructions, Has.Count.EqualTo(1));
            Instruction copy = lowResolution.Instructions[0];
            Assert.That(copy.Action, Is.EqualTo(Instruction.ActionType.Copy));
            Assert.That(copy.Source[0], Is.EqualTo($"<<modDirectory>>/{ArchiveFolder}/textures/low/*"));
            Assert.That(copy.Destination, Is.EqualTo("<<kotorDirectory>>/Override"));
        }

        [Test]
        public void Map_ConditionalPatternSatisfiedBySinglePlugin_AttachesToThatOption()
        {
            Option highResolution = _component.Options[0];

            // Folder install from the plugin plus the conditional high_patch.2da install.
            Assert.That(highResolution.Instructions, Has.Count.EqualTo(2));
            Instruction patchCopy = highResolution.Instructions[1];
            Assert.That(patchCopy.Source[0], Is.EqualTo($"<<modDirectory>>/{ArchiveFolder}/patches/high_patch.2da"));
            Assert.That(patchCopy.Destination, Is.EqualTo("<<kotorDirectory>>/Override"));
        }

        [Test]
        public void Map_EveryGeneratedPath_StartsWithSandboxPlaceholder()
        {
            var allInstructions = new List<Instruction>();
            allInstructions.AddRange(_component.Instructions);
            foreach (Option option in _component.Options)
            {
                allInstructions.AddRange(option.Instructions);
            }

            Assert.That(allInstructions, Is.Not.Empty);
            foreach (Instruction instruction in allInstructions)
            {
                if (instruction.Action == Instruction.ActionType.Choose)
                {
                    // Choose is the documented exception: Source lists option GUIDs.
                    foreach (string source in instruction.Source)
                    {
                        Assert.That(Guid.TryParse(source, out _), Is.True, $"Choose source '{source}' should be an option GUID");
                    }

                    continue;
                }

                foreach (string source in instruction.Source)
                {
                    Assert.That(
                        source.StartsWith("<<modDirectory>>", StringComparison.Ordinal)
                            || source.StartsWith("<<kotorDirectory>>", StringComparison.Ordinal),
                        Is.True,
                        $"Instruction source '{source}' must start with a sandbox placeholder"
                    );
                }

                Assert.That(
                    instruction.Destination.StartsWith("<<modDirectory>>", StringComparison.Ordinal)
                        || instruction.Destination.StartsWith("<<kotorDirectory>>", StringComparison.Ordinal),
                    Is.True,
                    $"Instruction destination '{instruction.Destination}' must start with a sandbox placeholder"
                );
            }
        }

        [Test]
        public void Map_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => FomodToComponentMapper.Map(_info, null, ArchiveFileName));
        }

        [Test]
        public void Map_MissingArchiveName_Throws()
        {
            Assert.Throws<ArgumentException>(() => FomodToComponentMapper.Map(_info, _config, "  "));
        }

        [Test]
        public void Map_PathTraversalInFomodPaths_Throws()
        {
            var config = new FomodModuleConfig
            {
                ModuleName = "Evil",
                RequiredInstallFiles =
                {
                    new FomodFileInstall { Source = @"..\..\outside.txt", Destination = "Override" },
                },
            };

            Assert.Throws<FormatException>(() => FomodToComponentMapper.Map(null, config, ArchiveFileName));
        }

        [Test]
        public void EvaluateDependency_AndOfFlags_RequiresAllFlags()
        {
            var dependency = new FomodDependency
            {
                Type = FomodDependencyType.Composite,
                Operator = FomodDependencyOperator.And,
                Children =
                {
                    new FomodDependency { Type = FomodDependencyType.Flag, FlagName = "A", FlagValue = "On" },
                    new FomodDependency { Type = FomodDependencyType.Flag, FlagName = "B", FlagValue = "On" },
                },
            };

            var bothSet = new Dictionary<string, string> { { "A", "On" }, { "B", "On" } };
            var oneSet = new Dictionary<string, string> { { "A", "On" } };

            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, bothSet), Is.True);
            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, oneSet), Is.False);
        }

        [Test]
        public void EvaluateDependency_OrOfFlags_RequiresAnyFlag()
        {
            var dependency = new FomodDependency
            {
                Type = FomodDependencyType.Composite,
                Operator = FomodDependencyOperator.Or,
                Children =
                {
                    new FomodDependency { Type = FomodDependencyType.Flag, FlagName = "A", FlagValue = "On" },
                    new FomodDependency { Type = FomodDependencyType.Flag, FlagName = "B", FlagValue = "On" },
                },
            };

            var onlyB = new Dictionary<string, string> { { "B", "On" } };
            var neither = new Dictionary<string, string> { { "C", "On" } };

            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, onlyB), Is.True);
            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, neither), Is.False);
        }

        [Test]
        public void EvaluateDependency_GameAndFileDependencies_AreAlwaysTrue()
        {
            var gameDependency = new FomodDependency { Type = FomodDependencyType.Game, GameVersion = "1.0" };
            var fileDependency = new FomodDependency { Type = FomodDependencyType.File, FilePath = "dialog.tlk" };
            var noFlags = new Dictionary<string, string>();

            Assert.That(FomodToComponentMapper.EvaluateDependency(gameDependency, noFlags), Is.True);
            Assert.That(FomodToComponentMapper.EvaluateDependency(fileDependency, noFlags), Is.True);
        }

        [Test]
        public void EvaluateDependency_FlagValueComparison_IsCaseSensitiveOnValue()
        {
            var dependency = new FomodDependency { Type = FomodDependencyType.Flag, FlagName = "A", FlagValue = "On" };
            var wrongCase = new Dictionary<string, string> { { "a", "ON" } };
            var matching = new Dictionary<string, string> { { "a", "On" } };

            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, wrongCase), Is.False);
            Assert.That(FomodToComponentMapper.EvaluateDependency(dependency, matching), Is.True);
        }
    }
}
