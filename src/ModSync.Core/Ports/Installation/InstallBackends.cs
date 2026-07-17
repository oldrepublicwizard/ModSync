// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.Deployment;

namespace ModSync.Core.Ports.Installation
{
    /// <summary>
    /// Default install path: instructions mutate the game directory directly.
    /// Deploy/uninstall/purge are intentional no-ops so callers can share one API.
    /// </summary>
    public sealed class ClassicInstructionInstallBackend : IInstallBackend
    {
        public static ClassicInstructionInstallBackend Instance { get; } = new ClassicInstructionInstallBackend();

        private ClassicInstructionInstallBackend()
        {
        }

        public InstallBackendKind Kind => InstallBackendKind.ClassicInstructions;

        public Task<DeploymentManifest> DeployComponentAsync(
            Guid componentGuid,
            string componentName,
            string stagedDirectory,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DeploymentManifest>(null);
        }

        public Task UninstallComponentAsync(Guid componentGuid, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task PurgeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>Managed deployment adapter over <see cref="DeploymentService"/>.</summary>
    public sealed class ManagedDeploymentInstallBackend : IInstallBackend
    {
        [NotNull]
        private readonly DeploymentService _deploymentService;

        public ManagedDeploymentInstallBackend([NotNull] DeploymentService deploymentService)
        {
            _deploymentService = deploymentService ?? throw new ArgumentNullException(nameof(deploymentService));
        }

        public ManagedDeploymentInstallBackend(
            [NotNull] string gameDirectory,
            [NotNull] string stagingRoot,
            [NotNull] string manifestRoot)
            : this(new DeploymentService(gameDirectory, stagingRoot, manifestRoot))
        {
        }

        public InstallBackendKind Kind => InstallBackendKind.ManagedDeployment;

        [NotNull]
        public DeploymentService DeploymentService => _deploymentService;

        public Task<DeploymentManifest> DeployComponentAsync(
            Guid componentGuid,
            string componentName,
            string stagedDirectory,
            CancellationToken cancellationToken = default)
        {
            return _deploymentService.DeployComponentAsync(componentGuid, componentName, stagedDirectory, cancellationToken);
        }

        public Task UninstallComponentAsync(Guid componentGuid, CancellationToken cancellationToken = default)
        {
            return _deploymentService.UninstallComponentAsync(componentGuid, cancellationToken);
        }

        public Task PurgeAsync(CancellationToken cancellationToken = default)
        {
            return _deploymentService.PurgeAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Selects managed deployment only when enabled and all absolute roots are provided;
    /// otherwise returns classic (default, non-breaking).
    /// </summary>
    public sealed class InstallBackendSelector : IInstallBackendSelector
    {
        public static InstallBackendSelector Instance { get; } = new InstallBackendSelector();

        public IInstallBackend Select(
            bool managedDeploymentEnabled,
            string gameDirectory,
            string stagingRoot,
            string manifestRoot)
        {
            if (!managedDeploymentEnabled
                || string.IsNullOrWhiteSpace(gameDirectory)
                || string.IsNullOrWhiteSpace(stagingRoot)
                || string.IsNullOrWhiteSpace(manifestRoot))
            {
                return ClassicInstructionInstallBackend.Instance;
            }

            return new ManagedDeploymentInstallBackend(gameDirectory, stagingRoot, manifestRoot);
        }
    }
}
