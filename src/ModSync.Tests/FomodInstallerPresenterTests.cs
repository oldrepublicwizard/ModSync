// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using ModSync.Core;
using ModSync.Core.Services.Fomod;
using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodInstallerPresenterTests
    {
        private const string ArchiveFileName = "UltimateOverhaul-2.1.zip";

        private FomodModuleConfig _config;
        private ModComponent _component;
        private FomodInstallerSession _session;

        [SetUp]
        public void SetUp()
        {
            _config = FomodParser.ParseModuleConfigXml(FomodParserTests.RealisticModuleConfigXml);
            _component = FomodToComponentMapper.Map(null, _config, ArchiveFileName);
            _session = FomodInstallerPresenter.CreateSession(_config, _component);
        }

        [Test]
        public void CreateSession_LinksMappedOptionsToPlugins()
        {
            Assert.That(_session.Steps, Has.Count.EqualTo(1));
            Assert.That(_session.Steps[0].Groups, Has.Count.EqualTo(2));
            Assert.That(_session.Steps[0].Groups[0].Plugins.Select(plugin => plugin.Name), Is.EqualTo(new[] { "High Resolution", "Low Resolution" }));
            Assert.That(_session.Steps[0].Groups[0].Plugins[0].OptionGuid, Is.EqualTo(_component.Options[0].Guid));
        }

        [Test]
        public void ValidateStep_SelectExactlyOne_RequiresSingleSelection()
        {
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 0, 0, false);
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 0, 1, false);

            string message = FomodInstallerPresenter.ValidateStep(_session, 0);
            Assert.That(message, Does.Contain("exactly one"));
        }

        [Test]
        public void TrySetPluginSelected_SelectExactlyOne_KeepsOnlyOnePluginSelected()
        {
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 0, 1, true);

            Assert.That(_session.Steps[0].Groups[0].Plugins[0].IsSelected, Is.False);
            Assert.That(_session.Steps[0].Groups[0].Plugins[1].IsSelected, Is.True);
        }

        [Test]
        public void BuildFlagValues_ReflectsSelectedPluginConditionFlags()
        {
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 0, 0, true);

            IReadOnlyDictionary<string, string> flagValues = FomodInstallerPresenter.BuildFlagValues(_session);
            Assert.That(flagValues["TextureQuality"], Is.EqualTo("High"));
        }

        [Test]
        public void ApplySelectionsToComponent_UpdatesComponentOptions()
        {
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 0, 1, true);
            FomodInstallerPresenter.TrySetPluginSelected(_session, 0, 1, 0, true);

            FomodInstallerPresenter.ApplySelectionsToComponent(_session);

            Assert.That(_component.Options[0].IsSelected, Is.False);
            Assert.That(_component.Options[1].IsSelected, Is.True);
            Assert.That(_component.Options[2].IsSelected, Is.True);
        }

        [Test]
        public void GetVisibleStepIndices_HidesStepWhenVisibleDependencyFails()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml("""
                <config>
                  <installSteps order="Explicit">
                    <installStep name="Textures">
                      <optionalFileGroups order="Explicit">
                        <group name="Quality" type="SelectExactlyOne">
                          <plugins order="Explicit">
                            <plugin name="High">
                              <conditionFlags><flag name="Tier">High</flag></conditionFlags>
                            </plugin>
                            <plugin name="Low" />
                          </plugins>
                        </group>
                      </optionalFileGroups>
                    </installStep>
                    <installStep name="Extras">
                      <visible>
                        <dependencies operator="And">
                          <flagDependency flag="Tier" value="High" />
                        </dependencies>
                      </visible>
                      <optionalFileGroups order="Explicit">
                        <group name="Bonus" type="SelectAny">
                          <plugins order="Explicit">
                            <plugin name="Extra Files" />
                          </plugins>
                        </group>
                      </optionalFileGroups>
                    </installStep>
                  </installSteps>
                </config>
                """);

            ModComponent component = FomodToComponentMapper.Map(null, config, ArchiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, component);

            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, false);
            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 1, true);
            IReadOnlyList<int> initialVisible = FomodInstallerPresenter.GetVisibleStepIndices(session);
            Assert.That(initialVisible, Is.EqualTo(new[] { 0 }));

            FomodInstallerPresenter.TrySetPluginSelected(session, 0, 0, 0, true);
            IReadOnlyList<int> afterHigh = FomodInstallerPresenter.GetVisibleStepIndices(session);
            Assert.That(afterHigh, Is.EqualTo(new[] { 0, 1 }));
        }
    }
}
