// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using JetBrains.Annotations;

using ModSync.Core.Utility;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Detects FOMOD installers inside downloaded archive files without extracting them.
    /// </summary>
    public static class FomodArchiveProbe
    {
        /// <summary>
        /// Inspects an archive on disk. Returns <c>false</c> when the archive cannot be enumerated
        /// (corrupt, unsupported, missing path). When <c>true</c>, <paramref name="isFomod"/> indicates
        /// whether a ModuleConfig was found.
        /// </summary>
        public static bool TryInspectArchive(
            [NotNull] string archivePath,
            out bool isFomod,
            out string moduleConfigEntryPath,
            out string failureMessage)
        {
            isFomod = false;
            moduleConfigEntryPath = null;
            failureMessage = null;

            if (string.IsNullOrWhiteSpace(archivePath))
            {
                failureMessage = "Archive path is empty";
                return false;
            }

            if (!ArchiveHelper.TryGetArchiveEntries(archivePath, out HashSet<string> entries, out failureMessage))
            {
                if (string.IsNullOrEmpty(failureMessage))
                {
                    failureMessage = "Unable to enumerate archive entries";
                }

                return false;
            }

            moduleConfigEntryPath = FomodDetector.FindModuleConfigPath(entries);
            isFomod = moduleConfigEntryPath != null;
            return true;
        }

        public static bool TryDetectInArchive(
            [NotNull] string archivePath,
            out string moduleConfigEntryPath)
        {
            return TryInspectArchive(archivePath, out bool isFomod, out moduleConfigEntryPath, out _)
                   && isFomod;
        }
    }
}
