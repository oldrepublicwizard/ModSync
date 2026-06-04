// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Core.CLI;

namespace ModSync.Tests
{

    public static class Program
    {
        public static int Main(string[] args)
        {

            return ModBuildConverter.Run(args);
        }
    }
}
