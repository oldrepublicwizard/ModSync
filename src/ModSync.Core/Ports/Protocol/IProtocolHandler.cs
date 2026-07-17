// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Ports.Protocol
{
    /// <summary>Normalized deep-link request after scheme detection.</summary>
    public sealed class ProtocolRequest
    {
        public ProtocolRequest([NotNull] string rawUrl)
        {
            RawUrl = rawUrl ?? throw new System.ArgumentNullException(nameof(rawUrl));
        }

        [NotNull]
        public string RawUrl { get; }
    }

    /// <summary>Outcome of accepting a protocol URL (parse-only in this foundation slice).</summary>
    public sealed class ProtocolHandleResult
    {
        public ProtocolHandleResult(
            bool accepted,
            [NotNull] string scheme,
            [CanBeNull] string summary = null,
            [CanBeNull] object payload = null)
        {
            Accepted = accepted;
            Scheme = scheme ?? throw new System.ArgumentNullException(nameof(scheme));
            Summary = summary;
            Payload = payload;
        }

        public bool Accepted { get; }

        [NotNull]
        public string Scheme { get; }

        [CanBeNull]
        public string Summary { get; }

        /// <summary>Typed parse payload (e.g. <c>NxmUrl</c> or <c>ModSyncUrl</c>).</summary>
        [CanBeNull]
        public object Payload { get; }
    }

    /// <summary>Pluggable deep-link scheme handler (nxm, modsync, future schemes).</summary>
    public interface IProtocolHandler
    {
        /// <summary>Scheme without <c>://</c>, e.g. <c>nxm</c> or <c>modsync</c>.</summary>
        [NotNull]
        string Scheme { get; }

        bool CanHandle([NotNull] string rawUrl);

        Task<ProtocolHandleResult> HandleAsync(
            [NotNull] ProtocolRequest request,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Resolves a raw URL to the first matching <see cref="IProtocolHandler"/>.</summary>
    public interface IProtocolHandlerRegistry
    {
        void Register([NotNull] IProtocolHandler handler);

        [CanBeNull]
        IProtocolHandler FindHandler([NotNull] string rawUrl);

        [NotNull]
        [ItemNotNull]
        IReadOnlyList<IProtocolHandler> Handlers { get; }

        Task<ProtocolHandleResult> HandleAsync(
            [NotNull] string rawUrl,
            CancellationToken cancellationToken = default);
    }
}
