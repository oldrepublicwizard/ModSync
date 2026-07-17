// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Settings
{
    /// <summary>
    /// Core-readable subset of persisted application settings in
    /// <c>%AppData%/ModSync/settings.json</c> (with legacy path fallback).
    /// </summary>
    public sealed class ModSyncSettings
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        [JsonPropertyName("managedDeploymentEnabled")]
        public bool ManagedDeploymentEnabled { get; set; }

        [JsonPropertyName("activeProfileName")]
        [CanBeNull]
        public string ActiveProfileName { get; set; }

        [NotNull]
        public static string GetSettingsDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModSync");
        }

        [NotNull]
        public static string ResolveSettingsFilePath()
        {
            string settingsPath = Path.Combine(GetSettingsDirectory(), "settings.json");
            string legacySettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KOTORModSync",
                "settings.json");

            if (!File.Exists(settingsPath) && File.Exists(legacySettingsPath))
            {
                return legacySettingsPath;
            }

            return settingsPath;
        }

        [NotNull]
        public static ModSyncSettings Load()
        {
            return LoadFromDirectory(GetSettingsDirectory());
        }

        [NotNull]
        public static ModSyncSettings LoadFromDirectory([NotNull] string settingsDirectory)
        {
            if (string.IsNullOrWhiteSpace(settingsDirectory))
            {
                throw new ArgumentException("Settings directory cannot be null or whitespace.", nameof(settingsDirectory));
            }

            string settingsPath = Path.Combine(settingsDirectory, "settings.json");
            string legacySettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KOTORModSync",
                "settings.json");

            if (!File.Exists(settingsPath) && File.Exists(legacySettingsPath))
            {
                settingsPath = legacySettingsPath;
            }

            if (!File.Exists(settingsPath))
            {
                return new ModSyncSettings();
            }

            try
            {
                string json = File.ReadAllText(settingsPath);
                ModSyncSettings settings = JsonSerializer.Deserialize<ModSyncSettings>(json, s_jsonOptions);
                return settings ?? new ModSyncSettings();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[ModSyncSettings.LoadFromDirectory] Failed to read settings: {ex.Message}");
                return new ModSyncSettings();
            }
        }

        /// <summary>
        /// Merges managed-deployment fields into the existing settings file without
        /// overwriting unrelated keys owned by the GUI.
        /// </summary>
        public void SaveManagedDeploymentFields()
        {
            SaveManagedDeploymentFieldsToDirectory(GetSettingsDirectory());
        }

        public void SaveManagedDeploymentFieldsToDirectory([NotNull] string settingsDirectory)
        {
            if (string.IsNullOrWhiteSpace(settingsDirectory))
            {
                throw new ArgumentException("Settings directory cannot be null or whitespace.", nameof(settingsDirectory));
            }

            if (!Directory.Exists(settingsDirectory))
            {
                _ = Directory.CreateDirectory(settingsDirectory);
            }

            string settingsPath = Path.Combine(settingsDirectory, "settings.json");
            File.WriteAllText(settingsPath, MergeIntoJson(settingsPath));
        }

        [NotNull]
        private string MergeIntoJson([NotNull] string settingsPath)
        {
            JsonObject root;
            if (File.Exists(settingsPath))
            {
                try
                {
                    root = JsonNode.Parse(File.ReadAllText(settingsPath))?.AsObject()
                        ?? new JsonObject();
                }
                catch
                {
                    root = new JsonObject();
                }
            }
            else
            {
                root = new JsonObject();
            }

            root["managedDeploymentEnabled"] = ManagedDeploymentEnabled;
            if (string.IsNullOrWhiteSpace(ActiveProfileName))
            {
                root.Remove("activeProfileName");
            }
            else
            {
                root["activeProfileName"] = ActiveProfileName;
            }

            return root.ToJsonString(s_jsonOptions);
        }
    }
}
