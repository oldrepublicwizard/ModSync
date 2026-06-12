// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Core.Services.Download;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NxmUrlTests
    {
        private const string FullUrl = "nxm://kotor/mods/1234/files/5678?key=abc123XYZ&expires=1718000000&user_id=42";

        [Test]
        public void TryParse_FullUrl_ParsesAllParts()
        {
            bool ok = NxmUrl.TryParse(FullUrl, out NxmUrl result);

            Assert.That(ok, Is.True);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GameDomain, Is.EqualTo("kotor"));
            Assert.That(result.ModId, Is.EqualTo(1234));
            Assert.That(result.FileId, Is.EqualTo(5678));
            Assert.That(result.Key, Is.EqualTo("abc123XYZ"));
            Assert.That(result.Expires, Is.EqualTo(1718000000));
            Assert.That(result.UserId, Is.EqualTo(42));
            Assert.That(result.HasDownloadAuthorization, Is.True);
            Assert.That(result.OriginalUrl, Is.EqualTo(FullUrl));
        }

        [Test]
        public void TryParse_NoQuery_ParsesWithoutAuthorization()
        {
            bool ok = NxmUrl.TryParse("nxm://kotor2/mods/1100/files/9", out NxmUrl result);

            Assert.That(ok, Is.True);
            Assert.That(result.GameDomain, Is.EqualTo("kotor2"));
            Assert.That(result.ModId, Is.EqualTo(1100));
            Assert.That(result.FileId, Is.EqualTo(9));
            Assert.That(result.Key, Is.Null);
            Assert.That(result.Expires, Is.EqualTo(0));
            Assert.That(result.HasDownloadAuthorization, Is.False);
        }

        [Test]
        public void TryParse_UppercaseSchemeAndPath_IsCaseInsensitive()
        {
            bool ok = NxmUrl.TryParse("NXM://KOTOR/MODS/12/FILES/34?KEY=k&EXPIRES=77", out NxmUrl result);

            Assert.That(ok, Is.True);
            Assert.That(result.GameDomain, Is.EqualTo("kotor"));
            Assert.That(result.ModId, Is.EqualTo(12));
            Assert.That(result.FileId, Is.EqualTo(34));
            Assert.That(result.Key, Is.EqualTo("k"));
            Assert.That(result.Expires, Is.EqualTo(77));
        }

        [Test]
        public void TryParse_LeadingWhitespace_StillParses()
        {
            bool ok = NxmUrl.TryParse("  nxm://kotor/mods/1/files/2", out NxmUrl result);

            Assert.That(ok, Is.True);
            Assert.That(result.ModId, Is.EqualTo(1));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("https://www.nexusmods.com/kotor/mods/1234")]
        [TestCase("nxm://kotor/mods/1234")]
        [TestCase("nxm://kotor/files/5678")]
        [TestCase("nxm://kotor/mods/abc/files/5678")]
        [TestCase("nxm://kotor/mods/1234/files/xyz")]
        [TestCase("nxm://kotor/mods/-1/files/2")]
        [TestCase("nxm://kotor/mods/1/files/2/extra")]
        [TestCase("notaurl")]
        public void TryParse_InvalidInput_ReturnsFalse(string url)
        {
            bool ok = NxmUrl.TryParse(url, out NxmUrl result);

            Assert.That(ok, Is.False);
            Assert.That(result, Is.Null);
        }

        [TestCase("nxm://kotor/mods/1/files/2", true)]
        [TestCase("NXM://kotor/mods/1/files/2", true)]
        [TestCase("  nxm://kotor/mods/1/files/2", true)]
        [TestCase("https://www.nexusmods.com/kotor/mods/1", false)]
        [TestCase(null, false)]
        [TestCase("", false)]
        public void IsNxmUrl_DetectsScheme(string url, bool expected)
        {
            Assert.That(NxmUrl.IsNxmUrl(url), Is.EqualTo(expected));
        }

        [Test]
        public void ToModPageUrl_ProducesCanonicalUrl()
        {
            _ = NxmUrl.TryParse(FullUrl, out NxmUrl result);

            Assert.That(result.ToModPageUrl(), Is.EqualTo("https://www.nexusmods.com/kotor/mods/1234"));
        }

        [Test]
        public void ToFileUrl_ProducesFileTabUrl()
        {
            _ = NxmUrl.TryParse(FullUrl, out NxmUrl result);

            Assert.That(result.ToFileUrl(), Is.EqualTo("https://www.nexusmods.com/kotor/mods/1234?tab=files&file_id=5678"));
        }

        [TestCase("https://www.nexusmods.com/kotor/mods/1234", true)]
        [TestCase("https://nexusmods.com/kotor/mods/1234", true)]
        [TestCase("https://www.nexusmods.com/KOTOR/mods/1234?tab=files&file_id=5678", true)]
        [TestCase("https://www.nexusmods.com/kotor/mods/9999", false)]
        [TestCase("https://www.nexusmods.com/kotor2/mods/1234", false)]
        [TestCase("https://example.com/kotor/mods/1234", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void MatchesNexusUrl_ComparesGameAndModId(string nexusUrl, bool expected)
        {
            _ = NxmUrl.TryParse(FullUrl, out NxmUrl result);

            Assert.That(result.MatchesNexusUrl(nexusUrl), Is.EqualTo(expected));
        }

        [Test]
        public void ToString_ContainsCoreCoordinates()
        {
            _ = NxmUrl.TryParse(FullUrl, out NxmUrl result);

            Assert.That(result.ToString(), Is.EqualTo("nxm://kotor/mods/1234/files/5678"));
        }
    }
}
