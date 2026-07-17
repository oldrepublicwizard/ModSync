// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Ports.Installation;
using ModSync.Core.Ports.Profiles;
using ModSync.Core.Services.Deployment;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Settings;
using ModSync.Core.Utility;

namespace ModSync.Core.Services.Installation
{
    /// <summary>
    /// Per-install session for opt-in managed deployment: staging redirects and
    /// post-component deploy via <see cref="IInstallBackend"/> (managed adapter).
    /// Classic installs leave <see cref="Current"/> null so instruction paths are unchanged.
    /// </summary>
    public sealed class ManagedInstallSession
    {
        [NotNull]
        private readonly IInstallBackend _installBackend;

        [NotNull]
        private readonly string _gameDirectory;

        [NotNull]
        private readonly string _stagingRoot;

        [NotNull]
        private readonly HashSet<Guid> _stagedComponentGuids = new HashSet<Guid>();

        [NotNull]
        private readonly HashSet<string> _patcherComponentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ManagedInstallSession(
            [NotNull] string profileName,
            [NotNull] IInstallBackend installBackend,
            [NotNull] string gameDirectory,
            [NotNull] string stagingRoot)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(profileName));
            }

            if (installBackend is null)
            {
                throw new ArgumentNullException(nameof(installBackend));
            }

            if (installBackend.Kind != InstallBackendKind.ManagedDeployment)
            {
                throw new ArgumentException(
                    "ManagedInstallSession requires a managed install backend.",
                    nameof(installBackend));
            }

            ProfileName = profileName.Trim();
            _installBackend = installBackend;
            _gameDirectory = Path.GetFullPath(gameDirectory);
            _stagingRoot = Path.GetFullPath(stagingRoot);
        }

        /// <summary>
        /// Convenience constructor that selects the managed backend via
        /// <see cref="InstallBackendSelector"/> (expansion seam for future backends).
        /// </summary>
        public ManagedInstallSession(
            [NotNull] string profileName,
            [NotNull] string gameDirectory,
            [NotNull] string stagingRoot,
            [NotNull] string manifestRoot,
            [CanBeNull] IInstallBackendSelector backendSelector = null)
            : this(
                profileName,
                SelectManagedBackend(backendSelector, gameDirectory, stagingRoot, manifestRoot),
                gameDirectory,
                stagingRoot)
        {
        }

        [NotNull]
        public string ProfileName { get; }

        [NotNull]
        public IInstallBackend InstallBackend => _installBackend;

        public InstallBackendKind BackendKind => _installBackend.Kind;

        [CanBeNull]
        public static ManagedInstallSession Current { get; set; }

        [NotNull]
        public static string MissingActiveProfileMessage { get; } =
            "Managed deployment is enabled but no active profile is loaded. "
            + "Open Profiles, activate a profile, then try again.";

        [CanBeNull]
        public static ManagedInstallSession TryCreate(
            [NotNull] ModSyncSettings settings,
            [NotNull] IProfileStore profileService,
            [CanBeNull] string profileOverride = null,
            [CanBeNull] IInstallBackendSelector backendSelector = null)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (profileService is null)
            {
                throw new ArgumentNullException(nameof(profileService));
            }

            if (!settings.ManagedDeploymentEnabled)
            {
                return null;
            }

            string profileName = !string.IsNullOrWhiteSpace(profileOverride)
                ? profileOverride.Trim()
                : settings.ActiveProfileName;

            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new InvalidOperationException(MissingActiveProfileMessage);
            }

            Profile _ = profileService.LoadProfile(profileName)
                ?? throw new InvalidOperationException(
                    $"Managed deployment requires profile '{profileName}', but it was not found on disk.");

            DirectoryInfo gameDirectory = MainConfig.DestinationPath
                ?? throw new InvalidOperationException("DestinationPath must be set before installing.");

            string artifactDirectory = profileService.GetProfileArtifactDirectory(profileName);
            string stagingRoot = Path.Combine(artifactDirectory, "staging");
            string manifestRoot = Path.Combine(artifactDirectory, "deployment");
            Directory.CreateDirectory(stagingRoot);
            Directory.CreateDirectory(manifestRoot);

            IInstallBackendSelector selector = backendSelector ?? InstallBackendSelector.Instance;
            IInstallBackend backend = selector.Select(
                managedDeploymentEnabled: true,
                gameDirectory.FullName,
                stagingRoot,
                manifestRoot);

            if (backend.Kind != InstallBackendKind.ManagedDeployment)
            {
                throw new InvalidOperationException(
                    "Managed deployment is enabled but the install backend selector returned classic mode. "
                    + "Ensure game directory, staging root, and manifest root are set.");
            }

            return new ManagedInstallSession(profileName, backend, gameDirectory.FullName, stagingRoot);
        }

        public static bool ShouldStageAction(Instruction.ActionType action)
        {
            return action == Instruction.ActionType.Extract
                || action == Instruction.ActionType.Move
                || action == Instruction.ActionType.Copy
                || action == Instruction.ActionType.Rename;
        }

        public int ManifestsWritten { get; private set; }

        public void RecordPatcherComponent([NotNull] string componentName)
        {
            if (!string.IsNullOrWhiteSpace(componentName))
            {
                _ = _patcherComponentNames.Add(componentName.Trim());
            }
        }

        public void ApplyStagingRedirect([NotNull] Instruction instruction, Guid componentGuid)
        {
            if (instruction is null)
            {
                throw new ArgumentNullException(nameof(instruction));
            }

            if (instruction.Action == Instruction.ActionType.Patcher)
            {
                RecordPatcherComponent(instruction.GetParentComponent()?.Name ?? string.Empty);
                return;
            }

            if (!ShouldStageAction(instruction.Action))
            {
                return;
            }

            bool remapped = false;

            // Follow prior staged outputs: Rename/Move/Copy sources that still resolve under the
            // live game tree must be remapped into staging or they operate on the wrong files.
            if (instruction.TryGetResolvedSourcePaths(out IReadOnlyList<string> sourcePaths)
                && sourcePaths != null
                && sourcePaths.Count > 0)
            {
                var remappedSources = new List<string>(sourcePaths.Count);
                bool anySourceRemapped = false;
                foreach (string sourcePath in sourcePaths)
                {
                    if (IsPathUnderGameDirectory(sourcePath))
                    {
                        remappedSources.Add(MapGamePathToStaging(componentGuid, sourcePath));
                        anySourceRemapped = true;
                    }
                    else
                    {
                        remappedSources.Add(sourcePath);
                    }
                }

                if (anySourceRemapped)
                {
                    instruction.RedirectResolvedSources(remappedSources);
                    remapped = true;
                }
            }

            if (instruction.TryGetResolvedDestinationFullName(out string destinationFullName)
                && IsPathUnderGameDirectory(destinationFullName))
            {
                string stagingDestination = MapGamePathToStaging(componentGuid, destinationFullName);
                instruction.RedirectResolvedDestination(stagingDestination);
                Directory.CreateDirectory(stagingDestination);
                remapped = true;
            }

            if (remapped)
            {
                _ = _stagedComponentGuids.Add(componentGuid);
            }
        }

        public bool IsPathUnderGameDirectory([NotNull] string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            string fullPath = Path.GetFullPath(absolutePath);
            string relative = NetFrameworkCompatibility.GetRelativePath(_gameDirectory, fullPath);
            if (string.IsNullOrEmpty(relative) || relative == ".")
            {
                return true;
            }

            return !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !string.Equals(relative, "..", StringComparison.Ordinal)
                && !relative.StartsWith("../", StringComparison.Ordinal)
                && !relative.StartsWith("..\\", StringComparison.Ordinal);
        }

        [NotNull]
        public string MapGamePathToStaging(Guid componentGuid, [NotNull] string gameAbsolutePath)
        {
            string relative = NormalizeRelativePath(NetFrameworkCompatibility.GetRelativePath(_gameDirectory, Path.GetFullPath(gameAbsolutePath)));
            return Path.Combine(_stagingRoot, componentGuid.ToString(), DenormalizeRelativePath(relative));
        }

        [NotNull]
        public string GetComponentStagingDirectory(Guid componentGuid) =>
            Path.Combine(_stagingRoot, componentGuid.ToString());

        public bool ComponentHadStagedOperations(Guid componentGuid) => _stagedComponentGuids.Contains(componentGuid);

        [ItemNotNull]
        public async Task<ModComponent.InstallExitCode> DeployComponentAsync(
            [NotNull] ModComponent component,
            CancellationToken cancellationToken = default)
        {
            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (!ComponentHadStagedOperations(component.Guid))
            {
                return ModComponent.InstallExitCode.Success;
            }

            string stagedDirectory = GetComponentStagingDirectory(component.Guid);
            if (!Directory.Exists(stagedDirectory)
                || !Directory.EnumerateFileSystemEntries(stagedDirectory).Any())
            {
                return ModComponent.InstallExitCode.Success;
            }

            try
            {
                DeploymentManifest manifest = await _installBackend.DeployComponentAsync(
                    component.Guid,
                    component.Name,
                    stagedDirectory,
                    cancellationToken).ConfigureAwait(false);

                if (manifest is null)
                {
                    await Logger.LogErrorAsync(
                        $"[ManagedInstall] Managed backend returned no manifest for '{component.Name}'.").ConfigureAwait(false);
                    return ModComponent.InstallExitCode.InvalidOperation;
                }

                ManifestsWritten++;
                return ModComponent.InstallExitCode.Success;
            }
            catch (Exception ex)
            {
                await Logger.LogErrorAsync(
                    $"[ManagedInstall] Deploy failed for '{component.Name}': {ex.Message}").ConfigureAwait(false);
                return ModComponent.InstallExitCode.InvalidOperation;
            }
        }

        [NotNull]
        public ManagedInstallResult BuildResult(int manifestsWritten)
        {
            var result = new ManagedInstallResult
            {
                ManagedModeUsed = true,
                ManifestsWritten = manifestsWritten,
                ActiveProfileName = ProfileName,
            };
            CopyPatcherNamesTo(result);
            return result;
        }

        public void CopyPatcherNamesTo([NotNull] ManagedInstallResult result)
        {
            foreach (string name in _patcherComponentNames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
            {
                result.PatcherComponentNames.Add(name);
            }
        }

        [NotNull]
        private static IInstallBackend SelectManagedBackend(
            [CanBeNull] IInstallBackendSelector backendSelector,
            [NotNull] string gameDirectory,
            [NotNull] string stagingRoot,
            [NotNull] string manifestRoot)
        {
            IInstallBackendSelector selector = backendSelector ?? InstallBackendSelector.Instance;
            IInstallBackend backend = selector.Select(true, gameDirectory, stagingRoot, manifestRoot);
            if (backend.Kind != InstallBackendKind.ManagedDeployment)
            {
                throw new InvalidOperationException(
                    "Expected managed install backend but selector returned classic.");
            }

            return backend;
        }

        [NotNull]
        private static string NormalizeRelativePath([NotNull] string relativePath)
        {
            return relativePath.Replace('\\', '/').TrimStart('/');
        }

        [NotNull]
        private static string DenormalizeRelativePath([NotNull] string relativePath)
        {
            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
