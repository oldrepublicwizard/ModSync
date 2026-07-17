// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Protocol
{
    /// <summary>
    /// Downloads an http(s) instruction URL to a local temp file for
    /// file-loading consumption. Enforces host safety and a size cap.
    /// </summary>
    public static class ModSyncInstructionFetcher
    {
        /// <summary>
        /// Downloads <paramref name="instructionUrl"/> to a new temp file whose
        /// extension is inferred from the URL path (defaults to <c>.toml</c>).
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public static async Task<string> DownloadToTempFileAsync(
            [NotNull] string instructionUrl,
            [NotNull] HttpClient httpClient,
            CancellationToken cancellationToken = default)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            Uri uri = InstructionUrlSafety.RequireAbsoluteHttpUrl(instructionUrl);
            await InstructionUrlSafety.EnsureSafeFetchTargetAsync(uri, cancellationToken)
                .ConfigureAwait(false);

            string extension = Path.GetExtension(uri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 8)
            {
                extension = ".toml";
            }

            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "modsync-protocol-" + Guid.NewGuid().ToString("N") + extension);

            try
            {
                using (HttpResponseMessage response = await httpClient
                           .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                           .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    Uri finalUri = response.RequestMessage?.RequestUri ?? uri;
                    await InstructionUrlSafety.EnsureSafeFetchTargetAsync(finalUri, cancellationToken)
                        .ConfigureAwait(false);

                    long? contentLength = response.Content.Headers.ContentLength;
                    if (contentLength.HasValue && contentLength.Value > InstructionUrlSafety.MaxInstructionBytes)
                    {
                        throw new InvalidOperationException(
                            $"Instruction file exceeds the {InstructionUrlSafety.MaxInstructionBytes} byte download limit.");
                    }

                    using (Stream remote = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (FileStream local = new FileStream(
                               tempPath,
                               FileMode.CreateNew,
                               FileAccess.Write,
                               FileShare.None,
                               bufferSize: 81920,
                               useAsync: true))
                    {
                        await CopyWithLimitAsync(
                                remote,
                                local,
                                InstructionUrlSafety.MaxInstructionBytes,
                                cancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                return tempPath;
            }
            catch
            {
                TryDeleteTempFile(tempPath);
                throw;
            }
        }

        private static async Task CopyWithLimitAsync(
            [NotNull] Stream source,
            [NotNull] Stream destination,
            long maxBytes,
            CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                int read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false);
                if (read <= 0)
                {
                    break;
                }

                total += read;
                if (total > maxBytes)
                {
                    throw new InvalidOperationException(
                        $"Instruction file exceeds the {maxBytes} byte download limit.");
                }

                await destination.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
            }
        }

        private static void TryDeleteTempFile([CanBeNull] string tempPath)
        {
            if (string.IsNullOrEmpty(tempPath))
            {
                return;
            }

            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Best-effort cleanup after a failed download.
            }
        }
    }
}
