// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using ModSync.Core.Services.Fomod;

namespace ModSync.Core.CLI
{
    /// <summary>
    /// Full terminal FOMOD wizard using the shared presenter rules.
    /// </summary>
    public static class FomodConsoleWizard
    {
        [CanBeNull]
        public static ModComponent Run(
            [NotNull] string extractedArchiveDirectory,
            [CanBeNull] string componentDisplayName,
            [CanBeNull] string archiveFileName = null)
        {
            string moduleConfigPath = FomodArchiveDiscovery.FindModuleConfigPath(extractedArchiveDirectory);
            if (moduleConfigPath is null)
            {
                Console.Error.WriteLine("No fomod/ModuleConfig.xml was found in the extracted archive.");
                return null;
            }

            FomodModuleConfig config = FomodParser.ParseModuleConfigXmlFile(moduleConfigPath);
            FomodInfo info = null;
            string infoPath = FomodArchiveDiscovery.FindInfoPath(extractedArchiveDirectory);
            if (infoPath != null)
            {
                info = FomodParser.ParseInfoXmlFile(infoPath);
            }

            string resolvedArchiveFileName = !string.IsNullOrWhiteSpace(archiveFileName)
                ? archiveFileName
                : Path.GetFileName(extractedArchiveDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + ".zip";
            ModComponent component = FomodToComponentMapper.Map(info, config, resolvedArchiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, component);

            string title = string.IsNullOrWhiteSpace(componentDisplayName)
                ? "FOMOD Installer"
                : $"FOMOD Installer — {componentDisplayName}";
            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine(new string('=', Math.Min(title.Length, 72)));

            IReadOnlyList<int> visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(session);
            int visibleCursor = 0;

            while (visibleCursor < visibleStepIndices.Count)
            {
                int stepIndex = visibleStepIndices[visibleCursor];
                FomodInstallerStepModel step = session.Steps[stepIndex];

                Console.WriteLine();
                Console.WriteLine($"Step {visibleCursor + 1} of {visibleStepIndices.Count}: {step.Name}");
                Console.WriteLine(new string('-', 40));

                for (int groupIndex = 0; groupIndex < step.Groups.Count; groupIndex++)
                {
                    FomodInstallerGroupModel group = step.Groups[groupIndex];
                    Console.WriteLine();
                    Console.WriteLine(group.Name + " (" + DescribeGroupType(group.GroupType) + ")");

                    for (int pluginIndex = 0; pluginIndex < group.Plugins.Count; pluginIndex++)
                    {
                        FomodInstallerPluginModel plugin = group.Plugins[pluginIndex];
                        string marker = plugin.IsSelected ? "[x]" : "[ ]";
                        string required = plugin.IsRequired ? " (required)" : string.Empty;
                        Console.WriteLine($"  {pluginIndex + 1}. {marker} {plugin.Name}{required}");
                        if (!string.IsNullOrWhiteSpace(plugin.Description))
                        {
                            Console.WriteLine($"     {plugin.Description}");
                        }
                    }

                    if (group.GroupType == FomodGroupType.SelectExactlyOne
                        || group.GroupType == FomodGroupType.SelectAtMostOne)
                    {
                        Console.Write($"Select option number for '{group.Name}' (0 to clear): ");
                        if (!TryReadSelection(group.Plugins.Count, out int selection))
                        {
                            return null;
                        }

                        for (int pluginIndex = 0; pluginIndex < group.Plugins.Count; pluginIndex++)
                        {
                            bool selected = selection == pluginIndex + 1;
                            FomodInstallerPresenter.TrySetPluginSelected(session, stepIndex, groupIndex, pluginIndex, selected);
                        }
                    }
                    else
                    {
                        for (int pluginIndex = 0; pluginIndex < group.Plugins.Count; pluginIndex++)
                        {
                            FomodInstallerPluginModel plugin = group.Plugins[pluginIndex];
                            if (plugin.IsRequired)
                            {
                                FomodInstallerPresenter.TrySetPluginSelected(session, stepIndex, groupIndex, pluginIndex, true);
                                continue;
                            }

                            Console.Write($"Include '{plugin.Name}'? [y/N]: ");
                            string response = Console.ReadLine()?.Trim();
                            bool selected = string.Equals(response, "y", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(response, "yes", StringComparison.OrdinalIgnoreCase);
                            FomodInstallerPresenter.TrySetPluginSelected(session, stepIndex, groupIndex, pluginIndex, selected);
                        }
                    }
                }

                visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(session);
                string validationMessage = FomodInstallerPresenter.ValidateStep(session, stepIndex);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    Console.WriteLine();
                    Console.WriteLine("Validation: " + validationMessage);
                    continue;
                }

                if (visibleCursor < visibleStepIndices.Count - 1)
                {
                    Console.WriteLine();
                    Console.Write("Press Enter for next step, or type 'back' to revise: ");
                    string nav = Console.ReadLine()?.Trim();
                    if (string.Equals(nav, "back", StringComparison.OrdinalIgnoreCase))
                    {
                        if (visibleCursor > 0)
                        {
                            visibleCursor--;
                        }

                        visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(session);
                        continue;
                    }
                }

                visibleCursor++;
                visibleStepIndices = FomodInstallerPresenter.GetVisibleStepIndices(session);
            }

            FomodInstallerPresenter.ApplySelectionsToComponent(session);
            return component;
        }

        private static bool TryReadSelection(int maxOption, out int selection)
        {
            selection = 0;
            string line = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            if (!int.TryParse(line, out int value) || value < 0 || value > maxOption)
            {
                Console.Error.WriteLine("Invalid selection.");
                return false;
            }

            selection = value;
            return true;
        }

        [NotNull]
        private static string DescribeGroupType(FomodGroupType groupType)
        {
            switch (groupType)
            {
                case FomodGroupType.SelectExactlyOne:
                    return "pick one";
                case FomodGroupType.SelectAtMostOne:
                    return "pick at most one";
                case FomodGroupType.SelectAtLeastOne:
                    return "pick at least one";
                case FomodGroupType.SelectAll:
                    return "all required";
                default:
                    return "optional";
            }
        }
    }
}
