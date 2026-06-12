// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class MacOsReleaseBundleTests
    {
        [Test]
        public void BuildAndReleaseWorkflow_BundlesMacOsAppWithNxmPlist()
        {
            string repoRoot = ResolveRepoRoot();
            string workflowPath = Path.Combine(repoRoot, ".github", "workflows", "build-and-release.yml");
            string workflow = File.ReadAllText(workflowPath);

            Assert.That(workflow, Does.Contain("Publish and bundle"));
            Assert.That(workflow, Does.Contain("-t:BundleApp"));
            Assert.That(workflow, Does.Contain("bundle-macos-app.ps1"));
            Assert.That(workflow, Does.Contain("ModSync.app"));
        }

        [Test]
        public void BundleMacOsAppScript_ValidatesNxmScheme()
        {
            string repoRoot = ResolveRepoRoot();
            string scriptPath = Path.Combine(repoRoot, "scripts", "ci", "bundle-macos-app.ps1");
            string script = File.ReadAllText(scriptPath);

            Assert.That(script, Does.Contain("CFBundleURLTypes"));
            Assert.That(script, Does.Contain("nxm"));
            Assert.That(script, Does.Contain("Contents/MacOS/ModSync"));
        }

        [Test]
        public void ModSyncGuiProject_PublishesMacOsIconForBundle()
        {
            string repoRoot = ResolveRepoRoot();
            string csproj = File.ReadAllText(Path.Combine(repoRoot, "src", "ModSync.GUI", "ModSync.csproj"));

            Assert.That(csproj, Does.Contain("icon53.icns"));
            Assert.That(File.Exists(Path.Combine(repoRoot, "src", "ModSync.GUI", "icon53.icns")), Is.True);
        }

        private static string ResolveRepoRoot()
        {
            string repoRoot = Path.GetFullPath(
                Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
            if (!File.Exists(Path.Combine(repoRoot, "ModSync.sln")))
            {
                repoRoot = Path.GetFullPath(Path.Combine(repoRoot, ".."));
            }

            return repoRoot;
        }
    }
}
