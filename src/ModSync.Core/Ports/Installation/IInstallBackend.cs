// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.Deployment;

namespace ModSync.Core.Ports.Installation
{
    /// <summary>Which install execution strategy is active.</summary>
    public enum InstallBackendKind
    {
        /// <summary>Classic instruction path: Extract/Move/Copy/Rename write directly into the game directory.</summary>
        ClassicInstructions = 0,

        /// <summary>Vortex/MO2-style staging + hardlink/copy deploy via <see cref="DeploymentService"/>.</summary>
        ManagedDeployment = 1,
    }

    /// <summary>
    /// Install backend port. Classic mode is a no-op façade (instructions already
    /// write to the game tree). Managed mode exposes deploy/uninstall/purge.
    /// </summary>
    public interface IInstallBackend
    {
        InstallBackendKind Kind { get; }

        /// <summary>
        /// Deploys a staged component tree. Classic backends return null (no-op).
        /// Managed backends persist a <see cref="DeploymentManifest"/>.
        /// </summary>
        [ItemCanBeNull]
        Task<DeploymentManifest> DeployComponentAsync(
            System.Guid componentGuid,
            [CanBeNull] string componentName,
            [NotNull] string stagedDirectory,
            CancellationToken cancellationToken = default);

        Task UninstallComponentAsync(System.Guid componentGuid, CancellationToken cancellationToken = default);

        Task PurgeAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Selects classic vs managed backend from settings / session context.</summary>
    public interface IInstallBackendSelector
    {
        [NotNull]
        IInstallBackend Select(
            bool managedDeploymentEnabled,
            [CanBeNull] string gameDirectory,
            [CanBeNull] string stagingRoot,
            [CanBeNull] string manifestRoot);
    }
}
