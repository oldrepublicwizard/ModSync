// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Ports.Installation;
using ModSync.Core.Services.Deployment;
using ModSync.Core.Services.Installation;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Settings;

namespace ModSync.Services
{
    /// <summary>
    /// GUI-facing entry points for managed purge / deployment indicator using
    /// <see cref="ManagedInstallSession"/> and <see cref="ManagedDeploymentLifecycle"/>.
    /// </summary>
    public static class ManagedDeploymentActions
    {
        [NotNull]
        public static ManagedDeploymentStatus GetStatusOrClassic()
        {
            if (!TryResolveBackend(out IInstallBackend backend, out string error))
            {
                return new ManagedDeploymentStatus(managedBackend: false, deployedComponentCount: 0);
            }

            _ = error;
            return ManagedDeploymentLifecycle.GetStatus(backend);
        }

        [NotNull]
        [ItemNotNull]
        public static async Task<(bool Success, string Message)> PurgeAsync(
            CancellationToken cancellationToken = default)
        {
            if (!TryResolveBackend(out IInstallBackend backend, out string error))
            {
                return (false, error);
            }

            ManagedDeploymentPurgeResult result = await ManagedDeploymentLifecycle
                .PurgeAsync(backend, cancellationToken)
                .ConfigureAwait(false);
            return (true, result.FormatSummary());
        }

        [NotNull]
        [ItemNotNull]
        public static async Task<(bool Success, string Message)> UninstallComponentAsync(
            Guid componentGuid,
            CancellationToken cancellationToken = default)
        {
            if (!TryResolveBackend(out IInstallBackend backend, out string error))
            {
                return (false, error);
            }

            await ManagedDeploymentLifecycle
                .UninstallComponentAsync(backend, componentGuid, cancellationToken)
                .ConfigureAwait(false);
            return (true, "Uninstalled managed deployment for the selected component.");
        }

        private static bool TryResolveBackend(out IInstallBackend backend, out string error)
        {
            backend = null;
            error = null;

            try
            {
                ModSyncSettings settings = ModSyncSettings.Load();
                if (!settings.ManagedDeploymentEnabled)
                {
                    error = "Managed deployment is disabled. Enable it in Settings, then try again.";
                    return false;
                }

                var profileService = new ProfileService(ModSyncSettings.GetSettingsDirectory());
                ManagedInstallSession session = ManagedInstallSession.TryCreate(settings, profileService);
                if (session is null)
                {
                    error = "Managed deployment session could not be created.";
                    return false;
                }

                backend = session.InstallBackend;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
