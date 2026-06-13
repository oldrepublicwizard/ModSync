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
        public static bool TryDetectInArchive(
            [NotNull] string archivePath,
            out string moduleConfigEntryPath)
        {
            moduleConfigEntryPath = null;

            if (string.IsNullOrWhiteSpace(archivePath))
            {
                return false;
            }

            if (!ArchiveHelper.TryGetArchiveEntries(archivePath, out HashSet<string> entries, out _))
            {
                return false;
            }

            moduleConfigEntryPath = FomodDetector.FindModuleConfigPath(entries);
            return moduleConfigEntryPath != null;
        }
    }
}
