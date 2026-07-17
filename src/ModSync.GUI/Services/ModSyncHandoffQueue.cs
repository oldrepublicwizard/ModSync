// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ModSync.Services
{
    /// <summary>
    /// Thread-safe static queue decoupling modsync:// URL arrival from GUI readiness.
    /// </summary>
    public static class ModSyncHandoffQueue
    {
        private static readonly object _lock = new object();
        private static readonly Queue<string> _pending = new Queue<string>();

        public static event EventHandler<string> UrlEnqueued;

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

        public static void Enqueue(string modSyncUrl)
        {
            if (string.IsNullOrWhiteSpace(modSyncUrl))
            {
                return;
            }

            lock (_lock)
            {
                _pending.Enqueue(modSyncUrl);
            }

            Core.Logger.LogVerbose($"[ModSyncHandoffQueue] Enqueued modsync URL (pending: {Count})");
            UrlEnqueued?.Invoke(null, modSyncUrl);
        }

        public static bool TryDequeue(out string modSyncUrl)
        {
            lock (_lock)
            {
                if (_pending.Count > 0)
                {
                    modSyncUrl = _pending.Dequeue();
                    return true;
                }
            }

            modSyncUrl = null;
            return false;
        }

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
