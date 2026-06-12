// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Net.Http;
using ModSync.Core.Services.Download;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NexusModsDownloadHandlerTests
    {
        private HttpClient _httpClient;
        private NexusModsDownloadHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _httpClient = new HttpClient();
            _handler = new NexusModsDownloadHandler(_httpClient, apiKey: null);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient?.Dispose();
        }

        [TestCase("nxm://kotor/mods/1234/files/5678?key=abc&expires=1718000000&user_id=42")]
        [TestCase("nxm://kotor2/mods/1100/files/9")]
        [TestCase("NXM://kotor/mods/1/files/2")]
        public void CanHandle_NxmUrl_ReturnsTrue(string url)
        {
            Assert.That(_handler.CanHandle(url), Is.True);
        }

        [TestCase("https://www.nexusmods.com/kotor/mods/1234")]
        [TestCase("https://nexusmods.com/kotor2/mods/1100?tab=files&file_id=9")]
        public void CanHandle_NexusModsWebUrl_StillReturnsTrue(string url)
        {
            Assert.That(_handler.CanHandle(url), Is.True);
        }

        [TestCase("https://mega.nz/file/1A4RCLha#Ro2GNVUPRfgot-woqh80jVaukixr-cnUmTdakuc0Ca4")]
        [TestCase("https://deadlystream.com/files/file/1313-kotor-dialogue-fixes/")]
        [TestCase("https://example.com/some/file.zip")]
        [TestCase("")]
        [TestCase(null)]
        public void CanHandle_NonNexusUrl_ReturnsFalse(string url)
        {
            Assert.That(_handler.CanHandle(url), Is.False);
        }
    }
}
