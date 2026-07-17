// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

using ModSync.Core.Utility;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodDownloadedArchivePaths
    {
        /// <summary>
        /// Registered archive entry under a component's resource registry.
        /// </summary>
        public sealed class RegisteredArchive
        {
            /// <summary>Relative registry key (may include nested folders).</summary>
            [NotNull]
            public string RegisteredName { get; set; }

            /// <summary>Absolute path under the mod directory.</summary>
            [NotNull]
            public string FullPath { get; set; }

            public bool ExistsOnDisk { get; set; }
        }

        /// <summary>
        /// Yields on-disk archive paths only (existing files). Used by post-download prompting.
        /// </summary>
        [ItemNotNull]
        public static IEnumerable<string> GetPaths(
            [NotNull] ModComponent component,
            [NotNull] string modDirectory)
        {
            foreach (RegisteredArchive entry in EnumerateRegisteredArchives(component, modDirectory))
            {
                if (entry.ExistsOnDisk)
                {
                    yield return entry.FullPath;
                }
            }
        }

        /// <summary>
        /// Enumerates every registered archive-like file for a component, including missing on-disk paths.
        /// </summary>
        [ItemNotNull]
        public static IEnumerable<RegisteredArchive> EnumerateRegisteredArchives(
            [NotNull] ModComponent component,
            [NotNull] string modDirectory)
        {
            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new ArgumentException("Mod directory is required.", nameof(modDirectory));
            }

            if (component.ResourceRegistry is null || component.ResourceRegistry.Count == 0)
            {
                yield break;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ResourceMetadata resource in component.ResourceRegistry.Values)
            {
                if (resource?.Files is null)
                {
                    continue;
                }

                foreach (string fileName in resource.Files.Keys)
                {
                    if (string.IsNullOrWhiteSpace(fileName) || !seen.Add(fileName))
                    {
                        continue;
                    }

                    if (!ArchiveHelper.IsArchive(fileName))
                    {
                        continue;
                    }

                    string filePath = Path.Combine(modDirectory, fileName);
                    yield return new RegisteredArchive
                    {
                        RegisteredName = fileName,
                        FullPath = filePath,
                        ExistsOnDisk = File.Exists(filePath),
                    };
                }
            }
        }
    }
}
