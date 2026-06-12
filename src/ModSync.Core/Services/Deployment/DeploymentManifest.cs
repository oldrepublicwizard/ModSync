// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Newtonsoft.Json;

namespace ModSync.Core.Services.Deployment
{
    /// <summary>
    /// How a single file was materialized into the game directory.
    /// </summary>
    public enum DeploymentMethod
    {
        Hardlink = 0,
        Copy = 1,
    }

    /// <summary>
    /// One deployed file inside a <see cref="DeploymentManifest"/>.
    /// </summary>
    public class DeploymentManifestEntry
    {
        /// <summary>
        /// Path of the deployed file relative to the game directory (forward slashes).
        /// </summary>
        [CanBeNull]
        public string RelativePath { get; set; }

        /// <summary>
        /// Lowercase hex SHA-256 of the staged source file at deployment time.
        /// </summary>
        [CanBeNull]
        public string SourceHash { get; set; }

        /// <summary>
        /// Size in bytes of the staged source file at deployment time.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Method actually used for this file (hardlink, or copy fallback).
        /// </summary>
        public DeploymentMethod DeploymentMethod { get; set; }

        /// <summary>
        /// True when a file already existed at the destination and was displaced.
        /// </summary>
        public bool OverwroteExisting { get; set; }

        /// <summary>
        /// When <see cref="OverwroteExisting"/> is true, path of the displaced file's
        /// backup relative to the per-component backup directory; otherwise null.
        /// </summary>
        [CanBeNull]
        public string BackupRelativePath { get; set; }
    }

    /// <summary>
    /// Per-component record of every file a deployment materialized into the game
    /// directory. Persisted as JSON under the manifest root so a component can be
    /// uninstalled exactly (and only) by what it deployed.
    /// </summary>
    public class DeploymentManifest
    {
        public Guid ComponentGuid { get; set; }

        [CanBeNull]
        public string ComponentName { get; set; }

        /// <summary>
        /// UTC timestamp of the deployment; purge unwinds newest-first by this value.
        /// </summary>
        public DateTime DeployedUtc { get; set; }

        [NotNull]
        public List<DeploymentManifestEntry> Entries { get; set; } = new List<DeploymentManifestEntry>();

        [NotNull]
        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        [CanBeNull]
        public static DeploymentManifest FromJson([CanBeNull] string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<DeploymentManifest>(json);
        }
    }
}
