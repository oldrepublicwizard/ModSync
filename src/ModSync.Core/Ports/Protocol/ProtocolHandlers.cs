// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.Download;
using ModSync.Core.Services.Protocol;

namespace ModSync.Core.Ports.Protocol
{
    /// <summary>Accepts Nexus <c>nxm://</c> deep links (parse + payload).</summary>
    public sealed class NxmProtocolHandler : IProtocolHandler
    {
        public string Scheme => "nxm";

        public bool CanHandle(string rawUrl) => NxmUrl.IsNxmUrl(rawUrl);

        public Task<ProtocolHandleResult> HandleAsync(ProtocolRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!NxmUrl.TryParse(request.RawUrl, out NxmUrl nxm))
            {
                return Task.FromResult(new ProtocolHandleResult(accepted: false, Scheme, "Invalid nxm:// URL"));
            }

            string summary = string.Format(
                CultureInfo.InvariantCulture,
                "nxm {0} mod={1} file={2}",
                nxm.GameDomain,
                nxm.ModId,
                nxm.FileId);

            return Task.FromResult(new ProtocolHandleResult(accepted: true, Scheme, summary, nxm));
        }
    }

    /// <summary>Accepts ModSync <c>modsync://</c> build deep links (parse + payload).</summary>
    public sealed class ModSyncProtocolHandler : IProtocolHandler
    {
        public string Scheme => "modsync";

        public bool CanHandle(string rawUrl) => ModSyncUrl.IsModSyncUrl(rawUrl);

        public Task<ProtocolHandleResult> HandleAsync(ProtocolRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!ModSyncUrl.TryParse(request.RawUrl, out ModSyncUrl modSyncUrl))
            {
                return Task.FromResult(new ProtocolHandleResult(accepted: false, Scheme, "Invalid modsync:// URL"));
            }

            string summary = string.Format(
                CultureInfo.InvariantCulture,
                "modsync {0} game={1} instruction={2}",
                modSyncUrl.Action,
                modSyncUrl.Game ?? "(any)",
                modSyncUrl.InstructionUrl);

            return Task.FromResult(new ProtocolHandleResult(accepted: true, Scheme, summary, modSyncUrl));
        }
    }

    /// <summary>Default registry: nxm then modsync (registration order = match order).</summary>
    public sealed class ProtocolHandlerRegistry : IProtocolHandlerRegistry
    {
        private readonly List<IProtocolHandler> _handlers = new List<IProtocolHandler>();

        public ProtocolHandlerRegistry()
        {
        }

        public ProtocolHandlerRegistry([NotNull][ItemNotNull] IEnumerable<IProtocolHandler> handlers)
        {
            if (handlers is null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            foreach (IProtocolHandler handler in handlers)
            {
                Register(handler);
            }
        }

        /// <summary>Creates a registry with the shipped nxm + modsync handlers.</summary>
        [NotNull]
        public static ProtocolHandlerRegistry CreateDefault()
        {
            return new ProtocolHandlerRegistry(new IProtocolHandler[]
            {
                new NxmProtocolHandler(),
                new ModSyncProtocolHandler(),
            });
        }

        public IReadOnlyList<IProtocolHandler> Handlers => _handlers;

        public void Register(IProtocolHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
        }

        public IProtocolHandler FindHandler(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return null;
            }

            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].CanHandle(rawUrl))
                {
                    return _handlers[i];
                }
            }

            return null;
        }

        public async Task<ProtocolHandleResult> HandleAsync(string rawUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return new ProtocolHandleResult(accepted: false, scheme: "unknown", summary: "Empty protocol URL");
            }

            IProtocolHandler handler = FindHandler(rawUrl);
            if (handler is null)
            {
                return new ProtocolHandleResult(accepted: false, scheme: "unknown", summary: "No protocol handler matched");
            }

            return await handler.HandleAsync(new ProtocolRequest(rawUrl), cancellationToken).ConfigureAwait(false);
        }
    }
}
