// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Headless FOMOD installer wizard state: maps parsed config to component options,
    /// tracks selections, evaluates step visibility, and applies choices back to the component.
    /// </summary>
    public static class FomodInstallerPresenter
    {
        [NotNull]
        public static FomodInstallerSession CreateSession(
            [NotNull] FomodModuleConfig config,
            [NotNull] ModComponent component)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var optionFlags = new Dictionary<Guid, List<FomodConditionFlag>>();
            var steps = new List<FomodInstallerStepModel>();

            foreach (FomodInstallStep installStep in config.InstallSteps)
            {
                var groupModels = new List<FomodInstallerGroupModel>();
                foreach (FomodGroup group in installStep.Groups)
                {
                    string heading = BuildHeading(installStep, group);
                    var pluginModels = new List<FomodInstallerPluginModel>();
                    foreach (FomodPlugin plugin in group.Plugins)
                    {
                        Option option = component.Options.FirstOrDefault(
                            candidate => candidate.Name == plugin.Name && candidate.Heading == heading);
                        if (option is null)
                        {
                            throw new InvalidOperationException(
                                $"Mapped component is missing option '{plugin.Name}' for heading '{heading}'.");
                        }

                        optionFlags[option.Guid] = plugin.ConditionFlags;
                        pluginModels.Add(
                            new FomodInstallerPluginModel(
                                option.Guid,
                                plugin.Name,
                                plugin.Description,
                                option.IsSelected,
                                plugin.TypeDescriptor == FomodPluginType.Required));
                    }

                    groupModels.Add(new FomodInstallerGroupModel(group.Name, group.Type, pluginModels));
                }

                steps.Add(new FomodInstallerStepModel(installStep.Name, installStep.Visible, groupModels));
            }

            return new FomodInstallerSession(config, component, steps, optionFlags);
        }

        [NotNull]
        public static IReadOnlyDictionary<string, string> BuildFlagValues([NotNull] FomodInstallerSession session)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var flagValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (FomodInstallerStepModel step in session.Steps)
            {
                foreach (FomodInstallerGroupModel group in step.Groups)
                {
                    foreach (FomodInstallerPluginModel plugin in group.Plugins.Where(plugin => plugin.IsSelected))
                    {
                        if (!session.OptionFlags.TryGetValue(plugin.OptionGuid, out List<FomodConditionFlag> flags))
                        {
                            continue;
                        }

                        foreach (FomodConditionFlag flag in flags)
                        {
                            flagValues[flag.Name] = flag.Value;
                        }
                    }
                }
            }

            return flagValues;
        }

        public static bool IsStepVisible([NotNull] FomodInstallerStepModel step, [NotNull] IReadOnlyDictionary<string, string> flagValues)
        {
            if (step is null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            if (flagValues is null)
            {
                throw new ArgumentNullException(nameof(flagValues));
            }

            return step.Visible is null || FomodToComponentMapper.EvaluateDependency(step.Visible, flagValues);
        }

        [NotNull]
        public static IReadOnlyList<int> GetVisibleStepIndices([NotNull] FomodInstallerSession session)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            IReadOnlyDictionary<string, string> flagValues = BuildFlagValues(session);
            var indices = new List<int>();
            for (int index = 0; index < session.Steps.Count; index++)
            {
                if (IsStepVisible(session.Steps[index], flagValues))
                {
                    indices.Add(index);
                }
            }

            return indices;
        }

        [CanBeNull]
        public static string ValidateStep([NotNull] FomodInstallerSession session, int stepIndex)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (stepIndex < 0 || stepIndex >= session.Steps.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(stepIndex));
            }

            FomodInstallerStepModel step = session.Steps[stepIndex];
            foreach (FomodInstallerGroupModel group in step.Groups)
            {
                int selectedCount = group.Plugins.Count(plugin => plugin.IsSelected);
                switch (group.GroupType)
                {
                    case FomodGroupType.SelectExactlyOne:
                        if (selectedCount != 1)
                        {
                            return $"Select exactly one option in '{group.Name}'.";
                        }

                        break;
                    case FomodGroupType.SelectAtMostOne:
                        if (selectedCount > 1)
                        {
                            return $"Select at most one option in '{group.Name}'.";
                        }

                        break;
                    case FomodGroupType.SelectAtLeastOne:
                        if (selectedCount < 1)
                        {
                            return $"Select at least one option in '{group.Name}'.";
                        }

                        break;
                    case FomodGroupType.SelectAll:
                        if (selectedCount != group.Plugins.Count)
                        {
                            return $"All options in '{group.Name}' must be selected.";
                        }

                        break;
                    case FomodGroupType.SelectAny:
                    default:
                        break;
                }
            }

            return null;
        }

        public static bool TrySetPluginSelected(
            [NotNull] FomodInstallerSession session,
            int stepIndex,
            int groupIndex,
            int pluginIndex,
            bool isSelected)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            FomodInstallerPluginModel plugin = session.Steps[stepIndex].Groups[groupIndex].Plugins[pluginIndex];
            if (plugin.IsRequired && !isSelected)
            {
                return false;
            }

            FomodInstallerGroupModel group = session.Steps[stepIndex].Groups[groupIndex];
            if (isSelected && (group.GroupType == FomodGroupType.SelectExactlyOne || group.GroupType == FomodGroupType.SelectAtMostOne))
            {
                for (int index = 0; index < group.Plugins.Count; index++)
                {
                    group.Plugins[index].IsSelected = index == pluginIndex;
                }

                return true;
            }

            plugin.IsSelected = isSelected;
            return true;
        }

        public static void ApplySelectionsToComponent([NotNull] FomodInstallerSession session)
        {
            if (session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            foreach (Option option in session.Component.Options)
            {
                option.IsSelected = false;
            }

            IReadOnlyList<int> visibleStepIndices = GetVisibleStepIndices(session);
            foreach (int stepIndex in visibleStepIndices)
            {
                FomodInstallerStepModel step = session.Steps[stepIndex];
                foreach (FomodInstallerGroupModel group in step.Groups)
                {
                    foreach (FomodInstallerPluginModel plugin in group.Plugins.Where(plugin => plugin.IsSelected))
                    {
                        Option option = session.Component.Options.First(candidate => candidate.Guid == plugin.OptionGuid);
                        option.IsSelected = true;
                    }
                }
            }
        }

        [NotNull]
        private static string BuildHeading([NotNull] FomodInstallStep step, [NotNull] FomodGroup group)
        {
            if (string.IsNullOrWhiteSpace(step.Name))
            {
                return group.Name;
            }

            return string.IsNullOrWhiteSpace(group.Name) ? step.Name : step.Name + " / " + group.Name;
        }
    }

    public sealed class FomodInstallerSession
    {
        [NotNull]
        public FomodModuleConfig Config { get; }

        [NotNull]
        public ModComponent Component { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<FomodInstallerStepModel> Steps { get; }

        [NotNull]
        public Dictionary<Guid, List<FomodConditionFlag>> OptionFlags { get; }

        public FomodInstallerSession(
            [NotNull] FomodModuleConfig config,
            [NotNull] ModComponent component,
            [NotNull][ItemNotNull] IReadOnlyList<FomodInstallerStepModel> steps,
            [NotNull] Dictionary<Guid, List<FomodConditionFlag>> optionFlags)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            OptionFlags = optionFlags ?? throw new ArgumentNullException(nameof(optionFlags));
        }
    }

    public sealed class FomodInstallerStepModel
    {
        [NotNull]
        public string Name { get; }

        [CanBeNull]
        public FomodDependency Visible { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<FomodInstallerGroupModel> Groups { get; }

        public FomodInstallerStepModel(
            [NotNull] string name,
            [CanBeNull] FomodDependency visible,
            [NotNull][ItemNotNull] IReadOnlyList<FomodInstallerGroupModel> groups)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Visible = visible;
            Groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }
    }

    public sealed class FomodInstallerGroupModel
    {
        [NotNull]
        public string Name { get; }

        public FomodGroupType GroupType { get; }

        [NotNull]
        [ItemNotNull]
        public List<FomodInstallerPluginModel> Plugins { get; }

        public FomodInstallerGroupModel(
            [NotNull] string name,
            FomodGroupType groupType,
            [NotNull][ItemNotNull] List<FomodInstallerPluginModel> plugins)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GroupType = groupType;
            Plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        }
    }

    public sealed class FomodInstallerPluginModel
    {
        public Guid OptionGuid { get; }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string Description { get; }

        public bool IsSelected { get; set; }

        public bool IsRequired { get; }

        public FomodInstallerPluginModel(
            Guid optionGuid,
            [NotNull] string name,
            [NotNull] string description,
            bool isSelected,
            bool isRequired)
        {
            OptionGuid = optionGuid;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            IsSelected = isSelected;
            IsRequired = isRequired;
        }
    }
}
