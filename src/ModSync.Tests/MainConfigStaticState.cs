// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using ModSync.Core;
using Xunit;

namespace ModSync.Tests
{
    /// <summary>
    /// <see cref="MainConfig.SourcePath"/>, <see cref="MainConfig.DestinationPath"/>, and
    /// <see cref="MainConfig.AllComponents"/> are process-wide statics. Tests that mutate them
    /// must serialize against each other (and reset in finally) to avoid cross-talk.
    /// </summary>
    public static class MainConfigStaticState
    {
        public const string CollectionName = "MainConfigStaticStateCollection";

        public static readonly object Gate = new object();

        public static void Reset()
        {
            lock (Gate)
            {
                MainConfig.AllComponents = new List<ModComponent>();
                var config = new MainConfig();
                config.sourcePath = null;
                config.destinationPath = null;
                MainConfig.Instance = config;
            }
        }
    }

    [CollectionDefinition(MainConfigStaticState.CollectionName, DisableParallelization = true)]
    public sealed class MainConfigStaticStateCollection : ICollectionFixture<object>
    {
    }
}
