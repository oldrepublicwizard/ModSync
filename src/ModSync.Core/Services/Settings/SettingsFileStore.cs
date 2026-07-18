// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Settings
{
    /// <summary>
    /// Generic read/write helpers for persisted <c>settings.json</c> without overwriting unrelated GUI-owned keys.
    /// </summary>
    public static class SettingsFileStore
    {
        [NotNull]
        private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        [NotNull]
        public static readonly IReadOnlyCollection<string> SensitiveKeys = new[]
        {
            "nexusModsApiKey",
        };

        [NotNull]
        public static string ResolveSettingsDirectory([CanBeNull] string settingsDirectory)
        {
            return string.IsNullOrWhiteSpace(settingsDirectory)
                ? ModSyncSettings.GetSettingsDirectory()
                : settingsDirectory.Trim();
        }

        [NotNull]
        public static string ResolveSettingsFilePath([CanBeNull] string settingsDirectory)
        {
            string directory = ResolveSettingsDirectory(settingsDirectory);
            string settingsPath = Path.Combine(directory, "settings.json");
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
        public static JsonObject LoadRoot([CanBeNull] string settingsDirectory)
        {
            string settingsPath = ResolveSettingsFilePath(settingsDirectory);
            if (!File.Exists(settingsPath))
            {
                return new JsonObject();
            }

            try
            {
                return JsonNode.Parse(File.ReadAllText(settingsPath))?.AsObject() ?? new JsonObject();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[SettingsFileStore] Failed to parse settings file '{settingsPath}': {ex.Message}");
                return new JsonObject();
            }
        }

        public static void SaveRoot([CanBeNull] string settingsDirectory, [NotNull] JsonObject root)
        {
            if (root is null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            string settingsPath = ResolveSettingsFilePath(settingsDirectory);
            string directory = Path.GetDirectoryName(settingsPath)
                ?? throw new InvalidOperationException($"Could not determine directory for settings path '{settingsPath}'.");
            if (!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            File.WriteAllText(settingsPath, root.ToJsonString(s_jsonOptions));
            RestrictOwnerPermissions(settingsPath);
        }

        [NotNull]
        public static IReadOnlyList<string> ListKeys([NotNull] JsonObject root)
        {
            return root.Select(pair => pair.Key).OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static bool TryGetValue([NotNull] JsonObject root, [NotNull] string key, out JsonNode value)
        {
            if (root.TryGetPropertyValue(key, out JsonNode node))
            {
                value = node?.DeepClone();
                return true;
            }

            value = null;
            return false;
        }

        public static void SetValue([NotNull] JsonObject root, [NotNull] string key, [CanBeNull] string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                root.Remove(key);
                return;
            }

            root[key] = ParseCliValue(rawValue);
        }

        [NotNull]
        public static JsonNode ParseCliValue([NotNull] string rawValue)
        {
            string trimmed = rawValue.Trim();
            if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(true);
            }

            if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(false);
            }

            if (string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (long.TryParse(trimmed, out long longValue))
            {
                return JsonValue.Create(longValue);
            }

            if (double.TryParse(trimmed, out double doubleValue))
            {
                return JsonValue.Create(doubleValue);
            }

            if ((trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
                || (trimmed.StartsWith("[", StringComparison.Ordinal) && trimmed.EndsWith("]", StringComparison.Ordinal)))
            {
                return JsonNode.Parse(trimmed)?.DeepClone();
            }

            return JsonValue.Create(trimmed);
        }

        [NotNull]
        public static JsonObject CloneForOutput([NotNull] JsonObject root, bool revealSecrets)
        {
            JsonObject clone = root.DeepClone()?.AsObject() ?? new JsonObject();
            if (revealSecrets)
            {
                return clone;
            }

            foreach (string sensitiveKey in SensitiveKeys)
            {
                if (clone.TryGetPropertyValue(sensitiveKey, out JsonNode sensitiveValue) && sensitiveValue != null)
                {
                    clone[sensitiveKey] = "***";
                }
            }

            return clone;
        }

        private static void RestrictOwnerPermissions([NotNull] string settingsPath)
        {
#if NET7_0_OR_GREATER
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    File.SetUnixFileMode(settingsPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"[SettingsFileStore] Could not restrict file permissions on '{settingsPath}': {ex.Message}");
                }
            }
#endif
        }
    }
}
