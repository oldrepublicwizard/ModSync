// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NxmInfoPlistTests
    {
        [TestCase("Info.plist")]
        [TestCase("src/ModSync.GUI/Info.plist")]
        public void InfoPlist_DeclaresNxmUrlScheme(string relativePath)
        {
            string repoRoot = Path.GetFullPath(
                Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", ".."));
            if (!File.Exists(Path.Combine(repoRoot, "ModSync.sln")))
            {
                repoRoot = Path.GetFullPath(Path.Combine(repoRoot, ".."));
            }

            string path = Path.GetFullPath(Path.Combine(repoRoot, relativePath));
            Assert.That(File.Exists(path), Is.True, $"Missing plist at {path}");

            string content = File.ReadAllText(path);
            Assert.That(content, Does.Contain("CFBundleURLTypes"));
            Assert.That(content, Does.Contain("<string>nxm</string>"));
        }
    }
}
