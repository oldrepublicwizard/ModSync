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
            if (!TryResolveBackend(out IInstallBackend backend, out string error, out bool managedRequested))
            {
                if (managedRequested && !string.IsNullOrWhiteSpace(error))
                {
                    return new ManagedDeploymentStatus(
                        managedBackend: false,
                        deployedComponentCount: 0,
                        resolveError: error);
                }

                return new ManagedDeploymentStatus(managedBackend: false, deployedComponentCount: 0);
            }

            return ManagedDeploymentLifecycle.GetStatus(backend);
        }

        [NotNull]
        [ItemNotNull]
        public static async Task<(bool Success, string Message)> PurgeAsync(
            CancellationToken cancellationToken = default)
        {
            if (ManagedInstallSession.Current != null)
            {
                return (
                    false,
                    "Cannot purge managed deployments while a managed install is in progress. " +
                    "Wait for the install to finish, then try again.");
            }

            if (!TryResolveBackend(out IInstallBackend backend, out string error, out _))
            {
                return (false, error);
            }

            try
            {
                ManagedDeploymentPurgeResult result = await ManagedDeploymentLifecycle
                    .PurgeAsync(backend, cancellationToken)
                    .ConfigureAwait(false);
                return (true, result.FormatSummary());
            }
            catch (Exception ex)
            {
                return (false, "Purge failed: " + ex.Message);
            }
        }

        [NotNull]
        [ItemNotNull]
        public static async Task<(bool Success, string Message)> UninstallComponentAsync(
            Guid componentGuid,
            CancellationToken cancellationToken = default)
        {
            if (ManagedInstallSession.Current != null)
            {
                return (
                    false,
                    "Cannot uninstall a managed deployment while a managed install is in progress.");
            }

            if (!TryResolveBackend(out IInstallBackend backend, out string error, out _))
            {
                return (false, error);
            }

            try
            {
                if (backend is ManagedDeploymentInstallBackend managed)
                {
                    bool removed = await managed.DeploymentService
                        .UninstallComponentAsync(componentGuid, cancellationToken)
                        .ConfigureAwait(false);
                    return removed
                        ? (true, "Uninstalled managed deployment for the selected component.")
                        : (false, "No managed deployment manifest was found for that component.");
                }

                await ManagedDeploymentLifecycle
                    .UninstallComponentAsync(backend, componentGuid, cancellationToken)
                    .ConfigureAwait(false);
                return (true, "Uninstalled managed deployment for the selected component.");
            }
            catch (Exception ex)
            {
                return (false, "Uninstall failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Returns whether a deployment manifest exists for <paramref name="componentGuid"/>
        /// under the current managed profile (false in classic mode or on resolve errors).
        /// </summary>
        public static bool IsComponentDeployed(Guid componentGuid)
        {
            if (!TryResolveBackend(out IInstallBackend backend, out _, out _))
            {
                return false;
            }

            if (backend is ManagedDeploymentInstallBackend managed)
            {
                return managed.DeploymentService.TryGetManifest(componentGuid, out _);
            }

            return false;
        }

        private static bool TryResolveBackend(
            out IInstallBackend backend,
            out string error,
            out bool managedRequested)
        {
            backend = null;
            error = null;
            managedRequested = false;

            try
            {
                ModSyncSettings settings = ModSyncSettings.Load();
                if (!settings.ManagedDeploymentEnabled)
                {
                    error = "Managed deployment is disabled. Enable it in Settings, then try again.";
                    return false;
                }

                managedRequested = true;

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
                managedRequested = true;
                error = ex.Message;
                return false;
            }
        }
    }
}
