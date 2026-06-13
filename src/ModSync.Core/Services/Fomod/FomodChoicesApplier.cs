// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodChoicesApplier
    {
        [NotNull]
        public static ModComponent ApplyChoices(
            [NotNull] string extractedArchiveDirectory,
            [NotNull] FomodArchiveChoices archiveChoices)
        {
            string moduleConfigPath = FomodArchiveDiscovery.FindModuleConfigPath(extractedArchiveDirectory);
            if (moduleConfigPath is null)
            {
                throw new InvalidOperationException("No fomod/ModuleConfig.xml found in extracted archive.");
            }

            FomodModuleConfig config = FomodParser.ParseModuleConfigXmlFile(moduleConfigPath);
            FomodInfo info = null;
            string infoPath = FomodArchiveDiscovery.FindInfoPath(extractedArchiveDirectory);
            if (infoPath != null)
            {
                info = FomodParser.ParseInfoXmlFile(infoPath);
            }

            string archiveFileName = archiveChoices.ArchiveFileName
                ?? Path.GetFileName(extractedArchiveDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) + ".zip";

            ModComponent component = FomodToComponentMapper.Map(info, config, archiveFileName);
            FomodInstallerSession session = FomodInstallerPresenter.CreateSession(config, component);

            foreach (FomodGroupSelection selection in archiveChoices.Selections ?? Enumerable.Empty<FomodGroupSelection>())
            {
                int stepIndex = FindStepIndex(session, selection.StepName);
                int groupIndex = FindGroupIndex(session, stepIndex, selection.GroupName);
                FomodInstallerGroupModel group = session.Steps[stepIndex].Groups[groupIndex];

                var selectedNames = new HashSet<string>(
                    selection.Plugins ?? new List<string>(),
                    StringComparer.OrdinalIgnoreCase);

                for (int pluginIndex = 0; pluginIndex < group.Plugins.Count; pluginIndex++)
                {
                    FomodInstallerPluginModel plugin = group.Plugins[pluginIndex];
                    bool shouldSelect = selectedNames.Contains(plugin.Name);
                    if (!FomodInstallerPresenter.TrySetPluginSelected(session, stepIndex, groupIndex, pluginIndex, shouldSelect))
                    {
                        throw new InvalidOperationException(
                            $"Cannot apply selection for required plugin '{plugin.Name}' in group '{group.Name}'.");
                    }
                }

                string validation = FomodInstallerPresenter.ValidateStep(session, stepIndex);
                if (!string.IsNullOrEmpty(validation))
                {
                    throw new InvalidOperationException(validation);
                }
            }

            FomodInstallerPresenter.ApplySelectionsToComponent(session);
            return component;
        }

        [NotNull]
        public static FomodChoicesFile LoadFromFile([NotNull] string path)
        {
            string json = File.ReadAllText(path);
            FomodChoicesFile choices = JsonConvert.DeserializeObject<FomodChoicesFile>(json);
            if (choices is null)
            {
                throw new InvalidOperationException($"FOMOD choices file '{path}' is empty or invalid.");
            }

            return choices;
        }

        [CanBeNull]
        public static FomodArchiveChoices FindArchiveChoices(
            [NotNull] FomodChoicesFile choicesFile,
            [NotNull] string archiveFileName)
        {
            return choicesFile.Archives?.FirstOrDefault(
                archive => string.Equals(archive.ArchiveFileName, archiveFileName, StringComparison.OrdinalIgnoreCase));
        }

        private static int FindStepIndex([NotNull] FomodInstallerSession session, [CanBeNull] string stepName)
        {
            for (int index = 0; index < session.Steps.Count; index++)
            {
                if (string.Equals(session.Steps[index].Name, stepName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"FOMOD step '{stepName}' was not found.");
        }

        private static int FindGroupIndex(
            [NotNull] FomodInstallerSession session,
            int stepIndex,
            [CanBeNull] string groupName)
        {
            IReadOnlyList<FomodInstallerGroupModel> groups = session.Steps[stepIndex].Groups;
            for (int index = 0; index < groups.Count; index++)
            {
                if (string.Equals(groups[index].Name, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"FOMOD group '{groupName}' was not found in step '{session.Steps[stepIndex].Name}'.");
        }
    }
}
