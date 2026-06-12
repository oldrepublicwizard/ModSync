// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Conflicts
{
    /// <summary>
    /// Result of a <see cref="FileConflictAnalyzer"/> pass over the selected components.
    /// </summary>
    public sealed class ConflictAnalysisResult
    {
        /// <summary>All conflicting destination paths, ordered by destination path.</summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<FileConflict> Conflicts { get; }

        /// <summary>
        /// For each component that participates in at least one conflict, the number of
        /// conflicting destination paths it writes.
        /// </summary>
        [NotNull]
        public IReadOnlyDictionary<Guid, int> ConflictCountsByComponent { get; }

        /// <summary>Number of selected components that were actually simulated.</summary>
        public int AnalyzedComponentCount { get; }

        public ConflictAnalysisResult(
            [NotNull][ItemNotNull] IReadOnlyList<FileConflict> conflicts,
            [NotNull] IReadOnlyDictionary<Guid, int> conflictCountsByComponent,
            int analyzedComponentCount)
        {
            Conflicts = conflicts ?? throw new ArgumentNullException(nameof(conflicts));
            ConflictCountsByComponent = conflictCountsByComponent ?? throw new ArgumentNullException(nameof(conflictCountsByComponent));
            AnalyzedComponentCount = analyzedComponentCount;
        }

        public bool HasConflicts => Conflicts.Count > 0;
    }
}
