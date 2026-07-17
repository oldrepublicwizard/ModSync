// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ModSync.Core.Services.Deployment;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class DeploymentServiceTests
    {
        private string _testRoot;
        private string _gameDirectory;
        private string _stagingRoot;
        private string _manifestRoot;
        private DeploymentService _service;

        [SetUp]
        public void SetUp()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), "ModSyncDeploymentTests_" + Guid.NewGuid().ToString("N"));
            _gameDirectory = Path.Combine(_testRoot, "game");
            _stagingRoot = Path.Combine(_testRoot, "staging");
            _manifestRoot = Path.Combine(_testRoot, "deployment");

            Directory.CreateDirectory(_gameDirectory);
            Directory.CreateDirectory(_stagingRoot);

            _service = new DeploymentService(_gameDirectory, _stagingRoot, _manifestRoot);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(_testRoot))
                {
                    Directory.Delete(_testRoot, recursive: true);
                }
            }
            catch (IOException)
            {
                // Best-effort cleanup; leftover temp dirs are harmless.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private string CreateStagedFile(string componentFolder, string relativePath, string content)
        {
            string stagedDir = Path.Combine(_stagingRoot, componentFolder);
            string fullPath = Path.Combine(stagedDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, content);
            return Path.Combine(_stagingRoot, componentFolder);
        }

        private string GamePath(string relativePath) =>
            Path.Combine(_gameDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));

        [Test]
        public async Task DeployComponent_CreatesFilesInGameDirectory_PreferringHardlinks()
        {
            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "Override/test.txt", "hello deployment");
            CreateStagedFile("comp1", "Modules/sub/module.mod", "module data");

            DeploymentManifest manifest = await _service.DeployComponentAsync(guid, "Comp One", staged);

            Assert.That(manifest.Entries, Has.Count.EqualTo(2));
            Assert.That(File.ReadAllText(GamePath("Override/test.txt")), Is.EqualTo("hello deployment"));
            Assert.That(File.ReadAllText(GamePath("Modules/sub/module.mod")), Is.EqualTo("module data"));

            DeploymentManifestEntry entry = manifest.Entries.Single(e => e.RelativePath == "Override/test.txt");
            Assert.That(entry.SourceHash, Is.Not.Null.And.Length.EqualTo(64), "SourceHash should be a SHA-256 hex string");
            Assert.That(entry.Size, Is.EqualTo(new FileInfo(GamePath("Override/test.txt")).Length));
            Assert.That(entry.OverwroteExisting, Is.False);

            if (entry.DeploymentMethod == DeploymentMethod.Hardlink)
            {
                // Hardlinked files share an inode: appending through the staged path
                // must be visible through the game path.
                string stagedFile = Path.Combine(staged, "Override", "test.txt");
                File.AppendAllText(stagedFile, " MUTATED");
                Assert.That(
                    File.ReadAllText(GamePath("Override/test.txt")),
                    Is.EqualTo("hello deployment MUTATED"),
                    "Hardlinked deploy target should reflect writes to the staged source");
            }
            else
            {
                Assert.That(entry.DeploymentMethod, Is.EqualTo(DeploymentMethod.Copy),
                    "When hardlinking is unsupported the recorded method must be Copy");
            }
        }

        [Test]
        public async Task Manifest_RoundTripsThroughJson()
        {
            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "Override/roundtrip.txt", "round trip me");

            DeploymentManifest original = await _service.DeployComponentAsync(guid, "RoundTrip", staged);

            DeploymentManifest parsed = DeploymentManifest.FromJson(original.ToJson());
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.ComponentGuid, Is.EqualTo(original.ComponentGuid));
            Assert.That(parsed.ComponentName, Is.EqualTo(original.ComponentName));
            Assert.That(parsed.DeployedUtc, Is.EqualTo(original.DeployedUtc));
            Assert.That(parsed.Entries, Has.Count.EqualTo(original.Entries.Count));
            Assert.That(parsed.Entries[0].RelativePath, Is.EqualTo(original.Entries[0].RelativePath));
            Assert.That(parsed.Entries[0].SourceHash, Is.EqualTo(original.Entries[0].SourceHash));
            Assert.That(parsed.Entries[0].Size, Is.EqualTo(original.Entries[0].Size));
            Assert.That(parsed.Entries[0].DeploymentMethod, Is.EqualTo(original.Entries[0].DeploymentMethod));

            // And the persisted manifest is readable through the service.
            Assert.That(_service.TryGetManifest(guid, out DeploymentManifest persisted), Is.True);
            Assert.That(persisted.Entries, Has.Count.EqualTo(1));
            Assert.That(persisted.Entries[0].RelativePath, Is.EqualTo("Override/roundtrip.txt"));
        }

        [Test]
        public async Task Deploy_BacksUpExistingGameFile_AndUninstallRestoresIt()
        {
            string existingPath = GamePath("Override/existing.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(existingPath));
            File.WriteAllText(existingPath, "original game content");

            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "Override/existing.txt", "mod content");

            DeploymentManifest manifest = await _service.DeployComponentAsync(guid, "Overwriter", staged);

            DeploymentManifestEntry entry = manifest.Entries.Single();
            Assert.That(entry.OverwroteExisting, Is.True);
            Assert.That(entry.BackupRelativePath, Is.EqualTo("Override/existing.txt"));
            Assert.That(File.ReadAllText(existingPath), Is.EqualTo("mod content"));

            string backupPath = Path.Combine(
                _manifestRoot, "backups", guid.ToString("D"), "Override", "existing.txt");
            Assert.That(File.Exists(backupPath), Is.True, "Displaced game file should be backed up");
            Assert.That(File.ReadAllText(backupPath), Is.EqualTo("original game content"));

            bool uninstalled = await _service.UninstallComponentAsync(guid);

            Assert.That(uninstalled, Is.True);
            Assert.That(File.ReadAllText(existingPath), Is.EqualTo("original game content"),
                "Uninstall should restore the displaced original file");
            Assert.That(_service.TryGetManifest(guid, out _), Is.False, "Manifest should be deleted after uninstall");
        }

        [Test]
        public async Task Uninstall_SkipsUserModifiedFiles()
        {
            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "Override/modified.txt", "deployed content");

            await _service.DeployComponentAsync(guid, "Modifiable", staged);

            string gameFile = GamePath("Override/modified.txt");
            File.WriteAllText(gameFile, "user changed this file");

            bool uninstalled = await _service.UninstallComponentAsync(guid);

            Assert.That(uninstalled, Is.True);
            Assert.That(File.Exists(gameFile), Is.True, "User-modified file must not be deleted");
            Assert.That(File.ReadAllText(gameFile), Is.EqualTo("user changed this file"));
            Assert.That(_service.TryGetManifest(guid, out _), Is.False);
        }

        [Test]
        public async Task Purge_RemovesAllComponents_AndRestoresBackups()
        {
            string existingPath = GamePath("Override/shared.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(existingPath));
            File.WriteAllText(existingPath, "vanilla file");

            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            string staged1 = CreateStagedFile("comp1", "Override/shared.txt", "from comp1");
            CreateStagedFile("comp1", "Override/one.txt", "one");
            string staged2 = CreateStagedFile("comp2", "Modules/two.txt", "two");

            await _service.DeployComponentAsync(guid1, "Comp One", staged1);
            await _service.DeployComponentAsync(guid2, "Comp Two", staged2);
            Assert.That(_service.GetDeployedComponents(), Has.Count.EqualTo(2));

            await _service.PurgeAsync();

            Assert.That(_service.GetDeployedComponents(), Is.Empty);
            Assert.That(File.Exists(GamePath("Override/one.txt")), Is.False);
            Assert.That(File.Exists(GamePath("Modules/two.txt")), Is.False);
            Assert.That(File.ReadAllText(existingPath), Is.EqualTo("vanilla file"),
                "Purge should restore the displaced vanilla file");
        }

        [Test]
        public async Task Uninstall_PrunesEmptyDirectoriesItCreated()
        {
            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "StreamVoice/deep/nested/voice.wav", "audio");

            await _service.DeployComponentAsync(guid, "Nested", staged);
            Assert.That(Directory.Exists(GamePath("StreamVoice/deep/nested")), Is.True);

            await _service.UninstallComponentAsync(guid);

            Assert.That(Directory.Exists(GamePath("StreamVoice")), Is.False,
                "Empty directories created by the deployment should be pruned");
            Assert.That(Directory.Exists(_gameDirectory), Is.True,
                "The game directory itself must never be pruned");
        }

        [Test]
        public async Task Uninstall_KeepsDirectoriesContainingOtherFiles()
        {
            string unrelated = GamePath("Override/unrelated.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(unrelated));
            File.WriteAllText(unrelated, "not ours");

            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp1", "Override/ours.txt", "ours");

            await _service.DeployComponentAsync(guid, "Polite", staged);
            await _service.UninstallComponentAsync(guid);

            Assert.That(File.Exists(unrelated), Is.True);
            Assert.That(Directory.Exists(GamePath("Override")), Is.True,
                "Directories still containing files must not be pruned");
        }

        [Test]
        public async Task Deploy_SamePathFromTwoComponents_RecordsConflict()
        {
            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            string staged1 = CreateStagedFile("comp1", "Override/contested.txt", "first mod wins?");
            string staged2 = CreateStagedFile("comp2", "Override/contested.txt", "second mod wins");

            DeploymentManifest manifest1 = await _service.DeployComponentAsync(guid1, "First", staged1);
            DeploymentManifest manifest2 = await _service.DeployComponentAsync(guid2, "Second", staged2);

            Assert.That(manifest1.Entries.Single().OverwroteExisting, Is.False);

            DeploymentManifestEntry conflicting = manifest2.Entries.Single();
            Assert.That(conflicting.OverwroteExisting, Is.True,
                "Second component must record that it displaced an existing file");
            Assert.That(conflicting.BackupRelativePath, Is.EqualTo("Override/contested.txt"));
            Assert.That(File.ReadAllText(GamePath("Override/contested.txt")), Is.EqualTo("second mod wins"));

            // The conflict is observable from persisted manifests: both components
            // list the same relative path.
            var owners = _service.GetDeployedComponents()
                .Where(m => m.Entries.Any(e => e.RelativePath == "Override/contested.txt"))
                .Select(m => m.ComponentGuid)
                .ToList();
            Assert.That(owners, Is.EquivalentTo(new[] { guid1, guid2 }));

            // Uninstalling the second component restores the first component's file.
            await _service.UninstallComponentAsync(guid2);
            Assert.That(File.ReadAllText(GamePath("Override/contested.txt")), Is.EqualTo("first mod wins?"));
        }

        [Test]
        public void Constructor_RejectsMissingArguments()
        {
            Assert.That(() => new DeploymentService(null, _stagingRoot, _manifestRoot), Throws.ArgumentNullException);
            Assert.That(() => new DeploymentService(_gameDirectory, " ", _manifestRoot), Throws.ArgumentNullException);
            Assert.That(() => new DeploymentService(_gameDirectory, _stagingRoot, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Deploy_MissingStagedDirectory_Throws()
        {
            Assert.That(
                async () => await _service.DeployComponentAsync(Guid.NewGuid(), "Ghost", Path.Combine(_stagingRoot, "missing")),
                Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public async Task Uninstall_UnknownComponent_ReturnsFalse()
        {
            bool result = await _service.UninstallComponentAsync(Guid.NewGuid());
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Uninstall_SkipsPathTraversalRelativePaths()
        {
            Guid guid = Guid.NewGuid();
            string outsideFile = Path.Combine(_testRoot, "outside.txt");
            File.WriteAllText(outsideFile, "must not delete");

            string safeStaged = CreateStagedFile("safe", "Override/safe.txt", "safe");
            DeploymentManifest manifest = await _service.DeployComponentAsync(guid, "Safe", safeStaged);

            manifest.Entries.Add(new DeploymentManifestEntry
            {
                RelativePath = "../outside.txt",
                SourceHash = "deadbeef",
                Size = 1,
                DeploymentMethod = DeploymentMethod.Copy,
            });

            string manifestPath = Path.Combine(_manifestRoot, "manifests", guid.ToString("D") + ".json");
            if (!File.Exists(manifestPath))
            {
                // Locate wherever the service wrote the manifest.
                manifestPath = Directory.GetFiles(_manifestRoot, guid.ToString("D") + ".json", SearchOption.AllDirectories)
                    .Single();
            }

            File.WriteAllText(manifestPath, manifest.ToJson());

            bool uninstalled = await _service.UninstallComponentAsync(guid);

            Assert.That(uninstalled, Is.True);
            Assert.That(File.Exists(outsideFile), Is.True, "Path-traversal target must not be deleted");
            Assert.That(File.Exists(GamePath("Override/safe.txt")), Is.False);
        }

        [Test]
        public async Task RecordLiveGameFiles_ThenUninstall_RemovesPatcherWrittenFile()
        {
            Guid guid = Guid.NewGuid();
            Dictionary<string, string> before = await _service.CaptureGameFileHashIndexAsync();

            Directory.CreateDirectory(Path.Combine(_gameDirectory, "Override"));
            File.WriteAllText(GamePath("Override/patcher_out.mdl"), "holopatcher bytes");

            List<string> changed = await _service.DiffGameFileHashIndexAsync(before);
            Assert.That(changed, Does.Contain("Override/patcher_out.mdl"));

            DeploymentManifest manifest = await _service.RecordLiveGameFilesAsync(
                guid,
                "Patcher Mod",
                changed);

            Assert.That(manifest.Entries.Any(e => e.RelativePath == "Override/patcher_out.mdl"), Is.True);
            Assert.That(File.Exists(GamePath("Override/patcher_out.mdl")), Is.True);

            bool uninstalled = await _service.UninstallComponentAsync(guid);
            Assert.That(uninstalled, Is.True);
            Assert.That(File.Exists(GamePath("Override/patcher_out.mdl")), Is.False);
        }

        [Test]
        public async Task RecordLiveGameFiles_MergesWithExistingStagedManifest()
        {
            Guid guid = Guid.NewGuid();
            string staged = CreateStagedFile("comp", "Override/staged.txt", "staged");
            await _service.DeployComponentAsync(guid, "Hybrid", staged);

            File.WriteAllText(GamePath("Override/live.txt"), "live patcher");
            DeploymentManifest merged = await _service.RecordLiveGameFilesAsync(
                guid,
                "Hybrid",
                new[] { "Override/live.txt" });

            Assert.That(merged.Entries.Select(e => e.RelativePath), Does.Contain("Override/staged.txt"));
            Assert.That(merged.Entries.Select(e => e.RelativePath), Does.Contain("Override/live.txt"));
        }
    }
}
