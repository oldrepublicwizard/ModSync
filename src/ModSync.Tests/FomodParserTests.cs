// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodParserTests
    {
        internal const string RealisticModuleConfigXml = """
            <config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
              <moduleName>Ultimate KOTOR Overhaul</moduleName>
              <requiredInstallFiles>
                <file source="core\readme.txt" destination="docs\readme.txt" priority="0" />
                <folder source="core\override" destination="Override" />
              </requiredInstallFiles>
              <installSteps order="Explicit">
                <installStep name="Textures">
                  <optionalFileGroups order="Explicit">
                    <group name="Texture Quality" type="SelectExactlyOne">
                      <plugins order="Explicit">
                        <plugin name="High Resolution">
                          <description>Crisp 4K textures.</description>
                          <image path="fomod\images\high.png" />
                          <files>
                            <folder source="textures\high" destination="Override" priority="1" />
                          </files>
                          <conditionFlags>
                            <flag name="TextureQuality">High</flag>
                          </conditionFlags>
                          <typeDescriptor>
                            <type name="Recommended" />
                          </typeDescriptor>
                        </plugin>
                        <plugin name="Low Resolution">
                          <description>Lightweight 1K textures.</description>
                          <files>
                            <folder source="textures\low" destination="Override" />
                          </files>
                          <conditionFlags>
                            <flag name="TextureQuality">Low</flag>
                          </conditionFlags>
                          <typeDescriptor>
                            <type name="Optional" />
                          </typeDescriptor>
                        </plugin>
                      </plugins>
                    </group>
                    <group name="Extras" type="SelectAny">
                      <plugins order="Explicit">
                        <plugin name="Bonus Music">
                          <description>Extra music tracks.</description>
                          <files>
                            <file source="music\bonus.wav" destination="streammusic\bonus.wav" />
                          </files>
                          <typeDescriptor>
                            <type name="Optional" />
                          </typeDescriptor>
                        </plugin>
                      </plugins>
                    </group>
                  </optionalFileGroups>
                </installStep>
              </installSteps>
              <conditionalFileInstalls>
                <patterns>
                  <pattern>
                    <dependencies operator="And">
                      <flagDependency flag="TextureQuality" value="High" />
                    </dependencies>
                    <files>
                      <file source="patches\high_patch.2da" destination="Override\high_patch.2da" />
                    </files>
                  </pattern>
                </patterns>
              </conditionalFileInstalls>
            </config>
            """;

        [Test]
        public void ParseModuleConfigXml_RealisticDocument_ParsesModuleNameAndRequiredFiles()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(RealisticModuleConfigXml);

            Assert.That(config.ModuleName, Is.EqualTo("Ultimate KOTOR Overhaul"));
            Assert.That(config.RequiredInstallFiles, Has.Count.EqualTo(2));
            Assert.That(config.RequiredInstallFiles[0].Source, Is.EqualTo(@"core\readme.txt"));
            Assert.That(config.RequiredInstallFiles[0].Destination, Is.EqualTo(@"docs\readme.txt"));
            Assert.That(config.RequiredInstallFiles[0].IsFolder, Is.False);
            Assert.That(config.RequiredInstallFiles[1].IsFolder, Is.True);
            Assert.That(config.RequiredInstallFiles[1].Destination, Is.EqualTo("Override"));
        }

        [Test]
        public void ParseModuleConfigXml_RealisticDocument_ParsesStepsGroupsAndPlugins()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(RealisticModuleConfigXml);

            Assert.That(config.InstallSteps, Has.Count.EqualTo(1));
            FomodInstallStep step = config.InstallSteps[0];
            Assert.That(step.Name, Is.EqualTo("Textures"));
            Assert.That(step.Groups, Has.Count.EqualTo(2));

            FomodGroup qualityGroup = step.Groups[0];
            Assert.That(qualityGroup.Name, Is.EqualTo("Texture Quality"));
            Assert.That(qualityGroup.Type, Is.EqualTo(FomodGroupType.SelectExactlyOne));
            Assert.That(qualityGroup.Plugins, Has.Count.EqualTo(2));

            FomodPlugin highResolution = qualityGroup.Plugins[0];
            Assert.That(highResolution.Name, Is.EqualTo("High Resolution"));
            Assert.That(highResolution.Description, Is.EqualTo("Crisp 4K textures."));
            Assert.That(highResolution.ImagePath, Is.EqualTo(@"fomod\images\high.png"));
            Assert.That(highResolution.TypeDescriptor, Is.EqualTo(FomodPluginType.Recommended));
            Assert.That(highResolution.Files, Has.Count.EqualTo(1));
            Assert.That(highResolution.Files[0].IsFolder, Is.True);
            Assert.That(highResolution.Files[0].Priority, Is.EqualTo(1));
            Assert.That(highResolution.ConditionFlags, Has.Count.EqualTo(1));
            Assert.That(highResolution.ConditionFlags[0].Name, Is.EqualTo("TextureQuality"));
            Assert.That(highResolution.ConditionFlags[0].Value, Is.EqualTo("High"));

            FomodGroup extrasGroup = step.Groups[1];
            Assert.That(extrasGroup.Type, Is.EqualTo(FomodGroupType.SelectAny));
            Assert.That(extrasGroup.Plugins, Has.Count.EqualTo(1));
            Assert.That(extrasGroup.Plugins[0].Files[0].Source, Is.EqualTo(@"music\bonus.wav"));
        }

        [Test]
        public void ParseModuleConfigXml_RealisticDocument_ParsesConditionalFileInstalls()
        {
            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(RealisticModuleConfigXml);

            Assert.That(config.ConditionalInstallPatterns, Has.Count.EqualTo(1));
            FomodConditionalInstallPattern pattern = config.ConditionalInstallPatterns[0];
            Assert.That(pattern.Dependencies.Type, Is.EqualTo(FomodDependencyType.Composite));
            Assert.That(pattern.Dependencies.Operator, Is.EqualTo(FomodDependencyOperator.And));
            Assert.That(pattern.Dependencies.Children, Has.Count.EqualTo(1));
            Assert.That(pattern.Dependencies.Children[0].Type, Is.EqualTo(FomodDependencyType.Flag));
            Assert.That(pattern.Dependencies.Children[0].FlagName, Is.EqualTo("TextureQuality"));
            Assert.That(pattern.Dependencies.Children[0].FlagValue, Is.EqualTo("High"));
            Assert.That(pattern.Files, Has.Count.EqualTo(1));
            Assert.That(pattern.Files[0].Source, Is.EqualTo(@"patches\high_patch.2da"));
        }

        [Test]
        public void ParseModuleConfigXml_MixedDependencyKinds_ParsesAllLeafTypes()
        {
            const string xml = """
                <config>
                  <moduleName>Deps</moduleName>
                  <conditionalFileInstalls>
                    <patterns>
                      <pattern>
                        <dependencies operator="Or">
                          <fileDependency file="dialog.tlk" state="Active" />
                          <gameDependency version="1.0.3" />
                          <dependencies operator="And">
                            <flagDependency flag="A" value="On" />
                          </dependencies>
                        </dependencies>
                        <files>
                          <file source="x.txt" destination="x.txt" />
                        </files>
                      </pattern>
                    </patterns>
                  </conditionalFileInstalls>
                </config>
                """;

            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(xml);

            FomodDependency dependencies = config.ConditionalInstallPatterns[0].Dependencies;
            Assert.That(dependencies.Operator, Is.EqualTo(FomodDependencyOperator.Or));
            Assert.That(dependencies.Children, Has.Count.EqualTo(3));
            Assert.That(dependencies.Children[0].Type, Is.EqualTo(FomodDependencyType.File));
            Assert.That(dependencies.Children[0].FilePath, Is.EqualTo("dialog.tlk"));
            Assert.That(dependencies.Children[0].FileState, Is.EqualTo("Active"));
            Assert.That(dependencies.Children[1].Type, Is.EqualTo(FomodDependencyType.Game));
            Assert.That(dependencies.Children[1].GameVersion, Is.EqualTo("1.0.3"));
            Assert.That(dependencies.Children[2].Type, Is.EqualTo(FomodDependencyType.Composite));
            Assert.That(dependencies.Children[2].Children, Has.Count.EqualTo(1));
        }

        [Test]
        public void ParseModuleConfigXml_MissingOptionalSections_IsTolerated()
        {
            const string xml = "<config><moduleName>Bare</moduleName></config>";

            FomodModuleConfig config = FomodParser.ParseModuleConfigXml(xml);

            Assert.That(config.ModuleName, Is.EqualTo("Bare"));
            Assert.That(config.RequiredInstallFiles, Is.Empty);
            Assert.That(config.InstallSteps, Is.Empty);
            Assert.That(config.ConditionalInstallPatterns, Is.Empty);
        }

        [Test]
        public void ParseModuleConfigXml_InvalidXml_ThrowsFormatException()
        {
            const string xml = "<config><moduleName>Broken</config>";

            Assert.Throws<FormatException>(() => FomodParser.ParseModuleConfigXml(xml));
        }

        [Test]
        public void ParseModuleConfigXml_EmptyContent_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => FomodParser.ParseModuleConfigXml("   "));
        }

        [Test]
        public void ParseModuleConfigXml_WrongRootElement_ThrowsFormatException()
        {
            const string xml = "<fomod><moduleName>NotAConfig</moduleName></fomod>";

            Assert.Throws<FormatException>(() => FomodParser.ParseModuleConfigXml(xml));
        }

        [Test]
        public void ParseInfoXml_TypicalDocument_ParsesAllFields()
        {
            const string xml = """
                <fomod>
                  <Name>Ultimate KOTOR Overhaul</Name>
                  <Author>Revan</Author>
                  <Version>2.1.0</Version>
                  <Website>https://example.com/mods/1</Website>
                  <Description>A complete visual overhaul.</Description>
                </fomod>
                """;

            FomodInfo info = FomodParser.ParseInfoXml(xml);

            Assert.That(info.Name, Is.EqualTo("Ultimate KOTOR Overhaul"));
            Assert.That(info.Author, Is.EqualTo("Revan"));
            Assert.That(info.Version, Is.EqualTo("2.1.0"));
            Assert.That(info.Website, Is.EqualTo("https://example.com/mods/1"));
            Assert.That(info.Description, Is.EqualTo("A complete visual overhaul."));
        }

        [Test]
        public void ParseInfoXml_MissingElements_MapToEmptyStrings()
        {
            const string xml = "<fomod><Name>Minimal</Name></fomod>";

            FomodInfo info = FomodParser.ParseInfoXml(xml);

            Assert.That(info.Name, Is.EqualTo("Minimal"));
            Assert.That(info.Author, Is.Empty);
            Assert.That(info.Version, Is.Empty);
            Assert.That(info.Website, Is.Empty);
            Assert.That(info.Description, Is.Empty);
        }

        [Test]
        public void ParseInfoXml_InvalidXml_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => FomodParser.ParseInfoXml("<fomod><Name>Broken</fomod>"));
        }

        [Test]
        public void FindModuleConfigPath_ExactPath_IsFound()
        {
            var entries = new List<string> { "readme.txt", "fomod/ModuleConfig.xml", "Override/file.tga" };

            string found = FomodDetector.FindModuleConfigPath(entries);

            Assert.That(found, Is.EqualTo("fomod/ModuleConfig.xml"));
        }

        [Test]
        public void FindModuleConfigPath_DifferentCaseAndRootPrefix_IsFound()
        {
            var entries = new List<string> { "MyMod-1.0/readme.txt", "MyMod-1.0/Fomod/moduleconfig.XML" };

            string found = FomodDetector.FindModuleConfigPath(entries);

            Assert.That(found, Is.EqualTo("MyMod-1.0/Fomod/moduleconfig.XML"));
        }

        [Test]
        public void FindModuleConfigPath_BackslashSeparators_IsFound()
        {
            var entries = new List<string> { @"MyMod\fomod\ModuleConfig.xml" };

            string found = FomodDetector.FindModuleConfigPath(entries);

            Assert.That(found, Is.EqualTo(@"MyMod\fomod\ModuleConfig.xml"));
        }

        [Test]
        public void FindModuleConfigPath_NoFomodDirectory_ReturnsNull()
        {
            var entries = new List<string> { "ModuleConfig.xml", "notfomod/ModuleConfig.xml.bak", "docs/fomod/readme.txt" };

            string found = FomodDetector.FindModuleConfigPath(entries);

            Assert.That(found, Is.Null);
        }

        [Test]
        public void FindModuleConfigPath_NullOrEmptyInput_ReturnsNull()
        {
            Assert.That(FomodDetector.FindModuleConfigPath(null), Is.Null);
            Assert.That(FomodDetector.FindModuleConfigPath(new List<string>()), Is.Null);
            Assert.That(FomodDetector.FindModuleConfigPath(new List<string> { null, "" }), Is.Null);
        }
    }
}
