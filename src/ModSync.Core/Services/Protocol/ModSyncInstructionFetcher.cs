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
    /// <see cref="FileLoadingService"/> consumption.
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
            if (string.IsNullOrWhiteSpace(instructionUrl))
            {
                throw new ArgumentException("Instruction URL is required.", nameof(instructionUrl));
            }

            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (!Uri.TryCreate(instructionUrl, UriKind.Absolute, out Uri uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException(
                    "Instruction URL must be an absolute http(s) URL.",
                    nameof(instructionUrl));
            }

            string extension = Path.GetExtension(uri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 8)
            {
                extension = ".toml";
            }

            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "modsync-protocol-" + Guid.NewGuid().ToString("N") + extension);

            using (HttpResponseMessage response = await httpClient
                       .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                       .ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                using (Stream remote = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (FileStream local = new FileStream(
                           tempPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None,
                           bufferSize: 81920,
                           useAsync: true))
                {
                    await remote.CopyToAsync(local, 81920, cancellationToken).ConfigureAwait(false);
                }
            }

            return tempPath;
        }
    }
}
