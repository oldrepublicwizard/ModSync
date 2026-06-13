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

using ModSync.Core.Services.Deployment;
using ModSync.Core.Services.Profiles;
using ModSync.Core.Services.Settings;
using ModSync.Core.Utility;

namespace ModSync.Core.Services.Installation
{
    /// <summary>
    /// Per-install session for opt-in managed deployment: staging redirects and
    /// post-component deploy via <see cref="DeploymentService"/>.
    /// </summary>
    public sealed class ManagedInstallSession
    {
        [NotNull]
        private readonly DeploymentService _deploymentService;

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
            [NotNull] string gameDirectory,
            [NotNull] string stagingRoot,
            [NotNull] string manifestRoot)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                throw new ArgumentException("Profile name cannot be null or whitespace.", nameof(profileName));
            }

            ProfileName = profileName.Trim();
            _gameDirectory = Path.GetFullPath(gameDirectory);
            _stagingRoot = Path.GetFullPath(stagingRoot);
            _deploymentService = new DeploymentService(_gameDirectory, _stagingRoot, manifestRoot);
        }

        [NotNull]
        public string ProfileName { get; }

        [CanBeNull]
        public static ManagedInstallSession Current { get; set; }

        [NotNull]
        public static string MissingActiveProfileMessage { get; } =
            "Managed deployment is enabled but no active profile is loaded. "
            + "Open Profiles, activate a profile, then try again.";

        [CanBeNull]
        public static ManagedInstallSession TryCreate(
            [NotNull] ModSyncSettings settings,
            [NotNull] ProfileService profileService,
            [CanBeNull] string profileOverride = null)
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

            Profile profile = profileService.LoadProfile(profileName)
                ?? throw new InvalidOperationException(
                    $"Managed deployment requires profile '{profileName}', but it was not found on disk.");

            DirectoryInfo gameDirectory = MainConfig.DestinationPath
                ?? throw new InvalidOperationException("DestinationPath must be set before installing.");

            string artifactDirectory = profileService.GetProfileArtifactDirectory(profileName);
            string stagingRoot = Path.Combine(artifactDirectory, "staging");
            string manifestRoot = Path.Combine(artifactDirectory, "deployment");
            Directory.CreateDirectory(stagingRoot);
            Directory.CreateDirectory(manifestRoot);

            return new ManagedInstallSession(profileName, gameDirectory.FullName, stagingRoot, manifestRoot);
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

            if (!instruction.TryGetResolvedDestinationFullName(out string destinationFullName)
                || !IsPathUnderGameDirectory(destinationFullName))
            {
                return;
            }

            string stagingDestination = MapGamePathToStaging(componentGuid, destinationFullName);
            instruction.RedirectResolvedDestination(stagingDestination);
            Directory.CreateDirectory(stagingDestination);
            _ = _stagedComponentGuids.Add(componentGuid);
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
                await _deploymentService.DeployComponentAsync(
                    component.Guid,
                    component.Name,
                    stagedDirectory,
                    cancellationToken).ConfigureAwait(false);
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
