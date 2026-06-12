// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Conflicts
{
    /// <summary>
    /// A destination path inside the game directory that more than one selected component writes
    /// during a simulated (dry-run) install.
    /// </summary>
    public sealed class FileConflict
    {
        /// <summary>
        /// Destination path relative to the game directory with the <c>&lt;&lt;kotorDirectory&gt;&gt;</c>
        /// placeholder prefix preserved, using forward slashes. Casing reflects the first write observed.
        /// </summary>
        [NotNull]
        public string DestinationPath { get; }

        /// <summary>
        /// Every component that writes <see cref="DestinationPath"/>, ordered by install order
        /// (the order the components were simulated in).
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<FileConflictWriter> Writers { get; }

        /// <summary>
        /// The component whose write lands last in install order; its file is the one that
        /// ends up on disk after a real install.
        /// </summary>
        public Guid WinnerComponentGuid { get; }

        public FileConflict(
            [NotNull] string destinationPath,
            [NotNull][ItemNotNull] IReadOnlyList<FileConflictWriter> writers,
            Guid winnerComponentGuid)
        {
            DestinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            Writers = writers ?? throw new ArgumentNullException(nameof(writers));
            WinnerComponentGuid = winnerComponentGuid;
        }
    }

    /// <summary>
    /// A single component's claim on a conflicting destination path.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Companion record of FileConflict")]
    public sealed class FileConflictWriter
    {
        public Guid ComponentGuid { get; }

        [NotNull]
        public string ComponentName { get; }

        /// <summary>
        /// Zero-based index into the component's <see cref="ModComponent.Instructions"/> of the
        /// last instruction of that component that wrote the destination path.
        /// </summary>
        public int InstructionIndex { get; }

        public FileConflictWriter(Guid componentGuid, [CanBeNull] string componentName, int instructionIndex)
        {
            ComponentGuid = componentGuid;
            ComponentName = componentName ?? string.Empty;
            InstructionIndex = instructionIndex;
        }
    }
}
