// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using ModSync.Core.Utility;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Translates a parsed FOMOD package into ModSync's native
    /// <see cref="ModComponent"/>/<see cref="Option"/> model so the existing
    /// instruction pipeline (including <see cref="Instruction.GetChosenOptions"/>)
    /// can execute it.
    ///
    /// Mapping rules:
    /// <list type="bullet">
    /// <item>requiredInstallFiles become Copy instructions directly on the component.</item>
    /// <item>Each installStep group becomes one <see cref="Option"/> per plugin. The
    /// group selection semantics are recorded in the Option's existing fields:
    /// <c>InstallationMethod</c> holds the <see cref="FomodGroupType"/> name and
    /// <c>Heading</c> holds "step / group" names. No new model fields are added.</item>
    /// <item>Each group also produces one Choose instruction on the component whose
    /// Source lists the GUIDs of the group's options; this is the only instruction
    /// kind whose Source is not placeholder-prefixed, matching the repo rule that
    /// all actions except Choose use path placeholders.</item>
    /// <item>Per-plugin file installs become Copy instructions inside the Option with
    /// sources under <c>&lt;&lt;modDirectory&gt;&gt;/&lt;archive-folder&gt;/...</c> and
    /// destinations under <c>&lt;&lt;kotorDirectory&gt;&gt;/...</c>. Folder installs map to a
    /// wildcard Copy of the folder's contents.</item>
    /// </list>
    ///
    /// Conditional installs (conditionalFileInstalls) and flag dependencies:
    /// flags are set by selected plugins, so a pattern whose flag dependencies are
    /// all satisfied by a single plugin's conditionFlags is appended to that
    /// plugin's Option (the files install if and only if that option is chosen).
    /// gameDependency and fileDependency nodes are not evaluated in this slice:
    /// they are treated as always-true and a warning is logged. Patterns that
    /// cannot be attributed to a single plugin (Or operators, nested composites,
    /// or flags spanning multiple plugins) are skipped with a logged warning.
    /// </summary>
    public static class FomodToComponentMapper
    {
        private const string ModDirectoryPlaceholder = "<<modDirectory>>";
        private const string KotorDirectoryPlaceholder = "<<kotorDirectory>>";

        /// <summary>
        /// Maps a parsed FOMOD into a <see cref="ModComponent"/>.
        /// </summary>
        /// <param name="info">Optional metadata from fomod/info.xml; may be null.</param>
        /// <param name="config">Parsed fomod/ModuleConfig.xml; required.</param>
        /// <param name="archiveFileName">
        /// File name of the mod archive (for example <c>MyMod-1.0.zip</c>). The archive
        /// is expected to be extracted to a folder of the same name (without the archive
        /// extension) inside the mod directory; all generated sources start with
        /// <c>&lt;&lt;modDirectory&gt;&gt;/&lt;that folder&gt;/</c>.
        /// </param>
        [NotNull]
        public static ModComponent Map([CanBeNull] FomodInfo info, [NotNull] FomodModuleConfig config, [NotNull] string archiveFileName)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(archiveFileName))
            {
                throw new ArgumentException("Archive file name is required.", nameof(archiveFileName));
            }

            string archiveFolder = GetArchiveFolderName(archiveFileName);
            string componentName = info != null && !string.IsNullOrWhiteSpace(info.Name)
                ? info.Name
                : (!string.IsNullOrWhiteSpace(config.ModuleName) ? config.ModuleName : archiveFolder);

            var component = new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = componentName,
                Author = info is null ? string.Empty : info.Author,
                Description = info is null ? string.Empty : info.Description,
                InstallationMethod = "FOMOD Installer",
            };

            foreach (FomodFileInstall fileInstall in config.RequiredInstallFiles)
            {
                component.Instructions.Add(CreateCopyInstruction(fileInstall, archiveFolder, component));
            }

            var pluginOptions = new List<KeyValuePair<FomodPlugin, Option>>();
            foreach (FomodInstallStep step in config.InstallSteps)
            {
                foreach (FomodGroup group in step.Groups)
                {
                    var optionGuids = new List<string>();
                    var groupOptions = new List<Option>();
                    foreach (FomodPlugin plugin in group.Plugins)
                    {
                        Option option = CreateOption(plugin, group, step, archiveFolder);
                        component.Options.Add(option);
                        groupOptions.Add(option);
                        optionGuids.Add(option.Guid.ToString());
                        pluginOptions.Add(new KeyValuePair<FomodPlugin, Option>(plugin, option));
                    }

                    ApplyGroupSelectionDefaults(group, groupOptions);

                    if (optionGuids.Count > 0)
                    {
                        var chooseInstruction = new Instruction
                        {
                            Action = Instruction.ActionType.Choose,
                            Source = optionGuids,
                        };
                        chooseInstruction.SetParentComponent(component);
                        component.Instructions.Add(chooseInstruction);
                    }
                }
            }

            ApplyConditionalInstalls(config, pluginOptions, component, archiveFolder);
            return component;
        }

        /// <summary>
        /// Evaluates a dependency tree against a set of flag values (flag name to value).
        /// Flag dependencies compare values ordinally (flag names case-insensitively).
        /// gameDependency and fileDependency are treated as always-true with a logged
        /// warning because ModSync does not track game version or plugin file state.
        /// </summary>
        public static bool EvaluateDependency([CanBeNull] FomodDependency dependency, [NotNull] IReadOnlyDictionary<string, string> flagValues)
        {
            if (flagValues is null)
            {
                throw new ArgumentNullException(nameof(flagValues));
            }

            if (dependency is null)
            {
                return true;
            }

            switch (dependency.Type)
            {
                case FomodDependencyType.Flag:
                    foreach (KeyValuePair<string, string> flag in flagValues)
                    {
                        if (string.Equals(flag.Key, dependency.FlagName, StringComparison.OrdinalIgnoreCase))
                        {
                            return string.Equals(flag.Value, dependency.FlagValue, StringComparison.Ordinal);
                        }
                    }

                    return false;
                case FomodDependencyType.File:
                    Logger.LogWarning(
                        $"[FomodToComponentMapper] fileDependency '{dependency.FilePath}' is not evaluated; treating as satisfied."
                    );
                    return true;
                case FomodDependencyType.Game:
                    Logger.LogWarning(
                        $"[FomodToComponentMapper] gameDependency '{dependency.GameVersion}' is not evaluated; treating as satisfied."
                    );
                    return true;
                case FomodDependencyType.Composite:
                default:
                    if (dependency.Children.Count == 0)
                    {
                        return true;
                    }

                    if (dependency.Operator == FomodDependencyOperator.Or)
                    {
                        return dependency.Children.Any(child => EvaluateDependency(child, flagValues));
                    }

                    return dependency.Children.All(child => EvaluateDependency(child, flagValues));
            }
        }

        [NotNull]
        private static Option CreateOption(
            [NotNull] FomodPlugin plugin,
            [NotNull] FomodGroup group,
            [NotNull] FomodInstallStep step,
            [NotNull] string archiveFolder
        )
        {
            string heading = string.IsNullOrWhiteSpace(step.Name)
                ? group.Name
                : (string.IsNullOrWhiteSpace(group.Name) ? step.Name : step.Name + " / " + group.Name);
            var option = new Option
            {
                Guid = Guid.NewGuid(),
                Name = plugin.Name,
                Description = plugin.Description,
                Heading = heading,
                InstallationMethod = group.Type.ToString(),
                IsSelected = plugin.TypeDescriptor == FomodPluginType.Required
                    || plugin.TypeDescriptor == FomodPluginType.Recommended,
            };

            foreach (FomodFileInstall fileInstall in plugin.Files)
            {
                option.Instructions.Add(CreateCopyInstruction(fileInstall, archiveFolder, option));
            }

            return option;
        }

        /// <summary>
        /// Ensures group selection semantics produce a sane default: SelectAll selects
        /// everything; SelectExactlyOne and SelectAtLeastOne select the first plugin
        /// when nothing is preselected by a Required/Recommended type descriptor.
        /// </summary>
        private static void ApplyGroupSelectionDefaults([NotNull] FomodGroup group, [NotNull][ItemNotNull] List<Option> groupOptions)
        {
            if (groupOptions.Count == 0)
            {
                return;
            }

            if (group.Type == FomodGroupType.SelectAll)
            {
                foreach (Option option in groupOptions)
                {
                    option.IsSelected = true;
                }

                return;
            }

            bool anySelected = groupOptions.Any(option => option.IsSelected);
            if (!anySelected && (group.Type == FomodGroupType.SelectExactlyOne || group.Type == FomodGroupType.SelectAtLeastOne))
            {
                groupOptions[0].IsSelected = true;
            }
        }

        private static void ApplyConditionalInstalls(
            [NotNull] FomodModuleConfig config,
            [NotNull] List<KeyValuePair<FomodPlugin, Option>> pluginOptions,
            [NotNull] ModComponent component,
            [NotNull] string archiveFolder
        )
        {
            foreach (FomodConditionalInstallPattern pattern in config.ConditionalInstallPatterns)
            {
                List<FomodDependency> flagDependencies = CollectSimpleFlagDependencies(pattern.Dependencies, out bool isSupported);
                if (!isSupported)
                {
                    Logger.LogWarning(
                        "[FomodToComponentMapper] Skipping a conditionalFileInstalls pattern: only a flat And of flagDependency elements is supported in this slice."
                    );
                    continue;
                }

                if (flagDependencies.Count == 0)
                {
                    // Only always-true dependencies (game/file) or none at all: install unconditionally.
                    foreach (FomodFileInstall fileInstall in pattern.Files)
                    {
                        component.Instructions.Add(CreateCopyInstruction(fileInstall, archiveFolder, component));
                    }

                    continue;
                }

                Option matchingOption = FindOptionSatisfyingFlags(pluginOptions, flagDependencies);
                if (matchingOption is null)
                {
                    Logger.LogWarning(
                        "[FomodToComponentMapper] Skipping a conditionalFileInstalls pattern: its flag dependencies are not all set by a single plugin."
                    );
                    continue;
                }

                foreach (FomodFileInstall fileInstall in pattern.Files)
                {
                    matchingOption.Instructions.Add(CreateCopyInstruction(fileInstall, archiveFolder, matchingOption));
                }
            }
        }

        /// <summary>
        /// Flattens a pattern's dependency tree into a list of flag dependencies for the
        /// simple supported case: a top-level And composite containing only flagDependency,
        /// fileDependency, and gameDependency leaves. File and game dependencies are
        /// treated as always-true (warning logged); Or composites and nested composites
        /// mark the pattern unsupported.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        private static List<FomodDependency> CollectSimpleFlagDependencies([CanBeNull] FomodDependency dependencies, out bool isSupported)
        {
            isSupported = true;
            var flagDependencies = new List<FomodDependency>();
            if (dependencies is null)
            {
                return flagDependencies;
            }

            if (dependencies.Type == FomodDependencyType.Flag)
            {
                flagDependencies.Add(dependencies);
                return flagDependencies;
            }

            if (dependencies.Type != FomodDependencyType.Composite)
            {
                LogIgnoredDependency(dependencies);
                return flagDependencies;
            }

            if (dependencies.Operator == FomodDependencyOperator.Or && dependencies.Children.Count > 1)
            {
                isSupported = false;
                return flagDependencies;
            }

            foreach (FomodDependency child in dependencies.Children)
            {
                if (child.Type == FomodDependencyType.Flag)
                {
                    flagDependencies.Add(child);
                }
                else if (child.Type == FomodDependencyType.Composite)
                {
                    isSupported = false;
                    return flagDependencies;
                }
                else
                {
                    LogIgnoredDependency(child);
                }
            }

            return flagDependencies;
        }

        private static void LogIgnoredDependency([NotNull] FomodDependency dependency)
        {
            string label = dependency.Type == FomodDependencyType.File
                ? $"fileDependency '{dependency.FilePath}'"
                : $"gameDependency '{dependency.GameVersion}'";
            Logger.LogWarning(
                $"[FomodToComponentMapper] {label} is not evaluated; treating as satisfied."
            );
        }

        [CanBeNull]
        private static Option FindOptionSatisfyingFlags(
            [NotNull] List<KeyValuePair<FomodPlugin, Option>> pluginOptions,
            [NotNull][ItemNotNull] List<FomodDependency> flagDependencies
        )
        {
            foreach (KeyValuePair<FomodPlugin, Option> pair in pluginOptions)
            {
                bool satisfiesAll = true;
                foreach (FomodDependency flagDependency in flagDependencies)
                {
                    bool found = pair.Key.ConditionFlags.Any(
                        flag => string.Equals(flag.Name, flagDependency.FlagName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(flag.Value, flagDependency.FlagValue, StringComparison.Ordinal)
                    );
                    if (!found)
                    {
                        satisfiesAll = false;
                        break;
                    }
                }

                if (satisfiesAll)
                {
                    return pair.Value;
                }
            }

            return null;
        }

        [NotNull]
        private static Instruction CreateCopyInstruction(
            [NotNull] FomodFileInstall fileInstall,
            [NotNull] string archiveFolder,
            [NotNull] ModComponent parent
        )
        {
            string relativeSource = NormalizeRelativePath(fileInstall.Source);
            string source = ModDirectoryPlaceholder + "/" + archiveFolder
                + (relativeSource.Length == 0 ? string.Empty : "/" + relativeSource);
            if (fileInstall.IsFolder)
            {
                source += "/*";
            }

            var instruction = new Instruction
            {
                Action = Instruction.ActionType.Copy,
                Source = new List<string> { source },
                Destination = BuildDestination(fileInstall),
                Overwrite = true,
            };
            instruction.SetParentComponent(parent);
            return instruction;
        }

        /// <summary>
        /// Maps a FOMOD destination (game-folder-relative) to a sandboxed
        /// <c>&lt;&lt;kotorDirectory&gt;&gt;</c> destination directory. For file installs the
        /// file-name part is dropped because ModSync's Copy action preserves the source
        /// file name; a warning is logged when the FOMOD intended a rename.
        /// </summary>
        [NotNull]
        private static string BuildDestination([NotNull] FomodFileInstall fileInstall)
        {
            string relativeDestination = NormalizeRelativePath(fileInstall.Destination);
            if (!fileInstall.IsFolder && relativeDestination.Length > 0)
            {
                int lastSlash = relativeDestination.LastIndexOf('/');
                string destinationFileName = lastSlash >= 0 ? relativeDestination.Substring(lastSlash + 1) : relativeDestination;
                string sourceFileName = Path.GetFileName(NormalizeRelativePath(fileInstall.Source));
                if (destinationFileName.IndexOf('.') >= 0 || string.Equals(destinationFileName, sourceFileName, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(destinationFileName, sourceFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.LogWarning(
                            $"[FomodToComponentMapper] FOMOD wants to install '{sourceFileName}' as '{destinationFileName}'; renames are not supported by the Copy action, the source file name is kept."
                        );
                    }

                    relativeDestination = lastSlash >= 0 ? relativeDestination.Substring(0, lastSlash) : string.Empty;
                }
            }

            return relativeDestination.Length == 0
                ? KotorDirectoryPlaceholder
                : KotorDirectoryPlaceholder + "/" + relativeDestination;
        }

        /// <summary>
        /// Normalizes a FOMOD-relative path: forward slashes, no leading/trailing
        /// slashes. Rooted paths and parent-directory traversal are rejected so the
        /// generated instructions always stay inside the placeholder sandbox.
        /// </summary>
        [NotNull]
        private static string NormalizeRelativePath([CanBeNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalized = path.Replace('\\', '/').Trim().Trim('/');
            string[] segments = normalized.Split('/');
            var kept = new List<string>();
            foreach (string segment in segments)
            {
                if (segment.Length == 0 || string.Equals(segment, ".", StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(segment, "..", StringComparison.Ordinal) || segment.IndexOf(':') >= 0)
                {
                    throw new FormatException(
                        $"FOMOD path '{path}' escapes the archive sandbox; rooted paths and '..' segments are not allowed."
                    );
                }

                kept.Add(segment);
            }

            return string.Join("/", kept);
        }

        /// <summary>
        /// Derives the extraction folder name from the archive file name by stripping a
        /// known archive extension (zip, 7z, rar, exe); other names are used verbatim.
        /// </summary>
        [NotNull]
        private static string GetArchiveFolderName([NotNull] string archiveFileName)
        {
            string fileName = Path.GetFileName(archiveFileName.Trim());
            string extension = Path.GetExtension(fileName);
            if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".7z", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".rar", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileNameWithoutExtension(fileName);
            }

            return fileName;
        }
    }
}
