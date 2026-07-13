// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Merges FOMOD wizard output into an existing mod component from the instruction file.
    /// </summary>
    public static class FomodConfiguredComponentMerger
    {
        private const string ModDirectoryPlaceholder = "<<modDirectory>>";

        public static void MergeInto(
            [NotNull] ModComponent target,
            [NotNull] ModComponent configured,
            [NotNull] string archiveFileName)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (configured is null)
            {
                throw new ArgumentNullException(nameof(configured));
            }

            if (string.IsNullOrWhiteSpace(archiveFileName))
            {
                throw new ArgumentException("Archive file name cannot be null or whitespace.", nameof(archiveFileName));
            }

            string archiveFolder = Path.GetFileNameWithoutExtension(archiveFileName);
            string archivePathPrefix = ModDirectoryPlaceholder + "/" + archiveFolder + "/";

            var configuredOptionGuids = new HashSet<Guid>(configured.Options.Select(option => option.Guid));
            RemovePriorFomodInstructions(target, archivePathPrefix, configuredOptionGuids);

            foreach (Option configuredOption in configured.Options)
            {
                Option existing = target.Options.FirstOrDefault(option => option.Guid == configuredOption.Guid);
                if (existing is null)
                {
                    target.Options.Add(configuredOption);
                    continue;
                }

                existing.IsSelected = configuredOption.IsSelected;
                existing.Instructions.Clear();
                foreach (Instruction instruction in configuredOption.Instructions)
                {
                    instruction.SetParentComponent(target);
                    existing.Instructions.Add(instruction);
                }
            }

            foreach (Instruction instruction in configured.Instructions)
            {
                instruction.SetParentComponent(target);
                target.Instructions.Add(instruction);
            }
        }

        private static void RemovePriorFomodInstructions(
            [NotNull] ModComponent target,
            [NotNull] string archivePathPrefix,
            [NotNull] HashSet<Guid> configuredOptionGuids)
        {
            RemoveMatchingInstructions(target.Instructions, archivePathPrefix, configuredOptionGuids);

            foreach (Option option in target.Options)
            {
                RemoveMatchingInstructions(option.Instructions, archivePathPrefix, configuredOptionGuids);
            }
        }

        private static void RemoveMatchingInstructions(
            [NotNull] System.Collections.ObjectModel.ObservableCollection<Instruction> instructions,
            [NotNull] string archivePathPrefix,
            [NotNull] HashSet<Guid> configuredOptionGuids)
        {
            for (int index = instructions.Count - 1; index >= 0; index--)
            {
                Instruction instruction = instructions[index];
                if (InstructionReferencesArchive(instruction, archivePathPrefix)
                    || IsChooseForConfiguredOptions(instruction, configuredOptionGuids))
                {
                    instructions.RemoveAt(index);
                }
            }
        }

        private static bool InstructionReferencesArchive([NotNull] Instruction instruction, [NotNull] string archivePathPrefix)
        {
            if (instruction.Source is null)
            {
                return false;
            }

            foreach (string source in instruction.Source)
            {
                if (string.IsNullOrEmpty(source))
                {
                    continue;
                }

                string normalizedSource = source.Replace('\\', '/');
                string normalizedPrefix = archivePathPrefix.Replace('\\', '/');
                if (normalizedSource.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsChooseForConfiguredOptions(
            [NotNull] Instruction instruction,
            [NotNull] HashSet<Guid> configuredOptionGuids)
        {
            if (instruction.Action != Instruction.ActionType.Choose || instruction.Source is null || instruction.Source.Count == 0)
            {
                return false;
            }

            foreach (string source in instruction.Source)
            {
                if (!Guid.TryParse(source, out Guid optionGuid) || !configuredOptionGuids.Contains(optionGuid))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
