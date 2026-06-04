// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

namespace ModSync.Core.Data
{
    public static class Game
    {
        public static readonly IReadOnlyList<string> TextureOverridePriorityList = new List<string> { ".dds", ".tpc", ".tga" };
    }
}
