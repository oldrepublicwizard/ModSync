// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using ModSync.Core.Services.Protocol;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ModSyncUrlTests
    {
        private const string InstructionHttps = "https://raw.githubusercontent.com/example/mod-builds/main/TOMLs/KOTOR1_Full.toml";
        private const string FullUrl =
            "modsync://install?url=https%3A%2F%2Fraw.githubusercontent.com%2Fexample%2Fmod-builds%2Fmain%2FTOMLs%2FKOTOR1_Full.toml";

        [Test]
        public void TryParse_InstallWithUrl_Succeeds()
        {
            bool ok = ModSyncUrl.TryParse(FullUrl, out ModSyncUrl result);
            Assert.That(ok, Is.True);
            Assert.That(result.Action, Is.EqualTo("install"));
            Assert.That(result.Game, Is.Null);
            Assert.That(result.InstructionUrl, Is.EqualTo(InstructionHttps));
        }

        [Test]
        public void TryParse_OpenWithInstructionAndGame_Succeeds()
        {
            string url = "modsync://open?instruction=https%3A%2F%2Fexample.com%2Fbuild.toml&game=kotor2";
            bool ok = ModSyncUrl.TryParse(url, out ModSyncUrl result);
            Assert.That(ok, Is.True);
            Assert.That(result.Action, Is.EqualTo("open"));
            Assert.That(result.Game, Is.EqualTo("kotor2"));
            Assert.That(result.InstructionUrl, Is.EqualTo("https://example.com/build.toml"));
        }

        [Test]
        public void TryParse_GameHostThenAction_Succeeds()
        {
            string url = "modsync://kotor/install?url=https%3A%2F%2Fexample.com%2Fa.toml";
            bool ok = ModSyncUrl.TryParse(url, out ModSyncUrl result);
            Assert.That(ok, Is.True);
            Assert.That(result.Action, Is.EqualTo("install"));
            Assert.That(result.Game, Is.EqualTo("kotor"));
        }

        [Test]
        public void TryParse_ActionHostWithGamePath_Succeeds()
        {
            string url = "modsync://install/kotor?url=https%3A%2F%2Fexample.com%2Fa.toml";
            bool ok = ModSyncUrl.TryParse(url, out ModSyncUrl result);
            Assert.That(ok, Is.True);
            Assert.That(result.Game, Is.EqualTo("kotor"));
        }

        [Test]
        public void TryParse_CaseInsensitive_Succeeds()
        {
            string url = "MODSYNC://INSTALL?URL=HTTPS%3A%2F%2FEXAMPLE.COM%2FBUILD.TOML&GAME=KOTOR";
            bool ok = ModSyncUrl.TryParse(url, out ModSyncUrl result);
            Assert.That(ok, Is.True);
            Assert.That(result.Action, Is.EqualTo("install"));
            Assert.That(result.Game, Is.EqualTo("kotor"));
        }

        [Test]
        public void TryParse_LeadingWhitespace_Succeeds()
        {
            Assert.That(ModSyncUrl.TryParse("  " + FullUrl, out ModSyncUrl result), Is.True);
            Assert.That(result.InstructionUrl, Is.EqualTo(InstructionHttps));
        }

        [Test]
        public void TryParse_HttpInstructionUrl_Succeeds()
        {
            string url = "modsync://install?url=http%3A%2F%2Fexample.com%2Fbuild.toml";
            Assert.That(ModSyncUrl.TryParse(url, out ModSyncUrl result), Is.True);
            Assert.That(result.InstructionUrl, Is.EqualTo("http://example.com/build.toml"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("https://example.com/build.toml")]
        [TestCase("nxm://kotor/mods/1/files/2")]
        [TestCase("modsync://install")]
        [TestCase("modsync://install?url=")]
        [TestCase("modsync://install?url=file%3A%2F%2F%2Ftmp%2Fbuild.toml")]
        [TestCase("modsync://download?url=https%3A%2F%2Fexample.com%2Fa.toml")]
        [TestCase("modsync://skyrim/install?url=https%3A%2F%2Fexample.com%2Fa.toml")]
        [TestCase("modsync://install?url=https%3A%2F%2Fexample.com%2Fa.toml&game=skyrim")]
        [TestCase("modsync://kotor/install?url=https%3A%2F%2Fexample.com%2Fa.toml&game=kotor2")]
        public void TryParse_Invalid_ReturnsFalse(string url)
        {
            Assert.That(ModSyncUrl.TryParse(url, out ModSyncUrl result), Is.False);
            Assert.That(result, Is.Null);
        }

        [TestCase("modsync://install?url=https%3A%2F%2Fexample.com%2Fa.toml", true)]
        [TestCase("nxm://kotor/mods/1/files/2", false)]
        [TestCase(null, false)]
        public void IsModSyncUrl_DetectsScheme(string url, bool expected)
        {
            Assert.That(ModSyncUrl.IsModSyncUrl(url), Is.EqualTo(expected));
        }

        [Test]
        public void ToString_WithGame_IncludesGameQuery()
        {
            string url = "modsync://kotor/install?url=https%3A%2F%2Fexample.com%2Fa.toml";
            _ = ModSyncUrl.TryParse(url, out ModSyncUrl result);
            Assert.That(result.ToString(), Is.EqualTo(
                "modsync://install?url=" + Uri.EscapeDataString("https://example.com/a.toml") + "&game=kotor"));
        }
    }
}
