// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Utility;

namespace ModSync.Core.Services.Deployment
{
    /// <summary>
    /// Vortex/MO2-style non-destructive deployment engine. Deploys an
    /// already-staged per-component folder tree into the game directory via
    /// hardlinks (automatic fallback to copy), records exactly what was deployed
    /// in a per-component <see cref="DeploymentManifest"/>, and uninstalls a
    /// component by removing exactly (and only) what its manifest lists,
    /// restoring any displaced game files from backup.
    ///
    /// Internal code legitimately works with absolute paths; the
    /// &lt;&lt;modDirectory&gt;&gt;/&lt;&lt;kotorDirectory&gt;&gt; placeholder
    /// rules apply to TOML instruction definitions only.
    /// </summary>
    public class DeploymentService
    {
        private const string ManifestsFolderName = "manifests";
        private const string BackupsFolderName = "backups";

        private readonly string _gameDirectory;
        private readonly string _stagingRoot;
        private readonly string _manifestRoot;
        private readonly string _manifestsDirectory;
        private readonly string _backupsDirectory;

        public DeploymentService([NotNull] string gameDirectory, [NotNull] string stagingRoot, [NotNull] string manifestRoot)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                throw new ArgumentNullException(nameof(gameDirectory));
            }

            if (string.IsNullOrWhiteSpace(stagingRoot))
            {
                throw new ArgumentNullException(nameof(stagingRoot));
            }

            if (string.IsNullOrWhiteSpace(manifestRoot))
            {
                throw new ArgumentNullException(nameof(manifestRoot));
            }

            _gameDirectory = Path.GetFullPath(gameDirectory);
            _stagingRoot = Path.GetFullPath(stagingRoot);
            _manifestRoot = Path.GetFullPath(manifestRoot);
            _manifestsDirectory = Path.Combine(_manifestRoot, ManifestsFolderName);
            _backupsDirectory = Path.Combine(_manifestRoot, BackupsFolderName);

            Directory.CreateDirectory(_manifestsDirectory);
            Directory.CreateDirectory(_backupsDirectory);
        }

        [NotNull]
        public string GameDirectory => _gameDirectory;

        [NotNull]
        public string StagingRoot => _stagingRoot;

        [NotNull]
        public string ManifestRoot => _manifestRoot;

        /// <summary>
        /// Deploys every file under <paramref name="stagedDirectory"/> into the game
        /// directory at the same relative path. Each file is hardlinked when the
        /// filesystem supports it, otherwise copied; the method used is recorded per
        /// entry. Pre-existing destination files are backed up under the per-component
        /// backup area before being overwritten. The resulting manifest is persisted
        /// atomically and returned.
        /// </summary>
        [ItemNotNull]
        public async Task<DeploymentManifest> DeployComponentAsync(
            Guid componentGuid,
            [CanBeNull] string componentName,
            [NotNull] string stagedDirectory,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(stagedDirectory))
            {
                throw new ArgumentNullException(nameof(stagedDirectory));
            }

            string stagedRoot = Path.GetFullPath(stagedDirectory);
            if (!Directory.Exists(stagedRoot))
            {
                throw new DirectoryNotFoundException($"Staged directory does not exist: {stagedRoot}");
            }

            Dictionary<string, string> pathsDeployedByOthers = BuildForeignPathIndex(componentGuid);

            var manifest = new DeploymentManifest
            {
                ComponentGuid = componentGuid,
                ComponentName = componentName,
                DeployedUtc = DateTime.UtcNow,
            };

            string componentBackupDirectory = GetComponentBackupDirectory(componentGuid);

            foreach (string stagedFile in Directory.EnumerateFiles(stagedRoot, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relativePath = NormalizeRelativePath(NetFrameworkCompatibility.GetRelativePath(stagedRoot, stagedFile));
                string destinationPath = Path.Combine(_gameDirectory, DenormalizeRelativePath(relativePath));

                if (pathsDeployedByOthers.TryGetValue(relativePath, out string otherComponentName))
                {
                    Logger.LogWarning(
                        $"[Deployment] Conflict: '{relativePath}' from component '{componentName}' is already deployed by component '{otherComponentName}'. The new file will overwrite it.");
                }

                string destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!(destinationDirectory is null))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                var entry = new DeploymentManifestEntry
                {
                    RelativePath = relativePath,
                    SourceHash = await ComputeFileHashAsync(stagedFile, cancellationToken).ConfigureAwait(false),
                    Size = new FileInfo(stagedFile).Length,
                };

                if (File.Exists(destinationPath))
                {
                    string backupPath = Path.Combine(componentBackupDirectory, DenormalizeRelativePath(relativePath));
                    string backupDirectory = Path.GetDirectoryName(backupPath);
                    if (!(backupDirectory is null))
                    {
                        Directory.CreateDirectory(backupDirectory);
                    }

                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    File.Move(destinationPath, backupPath);
                    entry.OverwroteExisting = true;
                    entry.BackupRelativePath = relativePath;
                }

                if (HardLinkHelper.TryCreateHardLink(stagedFile, destinationPath))
                {
                    entry.DeploymentMethod = DeploymentMethod.Hardlink;
                }
                else
                {
                    File.Copy(stagedFile, destinationPath, overwrite: false);
                    entry.DeploymentMethod = DeploymentMethod.Copy;
                }

                manifest.Entries.Add(entry);
            }

            await SaveManifestAtomicAsync(manifest, cancellationToken).ConfigureAwait(false);

            await Logger.LogAsync(
                $"[Deployment] Deployed component '{componentName}' ({componentGuid}): {manifest.Entries.Count} files " +
                $"({manifest.Entries.Count(e => e.DeploymentMethod == DeploymentMethod.Hardlink)} hardlinked, " +
                $"{manifest.Entries.Count(e => e.DeploymentMethod == DeploymentMethod.Copy)} copied, " +
                $"{manifest.Entries.Count(e => e.OverwroteExisting)} overwrote existing files).").ConfigureAwait(false);

            return manifest;
        }

        /// <summary>
        /// Records files that already exist in the game directory (e.g. HoloPatcher
        /// outputs) into the component's deployment manifest so managed uninstall
        /// can remove them. Merges with any existing staged-deploy manifest.
        /// Does not restore pre-patcher bytes for in-place overwrites in this slice —
        /// uninstall deletes matching hashes only.
        /// </summary>
        [ItemNotNull]
        public async Task<DeploymentManifest> RecordLiveGameFilesAsync(
            Guid componentGuid,
            [CanBeNull] string componentName,
            [NotNull] IEnumerable<string> relativePaths,
            CancellationToken cancellationToken = default)
        {
            if (relativePaths is null)
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }

            DeploymentManifest manifest;
            if (TryGetManifest(componentGuid, out DeploymentManifest existing) && existing != null)
            {
                manifest = existing;
                if (!string.IsNullOrWhiteSpace(componentName))
                {
                    manifest.ComponentName = componentName;
                }

                manifest.DeployedUtc = DateTime.UtcNow;
            }
            else
            {
                manifest = new DeploymentManifest
                {
                    ComponentGuid = componentGuid,
                    ComponentName = componentName,
                    DeployedUtc = DateTime.UtcNow,
                };
            }

            var existingPaths = new HashSet<string>(
                manifest.Entries
                    .Where(e => !string.IsNullOrWhiteSpace(e.RelativePath))
                    .Select(e => e.RelativePath),
                StringComparer.OrdinalIgnoreCase);

            int recorded = 0;
            foreach (string rawRelative in relativePaths.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relativePath = NormalizeRelativePath(rawRelative ?? string.Empty);
                if (string.IsNullOrWhiteSpace(relativePath) || existingPaths.Contains(relativePath))
                {
                    continue;
                }

                if (!TryResolveConfinedGamePath(relativePath, out string gamePath) || !File.Exists(gamePath))
                {
                    continue;
                }

                var entry = new DeploymentManifestEntry
                {
                    RelativePath = relativePath,
                    SourceHash = await ComputeFileHashAsync(gamePath, cancellationToken).ConfigureAwait(false),
                    Size = new FileInfo(gamePath).Length,
                    DeploymentMethod = DeploymentMethod.Copy,
                    OverwroteExisting = false,
                };

                manifest.Entries.Add(entry);
                _ = existingPaths.Add(relativePath);
                recorded++;
            }

            if (recorded > 0 || !TryGetManifest(componentGuid, out _))
            {
                await SaveManifestAtomicAsync(manifest, cancellationToken).ConfigureAwait(false);
            }

            await Logger.LogAsync(
                $"[Deployment] Recorded {recorded} live game file(s) for component '{componentName}' ({componentGuid}) " +
                $"(patcher/provenance; manifest now has {manifest.Entries.Count} entries).").ConfigureAwait(false);

            return manifest;
        }

        /// <summary>
        /// Builds a relative-path → SHA-256 index of files under the game directory
        /// for patcher provenance diffs. Skips empty trees.
        /// </summary>
        [NotNull]
        public async Task<Dictionary<string, string>> CaptureGameFileHashIndexAsync(
            CancellationToken cancellationToken = default)
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(_gameDirectory))
            {
                return index;
            }

            foreach (string fullPath in Directory.EnumerateFiles(_gameDirectory, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string relative = NormalizeRelativePath(
                    NetFrameworkCompatibility.GetRelativePath(_gameDirectory, fullPath));
                if (string.IsNullOrWhiteSpace(relative))
                {
                    continue;
                }

                index[relative] = await ComputeFileHashAsync(fullPath, cancellationToken).ConfigureAwait(false);
            }

            return index;
        }

        /// <summary>
        /// Returns relative paths that were added or whose content hash changed versus
        /// <paramref name="beforeIndex"/>.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public async Task<List<string>> DiffGameFileHashIndexAsync(
            [NotNull] IReadOnlyDictionary<string, string> beforeIndex,
            CancellationToken cancellationToken = default)
        {
            if (beforeIndex is null)
            {
                throw new ArgumentNullException(nameof(beforeIndex));
            }

            Dictionary<string, string> after = await CaptureGameFileHashIndexAsync(cancellationToken).ConfigureAwait(false);
            var changed = new List<string>();
            foreach (KeyValuePair<string, string> kvp in after)
            {
                if (!beforeIndex.TryGetValue(kvp.Key, out string beforeHash)
                    || !string.Equals(beforeHash, kvp.Value, StringComparison.OrdinalIgnoreCase))
                {
                    changed.Add(kvp.Key);
                }
            }

            return changed;
        }

        /// <summary>
        /// Removes exactly the files listed in the component's manifest. Files whose
        /// current content hash no longer matches the manifest hash (modified by the
        /// user or another mod) are skipped with a warning. Displaced backups are
        /// restored, empty directories left behind are pruned, and the manifest is
        /// deleted. Returns false when no manifest exists for the component.
        /// </summary>
        public async Task<bool> UninstallComponentAsync(Guid componentGuid, CancellationToken cancellationToken = default)
        {
            if (!TryGetManifest(componentGuid, out DeploymentManifest manifest))
            {
                Logger.LogWarning($"[Deployment] No deployment manifest found for component {componentGuid}; nothing to uninstall.");
                return false;
            }

            string componentBackupDirectory = GetComponentBackupDirectory(componentGuid);

            foreach (DeploymentManifestEntry entry in manifest.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(entry.RelativePath))
                {
                    continue;
                }

                if (!TryResolveConfinedGamePath(entry.RelativePath, out string destinationPath))
                {
                    Logger.LogWarning(
                        $"[Deployment] Skipping unsafe relative path '{entry.RelativePath}' during uninstall of " +
                        $"'{manifest.ComponentName}' ({componentGuid}).");
                    continue;
                }

                bool removed = false;

                if (File.Exists(destinationPath))
                {
                    string currentHash = await ComputeFileHashAsync(destinationPath, cancellationToken).ConfigureAwait(false);
                    if (string.Equals(currentHash, entry.SourceHash, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(destinationPath);
                        removed = true;
                    }
                    else
                    {
                        Logger.LogWarning(
                            $"[Deployment] Skipping '{entry.RelativePath}' during uninstall of '{manifest.ComponentName}' ({componentGuid}): " +
                            "file was modified after deployment (hash mismatch).");
                    }
                }
                else
                {
                    removed = true;
                }

                if (entry.OverwroteExisting && !string.IsNullOrWhiteSpace(entry.BackupRelativePath))
                {
                    if (!TryResolveConfinedBackupPath(
                            componentBackupDirectory,
                            entry.BackupRelativePath,
                            out string backupPath))
                    {
                        Logger.LogWarning(
                            $"[Deployment] Skipping unsafe backup path '{entry.BackupRelativePath}' during uninstall of " +
                            $"'{manifest.ComponentName}' ({componentGuid}).");
                    }
                    else if (File.Exists(backupPath))
                    {
                        if (removed)
                        {
                            string destinationDirectory = Path.GetDirectoryName(destinationPath);
                            if (!(destinationDirectory is null))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }

                            File.Move(backupPath, destinationPath);
                        }
                        else
                        {
                            Logger.LogWarning(
                                $"[Deployment] Backup for '{entry.RelativePath}' was not restored because the deployed file was " +
                                $"skipped; the backup remains at '{backupPath}'.");
                        }
                    }
                }

                if (removed)
                {
                    PruneEmptyDirectories(Path.GetDirectoryName(destinationPath));
                }
            }

            File.Delete(GetManifestPath(componentGuid));
            DeleteDirectoryIfEmptyRecursive(componentBackupDirectory);

            await Logger.LogAsync(
                $"[Deployment] Uninstalled component '{manifest.ComponentName}' ({componentGuid}).").ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// MO2-style purge: uninstalls every deployed component, newest deployment first.
        /// </summary>
        public async Task PurgeAsync(CancellationToken cancellationToken = default)
        {
            List<DeploymentManifest> manifests = GetDeployedComponents()
                .OrderByDescending(m => m.DeployedUtc)
                .ToList();

            await Logger.LogAsync($"[Deployment] Purging {manifests.Count} deployed component(s)...").ConfigureAwait(false);

            foreach (DeploymentManifest manifest in manifests)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UninstallComponentAsync(manifest.ComponentGuid, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Enumerates all persisted deployment manifests, ordered oldest first.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<DeploymentManifest> GetDeployedComponents()
        {
            var manifests = new List<DeploymentManifest>();
            if (!Directory.Exists(_manifestsDirectory))
            {
                return manifests;
            }

            foreach (string manifestFile in Directory.EnumerateFiles(_manifestsDirectory, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    DeploymentManifest manifest = DeploymentManifest.FromJson(File.ReadAllText(manifestFile));
                    if (!(manifest is null))
                    {
                        manifests.Add(manifest);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"[Deployment] Failed to read deployment manifest '{manifestFile}'.", ex);
                }
            }

            return manifests.OrderBy(m => m.DeployedUtc).ToList();
        }

        /// <summary>
        /// Loads the persisted manifest for a component, if one exists.
        /// </summary>
        public bool TryGetManifest(Guid componentGuid, [CanBeNull] out DeploymentManifest manifest)
        {
            manifest = null;
            string manifestPath = GetManifestPath(componentGuid);
            if (!File.Exists(manifestPath))
            {
                return false;
            }

            try
            {
                manifest = DeploymentManifest.FromJson(File.ReadAllText(manifestPath));
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[Deployment] Failed to read deployment manifest '{manifestPath}'.", ex);
                return false;
            }

            return !(manifest is null);
        }

        [NotNull]
        private string GetManifestPath(Guid componentGuid) =>
            Path.Combine(_manifestsDirectory, componentGuid.ToString("D") + ".json");

        [NotNull]
        private string GetComponentBackupDirectory(Guid componentGuid) =>
            Path.Combine(_backupsDirectory, componentGuid.ToString("D"));

        /// <summary>
        /// Maps every relative path deployed by components other than
        /// <paramref name="componentGuid"/> to the owning component's name, for
        /// conflict detection during deployment.
        /// </summary>
        [NotNull]
        private Dictionary<string, string> BuildForeignPathIndex(Guid componentGuid)
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DeploymentManifest manifest in GetDeployedComponents())
            {
                if (manifest.ComponentGuid == componentGuid)
                {
                    continue;
                }

                foreach (DeploymentManifestEntry entry in manifest.Entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.RelativePath) && !index.ContainsKey(entry.RelativePath))
                    {
                        index[entry.RelativePath] = manifest.ComponentName ?? manifest.ComponentGuid.ToString("D");
                    }
                }
            }

            return index;
        }

        private async Task SaveManifestAtomicAsync([NotNull] DeploymentManifest manifest, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_manifestsDirectory);

            string manifestPath = GetManifestPath(manifest.ComponentGuid);
            string tempPath = manifestPath + ".tmp";

            await NetFrameworkCompatibility.WriteAllTextAsync(tempPath, manifest.ToJson(), cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }

            File.Move(tempPath, manifestPath);
        }

        [ItemNotNull]
        private static async Task<string> ComputeFileHashAsync([NotNull] string filePath, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var sha256 = SHA256.Create())
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
                }
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes empty directories starting at <paramref name="directory"/> and
        /// walking up toward (but never including or escaping) the game directory.
        /// </summary>
        private void PruneEmptyDirectories([CanBeNull] string directory)
        {
            string gameRoot = _gameDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string current = directory;
            while (!string.IsNullOrEmpty(current))
            {
                string normalized = Path.GetFullPath(current).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (string.Equals(normalized, gameRoot, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!normalized.StartsWith(gameRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (!Directory.Exists(normalized)
                    || Directory.EnumerateFileSystemEntries(normalized).Any())
                {
                    return;
                }

                Directory.Delete(normalized);
                current = Path.GetDirectoryName(normalized);
            }
        }

        private static void DeleteDirectoryIfEmptyRecursive([CanBeNull] string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return;
            }

            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                DeleteDirectoryIfEmptyRecursive(subDirectory);
            }

            if (!Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }

        [NotNull]
        private static string NormalizeRelativePath([NotNull] string relativePath) =>
            relativePath.Replace('\\', '/');

        [NotNull]
        private static string DenormalizeRelativePath([NotNull] string relativePath) =>
            relativePath.Replace('/', Path.DirectorySeparatorChar);

        /// <summary>
        /// Resolves a manifest relative path under <paramref name="root"/>, rejecting
        /// rooted paths, <c>..</c> segments, and any result that escapes the root.
        /// </summary>
        private static bool TryResolveConfinedPath(
            [NotNull] string root,
            [NotNull] string relativePath,
            [CanBeNull] out string absolutePath)
        {
            absolutePath = null;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            string denormalized = DenormalizeRelativePath(relativePath.Trim());
            if (Path.IsPathRooted(denormalized))
            {
                return false;
            }

            string[] segments = denormalized.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(segment => segment == ".."))
            {
                return false;
            }

            string candidate = Path.GetFullPath(Path.Combine(root, denormalized));
            string rootFull = Path.GetFullPath(root);
            if (!candidate.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(candidate, rootFull, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            absolutePath = candidate;
            return true;
        }

        private bool TryResolveConfinedGamePath(
            [NotNull] string relativePath,
            [CanBeNull] out string absolutePath) =>
            TryResolveConfinedPath(_gameDirectory, relativePath, out absolutePath);

        private static bool TryResolveConfinedBackupPath(
            [NotNull] string backupRoot,
            [NotNull] string relativePath,
            [CanBeNull] out string absolutePath) =>
            TryResolveConfinedPath(backupRoot, relativePath, out absolutePath);
    }
}
