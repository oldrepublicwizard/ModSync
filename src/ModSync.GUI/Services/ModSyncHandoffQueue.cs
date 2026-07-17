// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ModSync.Services
{
    /// <summary>
    /// Thread-safe static queue decoupling modsync:// URL arrival (CLI argument or
    /// single-instance pipe message) from GUI readiness.
    /// </summary>
    public static class ModSyncHandoffQueue
    {
        private static readonly object s_lock = new object();
        private static readonly Queue<string> s_pending = new Queue<string>();

        /// <summary>
        /// Raised when a URL is enqueued. Raised on the enqueuing thread; subscribers
        /// must marshal to the UI thread themselves.
        /// </summary>
        public static event EventHandler<string> UrlEnqueued;

        /// <summary>Number of URLs currently waiting.</summary>
        public static int Count
        {
            get
            {
                lock (s_lock)
                {
                    return s_pending.Count;
                }
            }
        }

        /// <summary>
        /// Adds a modsync URL to the queue and raises <see cref="UrlEnqueued"/>.
        /// Null/whitespace input is ignored.
        /// </summary>
        public static void Enqueue(string modSyncUrl)
        {
            if (string.IsNullOrWhiteSpace(modSyncUrl))
            {
                return;
            }

            lock (s_lock)
            {
                s_pending.Enqueue(modSyncUrl);
            }

            Core.Logger.LogVerbose($"[ModSyncHandoffQueue] Enqueued modsync URL (pending: {Count})");
            UrlEnqueued?.Invoke(null, modSyncUrl);
        }

        /// <summary>
        /// Attempts to remove the oldest pending URL.
        /// </summary>
        public static bool TryDequeue(out string modSyncUrl)
        {
            lock (s_lock)
            {
                if (s_pending.Count > 0)
                {
                    modSyncUrl = s_pending.Dequeue();
                    return true;
                }
            }

            modSyncUrl = null;
            return false;
        }

        /// <summary>
        /// Removes and returns all pending URLs in arrival order.
        /// </summary>
        public static List<string> DrainAll()
        {
            lock (s_lock)
            {
                var drained = new List<string>(s_pending);
                s_pending.Clear();
                return drained;
            }
        }
    }
}
