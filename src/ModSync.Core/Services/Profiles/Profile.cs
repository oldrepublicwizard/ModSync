// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace ModSync.Core.Services.Profiles
{
    /// <summary>
    /// Per-component selection state stored inside a <see cref="Profile"/>.
    /// </summary>
    public sealed class ProfileComponentSelection
    {
        /// <summary>Whether the component itself is selected for install.</summary>
        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; }

        /// <summary>GUIDs of the component's options that are selected.</summary>
        [NotNull]
        [JsonProperty("selectedOptionGuids")]
        public List<Guid> SelectedOptionGuids { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// A named loadout: directories, instruction file, and per-component selection state.
    /// Activation copies these values into <see cref="MainConfig"/> and the loaded
    /// component list so the existing install pipeline keeps working unchanged.
    /// </summary>
    public sealed class Profile
    {
        /// <summary>Display name of the profile. Also drives the (sanitized) filename.</summary>
        [NotNull]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Absolute path to the KOTOR game directory (MainConfig.DestinationPath).</summary>
        [CanBeNull]
        [JsonProperty("kotorDirectory")]
        public string KotorDirectory { get; set; }

        /// <summary>Absolute path to the mod archives directory (MainConfig.SourcePath).</summary>
        [CanBeNull]
        [JsonProperty("modDirectory")]
        public string ModDirectory { get; set; }

        /// <summary>Absolute path of the instruction file this profile was captured against.</summary>
        [CanBeNull]
        [JsonProperty("instructionFilePath")]
        public string InstructionFilePath { get; set; }

        /// <summary>Selection state keyed by component GUID.</summary>
        [NotNull]
        [JsonProperty("componentSelections")]
        public Dictionary<Guid, ProfileComponentSelection> ComponentSelections { get; set; }
            = new Dictionary<Guid, ProfileComponentSelection>();

        /// <summary>UTC timestamp when the profile was first created.</summary>
        [JsonProperty("createdUtc")]
        public DateTime CreatedUtc { get; set; }

        /// <summary>UTC timestamp when the profile was last activated.</summary>
        [JsonProperty("lastUsedUtc")]
        public DateTime LastUsedUtc { get; set; }
    }
}
