// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Parses the two XML files of a FOMOD installer package:
    /// <c>fomod/info.xml</c> (mod metadata) and <c>fomod/ModuleConfig.xml</c>
    /// (install steps, groups, plugins, file installs, condition flags).
    /// The FOMOD ModuleConfig schema uses camelCase element names
    /// (moduleName, requiredInstallFiles, installSteps, ...) with
    /// name/source/destination/priority attributes, while info.xml typically uses
    /// PascalCase element names; all element and attribute lookups here are
    /// case-insensitive so both conventions and real-world variations are accepted.
    /// Missing optional elements are tolerated; structurally invalid XML throws
    /// a <see cref="FormatException"/>.
    /// </summary>
    public static class FomodParser
    {
        /// <summary>Parses the contents of a fomod/info.xml document.</summary>
        /// <exception cref="FormatException">The text is not well-formed XML.</exception>
        [NotNull]
        public static FomodInfo ParseInfoXml([NotNull] string xml)
        {
            XElement root = LoadRoot(xml, "info.xml");
            var info = new FomodInfo
            {
                Name = ElementValue(root, "Name"),
                Author = ElementValue(root, "Author"),
                Version = ElementValue(root, "Version"),
                Website = ElementValue(root, "Website"),
                Description = ElementValue(root, "Description"),
            };
            return info;
        }

        /// <summary>Parses fomod/info.xml from a stream.</summary>
        [NotNull]
        public static FomodInfo ParseInfoXml([NotNull] Stream stream)
        {
            return ParseInfoXml(ReadAllText(stream));
        }

        /// <summary>Parses fomod/info.xml from a file on disk.</summary>
        [NotNull]
        public static FomodInfo ParseInfoXmlFile([NotNull] string filePath)
        {
            return ParseInfoXml(File.ReadAllText(filePath));
        }

        /// <summary>Parses the contents of a fomod/ModuleConfig.xml document.</summary>
        /// <exception cref="FormatException">The text is not well-formed XML or the root element is not a FOMOD config element.</exception>
        [NotNull]
        public static FomodModuleConfig ParseModuleConfigXml([NotNull] string xml)
        {
            XElement root = LoadRoot(xml, "ModuleConfig.xml");
            if (!string.Equals(root.Name.LocalName, "config", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"ModuleConfig.xml root element must be 'config' but was '{root.Name.LocalName}'."
                );
            }

            var config = new FomodModuleConfig
            {
                ModuleName = ElementValue(root, "moduleName"),
            };

            XElement requiredInstallFiles = ChildElement(root, "requiredInstallFiles");
            if (requiredInstallFiles != null)
            {
                config.RequiredInstallFiles.AddRange(ParseFileList(requiredInstallFiles));
            }

            XElement installSteps = ChildElement(root, "installSteps");
            if (installSteps != null)
            {
                foreach (XElement stepElement in ChildElements(installSteps, "installStep"))
                {
                    config.InstallSteps.Add(ParseInstallStep(stepElement));
                }
            }

            XElement conditionalFileInstalls = ChildElement(root, "conditionalFileInstalls");
            XElement patterns = conditionalFileInstalls is null ? null : ChildElement(conditionalFileInstalls, "patterns");
            if (patterns != null)
            {
                foreach (XElement patternElement in ChildElements(patterns, "pattern"))
                {
                    config.ConditionalInstallPatterns.Add(ParseConditionalInstallPattern(patternElement));
                }
            }

            return config;
        }

        /// <summary>Parses fomod/ModuleConfig.xml from a stream.</summary>
        [NotNull]
        public static FomodModuleConfig ParseModuleConfigXml([NotNull] Stream stream)
        {
            return ParseModuleConfigXml(ReadAllText(stream));
        }

        /// <summary>Parses fomod/ModuleConfig.xml from a file on disk.</summary>
        [NotNull]
        public static FomodModuleConfig ParseModuleConfigXmlFile([NotNull] string filePath)
        {
            return ParseModuleConfigXml(File.ReadAllText(filePath));
        }

        [NotNull]
        private static XElement LoadRoot([NotNull] string xml, [NotNull] string documentName)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new FormatException($"{documentName} content is empty.");
            }

            XDocument document;
            try
            {
                document = XDocument.Parse(xml);
            }
            catch (XmlException ex)
            {
                throw new FormatException($"{documentName} is not well-formed XML: {ex.Message}", ex);
            }

            XElement root = document.Root;
            if (root is null)
            {
                throw new FormatException($"{documentName} has no root element.");
            }

            return root;
        }

        [NotNull]
        private static FomodInstallStep ParseInstallStep([NotNull] XElement stepElement)
        {
            var step = new FomodInstallStep
            {
                Name = AttributeValue(stepElement, "name"),
            };

            XElement visible = ChildElement(stepElement, "visible");
            if (visible != null)
            {
                step.Visible = ParseDependencyContainer(visible);
            }

            XElement optionalFileGroups = ChildElement(stepElement, "optionalFileGroups");
            if (optionalFileGroups != null)
            {
                foreach (XElement groupElement in ChildElements(optionalFileGroups, "group"))
                {
                    step.Groups.Add(ParseGroup(groupElement));
                }
            }

            return step;
        }

        [NotNull]
        private static FomodGroup ParseGroup([NotNull] XElement groupElement)
        {
            var group = new FomodGroup
            {
                Name = AttributeValue(groupElement, "name"),
                Type = ParseEnum(AttributeValue(groupElement, "type"), FomodGroupType.SelectAny),
            };

            XElement plugins = ChildElement(groupElement, "plugins");
            if (plugins != null)
            {
                foreach (XElement pluginElement in ChildElements(plugins, "plugin"))
                {
                    group.Plugins.Add(ParsePlugin(pluginElement));
                }
            }

            return group;
        }

        [NotNull]
        private static FomodPlugin ParsePlugin([NotNull] XElement pluginElement)
        {
            var plugin = new FomodPlugin
            {
                Name = AttributeValue(pluginElement, "name"),
                Description = ElementValue(pluginElement, "description").Trim(),
            };

            XElement image = ChildElement(pluginElement, "image");
            if (image != null)
            {
                plugin.ImagePath = AttributeValue(image, "path");
            }

            XElement files = ChildElement(pluginElement, "files");
            if (files != null)
            {
                plugin.Files.AddRange(ParseFileList(files));
            }

            XElement conditionFlags = ChildElement(pluginElement, "conditionFlags");
            if (conditionFlags != null)
            {
                foreach (XElement flagElement in ChildElements(conditionFlags, "flag"))
                {
                    plugin.ConditionFlags.Add(new FomodConditionFlag
                    {
                        Name = AttributeValue(flagElement, "name"),
                        Value = flagElement.Value.Trim(),
                    });
                }
            }

            plugin.TypeDescriptor = ParseTypeDescriptor(ChildElement(pluginElement, "typeDescriptor"));
            return plugin;
        }

        /// <summary>
        /// Resolves a typeDescriptor element to a plugin type. Supports both the
        /// simple form (type name="...") and the dependencyType form, where only the
        /// defaultType is honored; per-pattern dependency types are out of scope for
        /// this slice and fall back to the default type.
        /// </summary>
        private static FomodPluginType ParseTypeDescriptor([CanBeNull] XElement typeDescriptor)
        {
            if (typeDescriptor is null)
            {
                return FomodPluginType.Optional;
            }

            XElement type = ChildElement(typeDescriptor, "type");
            if (type != null)
            {
                return ParseEnum(AttributeValue(type, "name"), FomodPluginType.Optional);
            }

            XElement dependencyType = ChildElement(typeDescriptor, "dependencyType");
            XElement defaultType = dependencyType is null ? null : ChildElement(dependencyType, "defaultType");
            if (defaultType != null)
            {
                return ParseEnum(AttributeValue(defaultType, "name"), FomodPluginType.Optional);
            }

            return FomodPluginType.Optional;
        }

        [NotNull]
        private static FomodConditionalInstallPattern ParseConditionalInstallPattern([NotNull] XElement patternElement)
        {
            var pattern = new FomodConditionalInstallPattern();

            XElement dependencies = ChildElement(patternElement, "dependencies");
            if (dependencies != null)
            {
                pattern.Dependencies = ParseDependencyContainer(dependencies);
            }

            XElement files = ChildElement(patternElement, "files");
            if (files != null)
            {
                pattern.Files.AddRange(ParseFileList(files));
            }

            return pattern;
        }

        /// <summary>
        /// Parses a dependency container (a dependencies or visible element) into a
        /// composite dependency node. Recognized children: fileDependency,
        /// flagDependency, gameDependency, and nested dependencies elements.
        /// </summary>
        [NotNull]
        private static FomodDependency ParseDependencyContainer([NotNull] XElement containerElement)
        {
            var dependency = new FomodDependency
            {
                Type = FomodDependencyType.Composite,
                Operator = string.Equals(AttributeValue(containerElement, "operator"), "Or", StringComparison.OrdinalIgnoreCase)
                    ? FomodDependencyOperator.Or
                    : FomodDependencyOperator.And,
            };

            foreach (XElement child in containerElement.Elements())
            {
                string localName = child.Name.LocalName;
                if (string.Equals(localName, "fileDependency", StringComparison.OrdinalIgnoreCase))
                {
                    dependency.Children.Add(new FomodDependency
                    {
                        Type = FomodDependencyType.File,
                        FilePath = AttributeValue(child, "file"),
                        FileState = AttributeValue(child, "state"),
                    });
                }
                else if (string.Equals(localName, "flagDependency", StringComparison.OrdinalIgnoreCase))
                {
                    dependency.Children.Add(new FomodDependency
                    {
                        Type = FomodDependencyType.Flag,
                        FlagName = AttributeValue(child, "flag"),
                        FlagValue = AttributeValue(child, "value"),
                    });
                }
                else if (string.Equals(localName, "gameDependency", StringComparison.OrdinalIgnoreCase))
                {
                    dependency.Children.Add(new FomodDependency
                    {
                        Type = FomodDependencyType.Game,
                        GameVersion = AttributeValue(child, "version"),
                    });
                }
                else if (string.Equals(localName, "dependencies", StringComparison.OrdinalIgnoreCase))
                {
                    dependency.Children.Add(ParseDependencyContainer(child));
                }
            }

            return dependency;
        }

        [NotNull]
        [ItemNotNull]
        private static List<FomodFileInstall> ParseFileList([NotNull] XElement filesContainer)
        {
            var result = new List<FomodFileInstall>();
            foreach (XElement child in filesContainer.Elements())
            {
                string localName = child.Name.LocalName;
                bool isFile = string.Equals(localName, "file", StringComparison.OrdinalIgnoreCase);
                bool isFolder = string.Equals(localName, "folder", StringComparison.OrdinalIgnoreCase);
                if (!isFile && !isFolder)
                {
                    continue;
                }

                var install = new FomodFileInstall
                {
                    Source = AttributeValue(child, "source"),
                    Destination = AttributeValue(child, "destination"),
                    IsFolder = isFolder,
                };
                string priorityText = AttributeValue(child, "priority");
                if (int.TryParse(priorityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int priority))
                {
                    install.Priority = priority;
                }

                result.Add(install);
            }

            return result;
        }

        private static TEnum ParseEnum<TEnum>([CanBeNull] string text, TEnum fallback)
            where TEnum : struct
        {
            if (!string.IsNullOrWhiteSpace(text) && Enum.TryParse(text, ignoreCase: true, out TEnum parsed))
            {
                return parsed;
            }

            return fallback;
        }

        [CanBeNull]
        private static XElement ChildElement([CanBeNull] XElement parent, [NotNull] string localName)
        {
            return parent?.Elements().FirstOrDefault(
                e => string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase)
            );
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<XElement> ChildElements([NotNull] XElement parent, [NotNull] string localName)
        {
            return parent.Elements().Where(
                e => string.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase)
            );
        }

        [NotNull]
        private static string ElementValue([CanBeNull] XElement parent, [NotNull] string localName)
        {
            XElement element = ChildElement(parent, localName);
            return element is null ? string.Empty : element.Value;
        }

        [NotNull]
        private static string AttributeValue([CanBeNull] XElement element, [NotNull] string localName)
        {
            if (element is null)
            {
                return string.Empty;
            }

            XAttribute attribute = element.Attributes().FirstOrDefault(
                a => string.Equals(a.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase)
            );
            return attribute is null ? string.Empty : attribute.Value;
        }

        [NotNull]
        private static string ReadAllText([NotNull] Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
