// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;

using ModSync.Core.Ports.Installation;
using ModSync.Core.Services.Deployment;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ManagedDeploymentLifecycleTests
    {
        private string _gameDirectory;
        private string _stagingRoot;
        private string _manifestRoot;
        private DeploymentService _deploymentService;
        private ManagedDeploymentInstallBackend _backend;

        [SetUp]
        public void SetUp()
        {
            string root = Path.Combine(Path.GetTempPath(), "modsync-lifecycle-" + Guid.NewGuid().ToString("N"));
            _gameDirectory = Path.Combine(root, "game");
            _stagingRoot = Path.Combine(root, "staging");
            _manifestRoot = Path.Combine(root, "manifests");
            Directory.CreateDirectory(_gameDirectory);
            Directory.CreateDirectory(_stagingRoot);
            Directory.CreateDirectory(_manifestRoot);
            _deploymentService = new DeploymentService(_gameDirectory, _stagingRoot, _manifestRoot);
            _backend = new ManagedDeploymentInstallBackend(_deploymentService);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                string root = Path.GetDirectoryName(_gameDirectory);
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
            catch
            {
                // best effort
            }
        }

        [Test]
        public void GetStatus_ClassicBackend_ReportsNoManagedDeployments()
        {
            ManagedDeploymentStatus status = ManagedDeploymentLifecycle.GetStatus(
                ClassicInstructionInstallBackend.Instance);

            Assert.That(status.ManagedBackend, Is.False);
            Assert.That(status.DeployedComponentCount, Is.EqualTo(0));
            Assert.That(status.FormatIndicator(), Does.Contain("classic mode"));
        }

        [Test]
        public async Task PurgeAsync_RemovesDeployedComponents()
        {
            Guid componentGuid = Guid.NewGuid();
            string staged = Path.Combine(_stagingRoot, componentGuid.ToString());
            Directory.CreateDirectory(staged);
            string stagedFile = Path.Combine(staged, "Override", "demo.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(stagedFile));
            File.WriteAllText(stagedFile, "managed-content");

            await _deploymentService.DeployComponentAsync(
                componentGuid,
                "Demo Component",
                staged);

            ManagedDeploymentStatus before = ManagedDeploymentLifecycle.GetStatus(_backend);
            Assert.That(before.DeployedComponentCount, Is.EqualTo(1));
            Assert.That(before.FormatIndicator(), Does.Contain("1 component"));

            ManagedDeploymentPurgeResult result = await ManagedDeploymentLifecycle.PurgeAsync(_backend);
            Assert.That(result.ComponentsPurged, Is.EqualTo(1));
            Assert.That(result.FormatSummary(), Does.Contain("Purged 1"));

            ManagedDeploymentStatus after = ManagedDeploymentLifecycle.GetStatus(_backend);
            Assert.That(after.DeployedComponentCount, Is.EqualTo(0));
            Assert.That(File.Exists(Path.Combine(_gameDirectory, "Override", "demo.txt")), Is.False);
        }

        [Test]
        public async Task UninstallComponentAsync_RemovesSingleComponent()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            await DeploySimpleAsync(first, "one.txt", "a");
            await DeploySimpleAsync(second, "two.txt", "b");

            await ManagedDeploymentLifecycle.UninstallComponentAsync(_backend, first);

            ManagedDeploymentStatus status = ManagedDeploymentLifecycle.GetStatus(_backend);
            Assert.That(status.DeployedComponentCount, Is.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(_gameDirectory, "Override", "one.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(_gameDirectory, "Override", "two.txt")), Is.True);
        }

        private async Task DeploySimpleAsync(Guid componentGuid, string fileName, string contents)
        {
            string staged = Path.Combine(_stagingRoot, componentGuid.ToString());
            string stagedFile = Path.Combine(staged, "Override", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(stagedFile));
            File.WriteAllText(stagedFile, contents);
            await _deploymentService.DeployComponentAsync(componentGuid, fileName, staged);
        }
    }
}
