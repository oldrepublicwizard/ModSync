// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class FileSystemServiceTests
    {
        [Fact(DisplayName = "SetupModDirectoryWatcher does not throw when watcher disabled")]
        public void SetupModDirectoryWatcher_Disabled_DoesNotThrow()
        {
            FileSystemService.SetupModDirectoryWatcher("/tmp", _ => { });
        }

        [Fact(DisplayName = "StopWatcher does not throw on fresh service")]
        public void StopWatcher_FreshService_DoesNotThrow()
        {
            using var service = new FileSystemService();
            service.StopWatcher();
        }

        [Fact(DisplayName = "Dispose can be called multiple times")]
        public void Dispose_MultipleCalls_DoesNotThrow()
        {
            var service = new FileSystemService();
            service.Dispose();
            service.Dispose();
        }
    }
}
