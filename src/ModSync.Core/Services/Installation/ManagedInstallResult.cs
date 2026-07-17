// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Installation
{
    /// <summary>
    /// Summary of a completed install when managed deployment was active.
    /// </summary>
    public sealed class ManagedInstallResult
    {
        public bool ManagedModeUsed { get; set; }

        public int ManifestsWritten { get; set; }

        [NotNull]
        public IList<string> PatcherComponentNames { get; } = new List<string>();

        [CanBeNull]
        public string ActiveProfileName { get; set; }

        public bool HasPatcherComponents => PatcherComponentNames.Count > 0;

        /// <summary>
        /// True when at least one patcher's live game-dir writes were merged into a deployment manifest.
        /// </summary>
        public bool PatcherProvenanceRecorded { get; set; }
    }
}
