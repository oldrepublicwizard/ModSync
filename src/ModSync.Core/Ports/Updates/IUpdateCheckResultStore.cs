// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

using ModSync.Core.Services;

using Newtonsoft.Json;

namespace ModSync.Core.Ports.Updates
{
    /// <summary>Persisted snapshot of a Nexus update-check run.</summary>
    public sealed class PersistedUpdateCheckSnapshot
    {
        public DateTime CheckedUtc { get; set; }

        public int CheckedCount { get; set; }

        public int SkippedCount { get; set; }

        public bool RateLimitReached { get; set; }

        [NotNull]
        [ItemNotNull]
        public List<ModUpdateInfo> UpdatesFound { get; set; } = new List<ModUpdateInfo>();

        [NotNull]
        [ItemNotNull]
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Stores the last Nexus update-check summary so GUI/CLI can share results
    /// without keeping them only in memory (Vortex/MO2 parity living-plan gap).
    /// </summary>
    public interface IUpdateCheckResultStore
    {
        void Save([NotNull] ModUpdateCheckResult result, DateTime? checkedUtc = null);

        [CanBeNull]
        PersistedUpdateCheckSnapshot Load();
    }

    /// <summary>JSON file store under <c>{storageDirectory}/update-check-last.json</c>.</summary>
    public sealed class JsonUpdateCheckResultStore : IUpdateCheckResultStore
    {
        private const string FileName = "update-check-last.json";

        [NotNull]
        private static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
        };

        [NotNull]
        private readonly string _filePath;

        public JsonUpdateCheckResultStore([NotNull] string storageDirectory)
        {
            if (string.IsNullOrWhiteSpace(storageDirectory))
            {
                throw new ArgumentException("Storage directory cannot be null or whitespace.", nameof(storageDirectory));
            }

            _filePath = Path.Combine(storageDirectory, FileName);
        }

        [NotNull]
        public string FilePath => _filePath;

        public void Save(ModUpdateCheckResult result, DateTime? checkedUtc = null)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var snapshot = new PersistedUpdateCheckSnapshot
            {
                CheckedUtc = checkedUtc ?? DateTime.UtcNow,
                CheckedCount = result.CheckedCount,
                SkippedCount = result.SkippedCount,
                RateLimitReached = result.RateLimitReached,
                UpdatesFound = new List<ModUpdateInfo>(result.UpdatesFound),
                Errors = new List<string>(result.Errors),
            };

            string json = JsonConvert.SerializeObject(snapshot, s_jsonSettings);
            string tempPath = _filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(tempPath, json);
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }

                File.Move(tempPath, _filePath);
            }
            catch
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Best-effort cleanup.
                }

                throw;
            }
        }

        public PersistedUpdateCheckSnapshot Load()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<PersistedUpdateCheckSnapshot>(
                    File.ReadAllText(_filePath),
                    s_jsonSettings);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[JsonUpdateCheckResultStore] Failed to read '{_filePath}': {ex.Message}");
                return null;
            }
        }
    }
}
