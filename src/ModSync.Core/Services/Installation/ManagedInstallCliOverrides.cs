// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;

using JetBrains.Annotations;

using ModSync.Core.Services.Settings;

namespace ModSync.Core.Services.Installation
{
    /// <summary>
    /// Applies CLI <c>--managed</c> / <c>--no-managed</c> / <c>--profile</c> overrides onto
    /// loaded <see cref="ModSyncSettings"/> without persisting them (process-local for the install).
    /// </summary>
    public static class ManagedInstallCliOverrides
    {
        /// <summary>
        /// Merges optional CLI overrides into <paramref name="settings"/> in place.
        /// </summary>
        /// <returns>Error message when flags conflict; otherwise <c>null</c>.</returns>
        [CanBeNull]
        public static string Apply(
            [NotNull] ModSyncSettings settings,
            bool enableManaged,
            bool disableManaged,
            [CanBeNull] string profileName)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (enableManaged && disableManaged)
            {
                return "Cannot combine --managed and --no-managed.";
            }

            if (enableManaged)
            {
                settings.ManagedDeploymentEnabled = true;
            }
            else if (disableManaged)
            {
                settings.ManagedDeploymentEnabled = false;
            }

            if (!string.IsNullOrWhiteSpace(profileName))
            {
                settings.ActiveProfileName = profileName.Trim();
            }

            if (settings.ManagedDeploymentEnabled && string.IsNullOrWhiteSpace(settings.ActiveProfileName))
            {
                return ManagedInstallSession.MissingActiveProfileMessage
                    + " Pass --profile <name> (or activate a profile in Settings).";
            }

            return null;
        }

        /// <summary>
        /// Resolves the managed-enabled override for <see cref="InstallationService"/>:
        /// <c>true</c>/<c>false</c> when CLI forces a value; <c>null</c> to keep settings.json.
        /// </summary>
        [CanBeNull]
        public static bool? ResolveManagedOverride(bool enableManaged, bool disableManaged)
        {
            if (enableManaged && disableManaged)
            {
                return null;
            }

            if (enableManaged)
            {
                return true;
            }

            if (disableManaged)
            {
                return false;
            }

            return null;
        }
    }
}
