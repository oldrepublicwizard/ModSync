// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Locates FOMOD metadata files inside an extracted mod archive directory.
    /// </summary>
    public static class FomodArchiveDiscovery
    {
        private const string ModuleConfigRelativePath = "fomod/ModuleConfig.xml";
        private const string InfoRelativePath = "fomod/info.xml";

        [CanBeNull]
        public static string FindModuleConfigPath([NotNull] string extractedArchiveDirectory)
        {
            if (extractedArchiveDirectory is null)
            {
                throw new ArgumentNullException(nameof(extractedArchiveDirectory));
            }

            return FindFileCaseInsensitive(extractedArchiveDirectory, ModuleConfigRelativePath);
        }

        [CanBeNull]
        public static string FindInfoPath([NotNull] string extractedArchiveDirectory)
        {
            if (extractedArchiveDirectory is null)
            {
                throw new ArgumentNullException(nameof(extractedArchiveDirectory));
            }

            return FindFileCaseInsensitive(extractedArchiveDirectory, InfoRelativePath);
        }

        [CanBeNull]
        private static string FindFileCaseInsensitive([NotNull] string rootDirectory, [NotNull] string relativePath)
        {
            string directPath = Path.Combine(rootDirectory, relativePath);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            string alternate = relativePath.Replace('/', '\\');
            if (!string.Equals(alternate, relativePath, StringComparison.Ordinal))
            {
                directPath = Path.Combine(rootDirectory, alternate);
                if (File.Exists(directPath))
                {
                    return directPath;
                }
            }

            foreach (string filePath in Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories))
            {
                string normalized = filePath.Replace('\\', '/');
                if (normalized.EndsWith("/" + relativePath, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(normalized, relativePath, StringComparison.OrdinalIgnoreCase))
                {
                    return filePath;
                }
            }

            return null;
        }
    }
}
