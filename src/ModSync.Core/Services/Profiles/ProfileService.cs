// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

using ModSync.Core.Ports.Profiles;

using Newtonsoft.Json;

namespace ModSync.Core.Services.Profiles
{
    /// <summary>
    /// CRUD + capture/apply for install profiles (MO2-style loadouts).
    /// <para>
    /// Profiles are persisted as one JSON file per profile under
    /// <c>{storageDirectory}/profiles/</c>. The storage directory is passed in by the
    /// caller (the GUI passes the ModSync settings directory) so Core never references
    /// GUI-side settings code.
    /// </para>
    /// <para>
    /// Activation deliberately writes into the existing static <see cref="MainConfig"/>
    /// (via its instance accessors) instead of refactoring MainConfig consumers -
    /// minimal-blast-radius by design.
    /// </para>
    /// </summary>
    public sealed class ProfileService : IProfileStore
    {
        private const string ProfilesSubdirectory = "profiles";
        private const string ProfileFileExtension = ".json";

        [NotNull]
        private static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
        };

        [NotNull]
        private readonly string _profilesDirectory;

        public ProfileService([NotNull] string storageDirectory)
        {
            if (string.IsNullOrWhiteSpace(storageDirectory))
            {
                throw new ArgumentException("Storage directory cannot be null or whitespace.", nameof(storageDirectory));
            }

            _profilesDirectory = Path.Combine(storageDirectory, ProfilesSubdirectory);
        }

        /// <summary>Directory holding the per-profile JSON files.</summary>
        [NotNull]
        public string ProfilesDirectory => _profilesDirectory;

        /// <summary>
        /// Converts a profile name into a safe filename (without extension).
        /// Invalid filename characters and path separators become underscores.
        /// </summary>
        [NotNull]
        public static string SanitizeProfileFileName([NotNull] string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(profileName));
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new System.Text.StringBuilder(profileName.Length);
            foreach (char c in profileName.Trim())
            {
                // Treat both separator styles as invalid on every OS so profile names
                // sanitize identically on Windows and Linux.
                bool invalid = c == '/'
                    || c == '\\'
                    || c == Path.DirectorySeparatorChar
                    || c == Path.AltDirectorySeparatorChar
                    || Array.IndexOf(invalidChars, c) >= 0;
                _ = sanitized.Append(invalid ? '_' : c);
            }

            string result = sanitized.ToString().Trim();

            // Avoid names that are only dots/underscores or empty after sanitization.
            if (result.Length == 0 || result.All(c => c == '.' || c == '_'))
            {
                throw new ArgumentException($"Profile name '{profileName}' does not produce a usable filename.", nameof(profileName));
            }

            return result;
        }

        [NotNull]
        private string GetProfileFilePath([NotNull] string profileName) =>
            Path.Combine(_profilesDirectory, SanitizeProfileFileName(profileName) + ProfileFileExtension);

        /// <summary>Returns all profiles on disk, ordered by name. Corrupt files are skipped.</summary>
        [NotNull]
        [ItemNotNull]
        public List<Profile> ListProfiles()
        {
            var profiles = new List<Profile>();
            if (!Directory.Exists(_profilesDirectory))
            {
                return profiles;
            }

            foreach (string filePath in Directory.GetFiles(_profilesDirectory, "*" + ProfileFileExtension))
            {
                try
                {
                    Profile profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(filePath), s_jsonSettings);
                    if (profile != null && !string.IsNullOrWhiteSpace(profile.Name))
                    {
                        profiles.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"[ProfileService.ListProfiles] Skipping unreadable profile file '{filePath}': {ex.Message}");
                }
            }

            return profiles.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>Loads a single profile by name, or null when it does not exist or cannot be read.</summary>
        [CanBeNull]
        public Profile LoadProfile([NotNull] string profileName)
        {
            string filePath = GetProfileFilePath(profileName);
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Profile>(File.ReadAllText(filePath), s_jsonSettings);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[ProfileService.LoadProfile] Failed to read profile '{profileName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>Persists a profile atomically (temp file in the same directory, then move).</summary>
        public void SaveProfile([NotNull] Profile profile)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            string filePath = GetProfileFilePath(profile.Name);
            if (!Directory.Exists(_profilesDirectory))
            {
                _ = Directory.CreateDirectory(_profilesDirectory);
            }

            string json = JsonConvert.SerializeObject(profile, s_jsonSettings);
            string tempPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(tempPath, json);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
            }
            catch
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Leave the temp file behind if cleanup also fails; nothing else to do.
                }

                throw;
            }
        }

        /// <summary>Creates and persists a new, empty profile. Fails when the name is already taken.</summary>
        [NotNull]
        public Profile CreateProfile([NotNull] string profileName)
        {
            string filePath = GetProfileFilePath(profileName);
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException($"A profile named '{profileName}' already exists.");
            }

            DateTime now = DateTime.UtcNow;
            var profile = new Profile
            {
                Name = profileName.Trim(),
                CreatedUtc = now,
                LastUsedUtc = now,
            };
            SaveProfile(profile);
            return profile;
        }

        /// <summary>Deep-copies an existing profile under a new name and persists the copy.</summary>
        [NotNull]
        public Profile CloneProfile([NotNull] Profile source, [NotNull] string newName)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            string filePath = GetProfileFilePath(newName);
            if (File.Exists(filePath))
            {
                throw new InvalidOperationException($"A profile named '{newName}' already exists.");
            }

            // Round-trip through JSON for a simple deep copy.
            Profile clone = JsonConvert.DeserializeObject<Profile>(JsonConvert.SerializeObject(source, s_jsonSettings), s_jsonSettings)
                ?? throw new InvalidOperationException($"Failed to clone profile '{source.Name}'.");
            clone.Name = newName.Trim();
            clone.CreatedUtc = DateTime.UtcNow;
            SaveProfile(clone);
            return clone;
        }

        /// <summary>Renames a profile on disk (saves under the new name, removes the old file).</summary>
        [NotNull]
        public Profile RenameProfile([NotNull] string oldName, [NotNull] string newName)
        {
            string newPath = GetProfileFilePath(newName);
            string oldPath = GetProfileFilePath(oldName);
            if (string.Equals(oldPath, newPath, StringComparison.Ordinal))
            {
                Profile unchanged = LoadProfile(oldName)
                    ?? throw new InvalidOperationException($"Profile '{oldName}' does not exist.");
                unchanged.Name = newName.Trim();
                SaveProfile(unchanged);
                return unchanged;
            }

            if (File.Exists(newPath))
            {
                throw new InvalidOperationException($"A profile named '{newName}' already exists.");
            }

            Profile profile = LoadProfile(oldName)
                ?? throw new InvalidOperationException($"Profile '{oldName}' does not exist.");
            profile.Name = newName.Trim();
            SaveProfile(profile);
            File.Delete(oldPath);
            return profile;
        }

        /// <summary>Deletes a profile file. Returns false when no such profile exists.</summary>
        public bool DeleteProfile([NotNull] string profileName)
        {
            string filePath = GetProfileFilePath(profileName);
            if (!File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            return true;
        }

        /// <summary>
        /// Builds and persists a profile from the current application state:
        /// <see cref="MainConfig.SourcePath"/>/<see cref="MainConfig.DestinationPath"/> plus the
        /// selection state of the given components and their options.
        /// </summary>
        [NotNull]
        public Profile CaptureFromCurrentState(
            [NotNull] string profileName,
            [NotNull][ItemNotNull] IEnumerable<ModComponent> components,
            [CanBeNull] string instructionFilePath = null)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            DateTime now = DateTime.UtcNow;
            Profile existing = LoadProfile(profileName);
            var profile = new Profile
            {
                Name = profileName.Trim(),
                KotorDirectory = MainConfig.DestinationPath?.FullName,
                ModDirectory = MainConfig.SourcePath?.FullName,
                InstructionFilePath = instructionFilePath,
                CreatedUtc = existing?.CreatedUtc ?? now,
                LastUsedUtc = now,
            };

            foreach (ModComponent component in components)
            {
                var selection = new ProfileComponentSelection
                {
                    IsSelected = component.IsSelected,
                };
                foreach (Option option in component.Options)
                {
                    if (option.IsSelected)
                    {
                        selection.SelectedOptionGuids.Add(option.Guid);
                    }
                }

                profile.ComponentSelections[component.Guid] = selection;
            }

            SaveProfile(profile);
            return profile;
        }

        /// <summary>
        /// Activates a profile: writes its directories into <see cref="MainConfig"/> (via the
        /// instance accessors) and applies component/option <c>IsSelected</c> flags to the given
        /// components. Components without an entry in the profile are left untouched. Updates
        /// <see cref="Profile.LastUsedUtc"/> and persists the profile.
        /// </summary>
        public void ApplyProfile([NotNull] Profile profile, [NotNull][ItemNotNull] IEnumerable<ModComponent> components)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (!string.IsNullOrEmpty(profile.ModDirectory))
            {
                MainConfig.Instance.sourcePath = new DirectoryInfo(profile.ModDirectory);
            }

            if (!string.IsNullOrEmpty(profile.KotorDirectory))
            {
                MainConfig.Instance.destinationPath = new DirectoryInfo(profile.KotorDirectory);
            }

            foreach (ModComponent component in components)
            {
                if (!profile.ComponentSelections.TryGetValue(component.Guid, out ProfileComponentSelection selection))
                {
                    continue;
                }

                component.IsSelected = selection.IsSelected;
                foreach (Option option in component.Options)
                {
                    option.IsSelected = selection.SelectedOptionGuids.Contains(option.Guid);
                }
            }

            profile.LastUsedUtc = DateTime.UtcNow;
            SaveProfile(profile);
        }
    }
}
