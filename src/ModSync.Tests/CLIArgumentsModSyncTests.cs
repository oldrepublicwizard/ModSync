// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class CLIArgumentsModSyncTests
    {
        [SetUp]
        public void SetUp()
        {
            CLIArguments.KotorPath = null;
            CLIArguments.ModDirectory = null;
            CLIArguments.InstructionFile = null;
            CLIArguments.NxmUrl = null;
            CLIArguments.ModSyncProtocolUrl = null;
            CLIArguments.AllowMultipleInstances = false;
        }

        [Test]
        public void Parse_ModSyncFlag_SetsProperty()
        {
            string url = "modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml";
            CLIArguments.Parse(new[] { "--modsync=" + url });
            Assert.That(CLIArguments.ModSyncProtocolUrl, Is.EqualTo(url));
            Assert.That(CLIArguments.HasProtocolHandoffUrl, Is.True);
            Assert.That(CLIArguments.ProtocolHandoffUrl, Is.EqualTo(url));
        }

        [Test]
        public void Parse_ModSyncPositional_SetsProperty()
        {
            string url = "modsync://kotor/open?url=https%3A%2F%2Fexample.com%2Fa.toml";
            CLIArguments.Parse(new[] { url });
            Assert.That(CLIArguments.ModSyncProtocolUrl, Is.EqualTo(url));
        }

        [Test]
        public void Parse_NxmTakesPrecedenceInProtocolHandoffUrl()
        {
            string nxm = "nxm://kotor/mods/1/files/2";
            string modsync = "modsync://install?url=https%3A%2F%2Fexample.com%2Fa.toml";
            CLIArguments.Parse(new[] { "--nxm=" + nxm, "--modsync=" + modsync });
            Assert.That(CLIArguments.ProtocolHandoffUrl, Is.EqualTo(nxm));
        }
    }
}
