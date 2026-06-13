// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.FileSystem;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodArchiveExtractService
    {
        [CanBeNull]
        public static async Task<string> ExtractAsync(
            [NotNull] string archivePath,
            [NotNull] string modDirectory,
            CancellationToken cancellationToken = default)
        {
            if (archivePath is null)
            {
                throw new ArgumentNullException(nameof(archivePath));
            }

            if (modDirectory is null)
            {
                throw new ArgumentNullException(nameof(modDirectory));
            }

            string extractFolderName = Path.GetFileNameWithoutExtension(archivePath);
            string extractedDirectory = Path.Combine(modDirectory, extractFolderName);

            try
            {
                var fileSystemProvider = new RealFileSystemProvider();
                _ = await fileSystemProvider.ExtractArchiveAsync(archivePath, modDirectory).ConfigureAwait(false);

                if (FomodArchiveDiscovery.FindModuleConfigPath(extractedDirectory) != null)
                {
                    return extractedDirectory;
                }

                await Logger.LogWarningAsync(
                    $"[FomodPostDownload] Extracted '{archivePath}' but no fomod/ModuleConfig.xml found under '{extractedDirectory}'.")
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, $"[FomodPostDownload] Failed to extract '{archivePath}'")
                    .ConfigureAwait(false);
            }

            return null;
        }
    }
}
