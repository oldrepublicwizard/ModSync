// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using ModSync.Core.Utility;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodDownloadedArchivePaths
    {
        [ItemNotNull]
        public static IEnumerable<string> GetPaths(
            [NotNull] ModComponent component,
            [NotNull] string modDirectory)
        {
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

                    string filePath = Path.Combine(modDirectory, fileName);
                    if (!File.Exists(filePath) || !ArchiveHelper.IsArchive(filePath))
                    {
                        continue;
                    }

                    yield return filePath;
                }
            }
        }
    }
}
