// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Selection semantics of an optionalFileGroups group element.
    /// </summary>
    public enum FomodGroupType
    {
        SelectAny,
        SelectExactlyOne,
        SelectAtMostOne,
        SelectAtLeastOne,
        SelectAll,
    }

    /// <summary>
    /// Plugin type from a plugin's typeDescriptor element (default type when a
    /// dependencyType wrapper is used).
    /// </summary>
    public enum FomodPluginType
    {
        Optional,
        Required,
        Recommended,
        NotUsable,
        CouldBeUsable,
    }

    /// <summary>
    /// Operator of a composite dependencies element.
    /// </summary>
    public enum FomodDependencyOperator
    {
        And,
        Or,
    }

    /// <summary>
    /// Discriminates the dependency node kinds found inside a dependencies element.
    /// </summary>
    public enum FomodDependencyType
    {
        /// <summary>A nested dependencies element combining children with <see cref="FomodDependency.Operator"/>.</summary>
        Composite,

        /// <summary>flagDependency: requires a condition flag to have a specific value.</summary>
        Flag,

        /// <summary>fileDependency: requires a game file to be in a given state (Active/Inactive/Missing).</summary>
        File,

        /// <summary>gameDependency: requires a minimum game version.</summary>
        Game,
    }

    /// <summary>
    /// A single file or folder install directive (file/folder elements inside
    /// requiredInstallFiles, plugin files, or conditionalFileInstalls patterns).
    /// Source and Destination are archive-relative paths exactly as written in the XML.
    /// </summary>
    public sealed class FomodFileInstall
    {
        [NotNull] public string Source { get; set; } = string.Empty;
        [NotNull] public string Destination { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsFolder { get; set; }
    }

    /// <summary>
    /// A condition flag set by a plugin when it is selected (flag element inside conditionFlags).
    /// </summary>
    public sealed class FomodConditionFlag
    {
        [NotNull] public string Name { get; set; } = string.Empty;
        [NotNull] public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// A dependency tree node. Composite nodes combine <see cref="Children"/> with
    /// <see cref="Operator"/>; leaf nodes are flag, file, or game dependencies.
    /// </summary>
    public sealed class FomodDependency
    {
        public FomodDependencyType Type { get; set; } = FomodDependencyType.Composite;
        public FomodDependencyOperator Operator { get; set; } = FomodDependencyOperator.And;
        [NotNull][ItemNotNull] public List<FomodDependency> Children { get; set; } = new List<FomodDependency>();

        /// <summary>Flag name when <see cref="Type"/> is <see cref="FomodDependencyType.Flag"/>.</summary>
        [NotNull] public string FlagName { get; set; } = string.Empty;

        /// <summary>Required flag value when <see cref="Type"/> is <see cref="FomodDependencyType.Flag"/>.</summary>
        [NotNull] public string FlagValue { get; set; } = string.Empty;

        /// <summary>File path when <see cref="Type"/> is <see cref="FomodDependencyType.File"/>.</summary>
        [NotNull] public string FilePath { get; set; } = string.Empty;

        /// <summary>Required file state (Active/Inactive/Missing) when <see cref="Type"/> is <see cref="FomodDependencyType.File"/>.</summary>
        [NotNull] public string FileState { get; set; } = string.Empty;

        /// <summary>Minimum game version when <see cref="Type"/> is <see cref="FomodDependencyType.Game"/>.</summary>
        [NotNull] public string GameVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// A selectable plugin inside a group.
    /// </summary>
    public sealed class FomodPlugin
    {
        [NotNull] public string Name { get; set; } = string.Empty;
        [NotNull] public string Description { get; set; } = string.Empty;
        [NotNull] public string ImagePath { get; set; } = string.Empty;
        [NotNull][ItemNotNull] public List<FomodFileInstall> Files { get; set; } = new List<FomodFileInstall>();
        [NotNull][ItemNotNull] public List<FomodConditionFlag> ConditionFlags { get; set; } = new List<FomodConditionFlag>();
        public FomodPluginType TypeDescriptor { get; set; } = FomodPluginType.Optional;
    }

    /// <summary>
    /// A group of plugins with shared selection semantics.
    /// </summary>
    public sealed class FomodGroup
    {
        [NotNull] public string Name { get; set; } = string.Empty;
        public FomodGroupType Type { get; set; } = FomodGroupType.SelectAny;
        [NotNull][ItemNotNull] public List<FomodPlugin> Plugins { get; set; } = new List<FomodPlugin>();
    }

    /// <summary>
    /// One page of the FOMOD wizard (installStep element).
    /// </summary>
    public sealed class FomodInstallStep
    {
        [NotNull] public string Name { get; set; } = string.Empty;

        /// <summary>Visibility condition (visible element); null when the step is always shown.</summary>
        [CanBeNull] public FomodDependency Visible { get; set; }

        [NotNull][ItemNotNull] public List<FomodGroup> Groups { get; set; } = new List<FomodGroup>();
    }

    /// <summary>
    /// A conditionalFileInstalls pattern: install <see cref="Files"/> when <see cref="Dependencies"/> evaluates true.
    /// </summary>
    public sealed class FomodConditionalInstallPattern
    {
        [NotNull] public FomodDependency Dependencies { get; set; } = new FomodDependency();
        [NotNull][ItemNotNull] public List<FomodFileInstall> Files { get; set; } = new List<FomodFileInstall>();
    }

    /// <summary>
    /// Root model for <c>fomod/ModuleConfig.xml</c>.
    /// </summary>
    public sealed class FomodModuleConfig
    {
        [NotNull] public string ModuleName { get; set; } = string.Empty;
        [NotNull][ItemNotNull] public List<FomodFileInstall> RequiredInstallFiles { get; set; } = new List<FomodFileInstall>();
        [NotNull][ItemNotNull] public List<FomodInstallStep> InstallSteps { get; set; } = new List<FomodInstallStep>();
        [NotNull][ItemNotNull] public List<FomodConditionalInstallPattern> ConditionalInstallPatterns { get; set; } = new List<FomodConditionalInstallPattern>();
    }
}
