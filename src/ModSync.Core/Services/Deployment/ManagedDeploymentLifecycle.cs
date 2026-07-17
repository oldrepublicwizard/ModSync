// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Ports.Installation;

namespace ModSync.Core.Services.Deployment
{
    /// <summary>Snapshot of managed deployment state for UI indicators.</summary>
    public sealed class ManagedDeploymentStatus
    {
        public ManagedDeploymentStatus(bool managedBackend, int deployedComponentCount)
        {
            ManagedBackend = managedBackend;
            DeployedComponentCount = deployedComponentCount;
        }

        public bool ManagedBackend { get; }

        public int DeployedComponentCount { get; }

        public bool HasDeployments => DeployedComponentCount > 0;

        [NotNull]
        public string FormatIndicator()
        {
            if (!ManagedBackend)
            {
                return "Managed deployment: classic mode (no staging manifests).";
            }

            if (!HasDeployments)
            {
                return "Managed deployment: no components deployed.";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Managed deployment: {0} component(s) deployed.",
                DeployedComponentCount);
        }
    }

    /// <summary>Result of an MO2-style purge.</summary>
    public sealed class ManagedDeploymentPurgeResult
    {
        public ManagedDeploymentPurgeResult(int componentsPurged)
        {
            ComponentsPurged = componentsPurged;
        }

        public int ComponentsPurged { get; }

        [NotNull]
        public string FormatSummary()
        {
            if (ComponentsPurged <= 0)
            {
                return "Nothing to purge — no managed deployments were recorded.";
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Purged {0} managed deployment(s) (newest first).",
                ComponentsPurged);
        }
    }

    /// <summary>
    /// Headless helpers for purge / uninstall / deployment indicators over
    /// <see cref="IInstallBackend"/>.
    /// </summary>
    public static class ManagedDeploymentLifecycle
    {
        [NotNull]
        public static ManagedDeploymentStatus GetStatus([NotNull] IInstallBackend backend)
        {
            if (backend is null)
            {
                throw new ArgumentNullException(nameof(backend));
            }

            if (backend is ManagedDeploymentInstallBackend managed)
            {
                return new ManagedDeploymentStatus(
                    managedBackend: true,
                    managed.DeploymentService.GetDeployedComponents().Count);
            }

            return new ManagedDeploymentStatus(managedBackend: false, deployedComponentCount: 0);
        }

        [NotNull]
        [ItemNotNull]
        public static async Task<ManagedDeploymentPurgeResult> PurgeAsync(
            [NotNull] IInstallBackend backend,
            CancellationToken cancellationToken = default)
        {
            if (backend is null)
            {
                throw new ArgumentNullException(nameof(backend));
            }

            ManagedDeploymentStatus before = GetStatus(backend);
            await backend.PurgeAsync(cancellationToken).ConfigureAwait(false);
            return new ManagedDeploymentPurgeResult(before.DeployedComponentCount);
        }

        [ItemNotNull]
        public static Task UninstallComponentAsync(
            [NotNull] IInstallBackend backend,
            Guid componentGuid,
            CancellationToken cancellationToken = default)
        {
            if (backend is null)
            {
                throw new ArgumentNullException(nameof(backend));
            }

            return backend.UninstallComponentAsync(componentGuid, cancellationToken);
        }
    }
}
