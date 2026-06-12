// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Detects FOMOD installer packages by locating the <c>fomod/ModuleConfig.xml</c>
    /// entry inside an archive's entry listing. Matching is case-insensitive and
    /// tolerates any root prefix (for example <c>MyMod-1.0/Fomod/moduleconfig.xml</c>)
    /// and both slash directions.
    /// </summary>
    public static class FomodDetector
    {
        private const string FomodDirectoryName = "fomod";
        private const string ModuleConfigFileName = "ModuleConfig.xml";

        /// <summary>
        /// Returns the first archive entry path that is a <c>fomod/ModuleConfig.xml</c>
        /// file, or null when the archive contains no FOMOD installer.
        /// </summary>
        [CanBeNull]
        public static string FindModuleConfigPath([CanBeNull][ItemCanBeNull] IEnumerable<string> archiveEntryPaths)
        {
            if (archiveEntryPaths is null)
            {
                return null;
            }

            foreach (string entryPath in archiveEntryPaths)
            {
                if (IsModuleConfigPath(entryPath))
                {
                    return entryPath;
                }
            }

            return null;
        }

        /// <summary>
        /// True when the given archive entry path ends with the
        /// <c>fomod/ModuleConfig.xml</c> segment pair (case-insensitive).
        /// </summary>
        public static bool IsModuleConfigPath([CanBeNull] string entryPath)
        {
            if (string.IsNullOrWhiteSpace(entryPath))
            {
                return false;
            }

            string normalized = entryPath.Replace('\\', '/').Trim('/');
            string[] segments = normalized.Split('/');
            if (segments.Length < 2)
            {
                return false;
            }

            return string.Equals(segments[segments.Length - 1], ModuleConfigFileName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(segments[segments.Length - 2], FomodDirectoryName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
