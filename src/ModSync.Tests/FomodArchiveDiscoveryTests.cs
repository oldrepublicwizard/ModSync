// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodArchiveDiscoveryTests
    {
        [Test]
        public void FindModuleConfigPath_DiscoversNestedFomodFolder()
        {
            string root = Path.Combine(Path.GetTempPath(), "modsync-fomod-discovery-" + Path.GetRandomFileName());
            string fomodDir = Path.Combine(root, "fomod");
            Directory.CreateDirectory(fomodDir);
            string moduleConfigPath = Path.Combine(fomodDir, "ModuleConfig.xml");
            File.WriteAllText(moduleConfigPath, "<config><moduleName>Test</moduleName></config>");

            try
            {
                string found = FomodArchiveDiscovery.FindModuleConfigPath(root);
                Assert.That(found, Is.EqualTo(moduleConfigPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
