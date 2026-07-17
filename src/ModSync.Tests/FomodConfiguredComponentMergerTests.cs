// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;

using ModSync.Core;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodConfiguredComponentMergerTests
    {
        private const string ArchiveFileName = "ExampleMod.zip";

        [Test]
        public void MergeInto_Reconfigure_UpdatesExistingOptionSelection()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(FomodParserTests.RealisticModuleConfigXml);
            ModComponent configured = FomodToComponentMapper.Map(null, config, ArchiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, configured);

            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, true);
            FomodInstallerPresenter.ApplySelectionsToComponent(session);

            var target = new ModComponent { Name = "Target" };
            FomodConfiguredComponentMerger.MergeInto(target, configured, ArchiveFileName);

            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, false);
            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 1, true);
            FomodInstallerPresenter.ApplySelectionsToComponent(session);
            FomodConfiguredComponentMerger.MergeInto(target, configured, ArchiveFileName);

            Assert.That(target.Options.First(option => option.Name == "High Resolution").IsSelected, Is.False);
            Assert.That(target.Options.First(option => option.Name == "Low Resolution").IsSelected, Is.True);
        }

        [Test]
        public void MergeInto_Reconfigure_ReplacesBackslashPrefixedPriorInstructions()
        {
            var target = new ModComponent { Name = "Target" };
            target.Instructions.Add(new Instruction
            {
                Action = Instruction.ActionType.Copy,
                Source = new System.Collections.ObjectModel.ObservableCollection<string>
                {
                    @"<<modDirectory>>\ExampleMod\file.tga",
                },
                Destination = "<<kotorDirectory>>/Override",
            });

            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(FomodParserTests.RealisticModuleConfigXml);
            ModComponent configured = FomodToComponentMapper.Map(null, config, ArchiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, configured);
            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, true);
            FomodInstallerPresenter.ApplySelectionsToComponent(session);

            FomodConfiguredComponentMerger.MergeInto(target, configured, ArchiveFileName);

            Assert.That(
                target.Instructions.Any(i =>
                    i.Source != null
                    && i.Source.Any(s => s != null && s.IndexOf(@"ExampleMod\file.tga", StringComparison.OrdinalIgnoreCase) >= 0)),
                Is.False,
                "Prior backslash-prefixed FOMOD sources must be removed on reconfigure.");
            Assert.That(
                FomodConfiguredComponentMerger.HasArchiveScopedInstructions(target, ArchiveFileName),
                Is.True);
        }

        [Test]
        public void HasArchiveScopedInstructions_MatchesNestedArchiveFileName()
        {
            var component = new ModComponent { Name = "Target" };
            component.Instructions.Add(new Instruction
            {
                Action = Instruction.ActionType.Copy,
                Source = new System.Collections.ObjectModel.ObservableCollection<string>
                {
                    "<<modDirectory>>/ExampleMod/textures/a.tga",
                },
                Destination = "<<kotorDirectory>>/Override",
            });

            Assert.That(
                FomodConfiguredComponentMerger.HasArchiveScopedInstructions(component, "nested/ExampleMod.7z"),
                Is.True);
            Assert.That(
                FomodConfiguredComponentMerger.HasArchiveScopedInstructions(component, "OtherMod.zip"),
                Is.False);
        }

        [Test]
        public void MergeInto_UsesBasenameFolder_WhenArchiveFileNameIncludesNestedPath()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(FomodParserTests.RealisticModuleConfigXml);
            ModComponent configured = FomodToComponentMapper.Map(null, config, "nested/ExampleMod.zip");
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, configured);
            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, true);
            FomodInstallerPresenter.ApplySelectionsToComponent(session);

            var target = new ModComponent { Name = "Target" };
            FomodConfiguredComponentMerger.MergeInto(target, configured, "nested/ExampleMod.zip");

            Assert.That(
                FomodConfiguredComponentMerger.HasArchiveScopedInstructions(target, "ExampleMod.zip"),
                Is.True);
            Assert.That(
                FomodConfiguredComponentMerger.BuildArchivePathPrefix("nested/ExampleMod.zip"),
                Is.EqualTo("<<modDirectory>>/ExampleMod/"));
        }
    }
}
