// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    public sealed class FomodPromptContext
    {
        [NotNull]
        public ModComponent Component { get; }

        [NotNull]
        public string ArchivePath { get; }

        [NotNull]
        public string ArchiveFileName { get; }

        public FomodPromptContext(
            [NotNull] ModComponent component,
            [NotNull] string archivePath,
            [NotNull] string archiveFileName)
        {
            Component = component ?? throw new System.ArgumentNullException(nameof(component));
            ArchivePath = archivePath ?? throw new System.ArgumentNullException(nameof(archivePath));
            ArchiveFileName = archiveFileName ?? throw new System.ArgumentNullException(nameof(archiveFileName));
        }
    }

    public enum FomodConfigurePromptResult
    {
        Dismiss,
        Configure,
        AlreadyHandled,
    }
}
