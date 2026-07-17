// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;

using JetBrains.Annotations;

namespace ModSync.Core.Ports.Profiles
{
    /// <summary>
    /// Profile / loadout store shared by GUI and CLI.
    /// Default implementation: <see cref="Services.Profiles.ProfileService"/>.
    /// </summary>
    public interface IProfileStore
    {
        [NotNull]
        string ProfilesDirectory { get; }

        [NotNull]
        [ItemNotNull]
        List<Services.Profiles.Profile> ListProfiles();

        [CanBeNull]
        Services.Profiles.Profile LoadProfile([NotNull] string profileName);

        void SaveProfile([NotNull] Services.Profiles.Profile profile);

        [NotNull]
        Services.Profiles.Profile CreateProfile([NotNull] string profileName);

        [NotNull]
        Services.Profiles.Profile CloneProfile([NotNull] Services.Profiles.Profile source, [NotNull] string newName);

        [NotNull]
        Services.Profiles.Profile RenameProfile([NotNull] string oldName, [NotNull] string newName);

        bool DeleteProfile([NotNull] string profileName);

        [NotNull]
        Services.Profiles.Profile CaptureFromCurrentState(
            [NotNull] string profileName,
            [NotNull][ItemNotNull] IEnumerable<ModComponent> components,
            [CanBeNull] string instructionFilePath = null);

        void ApplyProfile(
            [NotNull] Services.Profiles.Profile profile,
            [NotNull][ItemNotNull] IEnumerable<ModComponent> components);
    }
}
