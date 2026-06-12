// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ModSync.Services
{
    /// <summary>
    /// Thread-safe static queue decoupling nxm:// URL arrival (CLI argument or
    /// single-instance pipe message) from GUI readiness. URLs enqueued before the
    /// main window subscribes are retained and can be drained later, so nothing
    /// is lost during startup.
    /// </summary>
    public static class NxmHandoffQueue
    {
        private static readonly object _lock = new object();
        private static readonly Queue<string> _pending = new Queue<string>();

        /// <summary>
        /// Raised when a URL is enqueued. Raised on the enqueuing thread; subscribers
        /// must marshal to the UI thread themselves. The URL stays in the queue until
        /// drained, so a subscriber may simply call <see cref="TryDequeue"/> in response.
        /// </summary>
        public static event EventHandler<string> UrlEnqueued;

        /// <summary>Number of URLs currently waiting.</summary>
        public static int Count
        {
            get
            {
                lock (_lock)
                {
                    return _pending.Count;
                }
            }
        }

        /// <summary>
        /// Adds an nxm URL to the queue and raises <see cref="UrlEnqueued"/>.
        /// Null/whitespace input is ignored.
        /// </summary>
        public static void Enqueue(string nxmUrl)
        {
            if (string.IsNullOrWhiteSpace(nxmUrl))
            {
                return;
            }

            lock (_lock)
            {
                _pending.Enqueue(nxmUrl);
            }

            Core.Logger.LogVerbose($"[NxmHandoffQueue] Enqueued nxm URL (pending: {Count})");
            UrlEnqueued?.Invoke(null, nxmUrl);
        }

        /// <summary>
        /// Attempts to remove the oldest pending URL.
        /// </summary>
        public static bool TryDequeue(out string nxmUrl)
        {
            lock (_lock)
            {
                if (_pending.Count > 0)
                {
                    nxmUrl = _pending.Dequeue();
                    return true;
                }
            }

            nxmUrl = null;
            return false;
        }

        /// <summary>
        /// Removes and returns all pending URLs in arrival order.
        /// </summary>
        public static List<string> DrainAll()
        {
            lock (_lock)
            {
                var drained = new List<string>(_pending);
                _pending.Clear();
                return drained;
            }
        }
    }
}
