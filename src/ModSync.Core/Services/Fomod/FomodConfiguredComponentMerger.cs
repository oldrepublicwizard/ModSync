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
    /// Merges FOMOD wizard output into an existing mod component from the instruction file.
    /// </summary>
    public static class FomodConfiguredComponentMerger
    {
        public static void MergeInto(
            [NotNull] ModComponent target,
            [NotNull] ModComponent configured)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (configured is null)
            {
                throw new ArgumentNullException(nameof(configured));
            }

            var existingOptionGuids = new HashSet<Guid>(target.Options.Select(option => option.Guid));
            foreach (Option option in configured.Options)
            {
                if (existingOptionGuids.Contains(option.Guid))
                {
                    continue;
                }

                target.Options.Add(option);
                existingOptionGuids.Add(option.Guid);
            }

            foreach (Instruction instruction in configured.Instructions)
            {
                instruction.SetParentComponent(target);
                target.Instructions.Add(instruction);
            }
        }
    }
}
