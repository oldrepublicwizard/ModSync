// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ModSync.Core.Services.Download;

namespace ModSync.Core.Ports.Download
{
    /// <summary>
    /// Expansion-ready registry of download providers shared by GUI and CLI.
    /// Default implementation: <see cref="DownloadManager"/>.
    /// </summary>
    public interface IDownloadProviderRegistry
    {
        /// <summary>Returns the first registered handler that can process <paramref name="url"/>, or null.</summary>
        IDownloadHandler GetHandlerForUrl(string url);

        /// <summary>Registered provider keys in resolution order (most specific first).</summary>
        IReadOnlyList<string> GetProviderKeys();

        Task<Dictionary<string, List<string>>> ResolveUrlsToFilenamesAsync(
            IEnumerable<string> urls,
            CancellationToken cancellationToken = default,
            bool sequential = false);

        /// <summary>
        /// Downloads via the resolved provider for <paramref name="url"/>.
        /// Returns a failed result when no handler matches.
        /// </summary>
        Task<DownloadResult> DownloadViaProviderAsync(
            string url,
            string destinationDirectory,
            System.IProgress<DownloadProgress> progress = null,
            List<string> targetFilenames = null,
            CancellationToken cancellationToken = default);
    }
}
