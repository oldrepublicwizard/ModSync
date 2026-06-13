// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

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
    }
}
