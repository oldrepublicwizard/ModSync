// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

namespace ModSync.Services
{
    /// <summary>
    /// Holds the primary <see cref="SingleInstanceService"/> for the running GUI process.
    /// </summary>
    public static class ApplicationSingleInstanceContext
    {
        public static SingleInstanceService PrimaryInstance { get; set; }
    }
}
