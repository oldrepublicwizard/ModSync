// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using Newtonsoft.Json;

namespace ModSync.Core.Services.Fomod
{
    public sealed class FomodChoicesFile
    {
        [JsonProperty("version")]
        public int Version { get; set; } = 1;

        [JsonProperty("archives")]
        public List<FomodArchiveChoices> Archives { get; set; } = new List<FomodArchiveChoices>();
    }

    public sealed class FomodArchiveChoices
    {
        [JsonProperty("archiveFileName")]
        public string ArchiveFileName { get; set; }

        [JsonProperty("selections")]
        public List<FomodGroupSelection> Selections { get; set; } = new List<FomodGroupSelection>();
    }

    public sealed class FomodGroupSelection
    {
        [JsonProperty("stepName")]
        public string StepName { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("plugins")]
        public List<string> Plugins { get; set; } = new List<string>();
    }
}
