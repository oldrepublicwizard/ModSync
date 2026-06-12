// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NxmHandlerProbeTests
    {
        private const string ModSyncExe = "/opt/modsync/ModSync";
        private const string Mo2Exe = "/home/user/MO2/ModOrganizer.exe";
        private const string VortexExe = @"C:\Program Files\Black Tree Gaming Ltd\Vortex\Vortex.exe";

        [Test]
        public void ParseWindowsOpenCommandFromRegQueryOutput_QuotedExe_ReturnsPath()
        {
            const string regOutput =
                "HKEY_CURRENT_USER\\Software\\Classes\\nxm\\shell\\open\\command\r\n" +
                "    (Default)    REG_SZ    \"C:\\Program Files\\ModSync\\ModSync.exe\" \"%1\"\r\n";

            string path = NxmHandlerProbe.ParseWindowsOpenCommandFromRegQueryOutput(regOutput);

            Assert.That(path, Is.EqualTo(@"C:\Program Files\ModSync\ModSync.exe"));
        }

        [Test]
        public void ParseWindowsOpenCommandFromRegQueryOutput_UnquotedExe_ReturnsPath()
        {
            const string regOutput =
                "    (Default)    REG_SZ    C:\\Apps\\Vortex.exe %1\r\n";

            string path = NxmHandlerProbe.ParseWindowsOpenCommandFromRegQueryOutput(regOutput);

            Assert.That(path, Is.EqualTo(@"C:\Apps\Vortex.exe"));
        }

        [Test]
        public void ParseDesktopExecLine_QuotedExec_ReturnsExecutable()
        {
            const string desktop =
                "[Desktop Entry]\n" +
                "Exec=\"/usr/bin/mo2\" %u\n";

            string path = NxmHandlerProbe.ParseDesktopExecLine(desktop);

            Assert.That(path, Is.EqualTo("/usr/bin/mo2"));
        }

        [Test]
        public void ParseDesktopExecLine_UnquotedExec_ReturnsFirstToken()
        {
            const string desktop = "Exec=/opt/vortex/Vortex %u\n";

            string path = NxmHandlerProbe.ParseDesktopExecLine(desktop);

            Assert.That(path, Is.EqualTo("/opt/vortex/Vortex"));
        }

        [TestCase(@"C:\MO2\ModOrganizer.exe", NxmHandlerIdentity.ModOrganizer2)]
        [TestCase("/home/user/ModOrganizer2/ModOrganizer", NxmHandlerIdentity.ModOrganizer2)]
        [TestCase(VortexExe, NxmHandlerIdentity.Vortex)]
        [TestCase("/opt/NexusClient/nexusclient", NxmHandlerIdentity.Vortex)]
        [TestCase("/usr/local/bin/custom-handler", NxmHandlerIdentity.Other)]
        public void IdentifyHandler_ClassifiesKnownCompetitors(string exePath, NxmHandlerIdentity expected)
        {
            Assert.That(NxmHandlerProbe.IdentifyHandler(exePath), Is.EqualTo(expected));
        }

        [Test]
        public void Evaluate_ModSyncOwnsActive_ReturnsModSyncActive()
        {
            NxmHandlerProbeResult result = NxmHandlerProbe.Evaluate(
                activeHandlerExecutable: ModSyncExe,
                modSyncExecutable: ModSyncExe,
                modSyncDesktopFileExists: false,
                linuxDefaultDesktopFileName: null);

            Assert.That(result.Status, Is.EqualTo(NxmHandlerStatus.ModSyncActive));
            Assert.That(result.Identity, Is.EqualTo(NxmHandlerIdentity.ModSync));
        }

        [Test]
        public void Evaluate_CompetitorOwnsActive_ReturnsCompetitorActive()
        {
            NxmHandlerProbeResult result = NxmHandlerProbe.Evaluate(
                activeHandlerExecutable: Mo2Exe,
                modSyncExecutable: ModSyncExe,
                modSyncDesktopFileExists: false,
                linuxDefaultDesktopFileName: null);

            Assert.That(result.Status, Is.EqualTo(NxmHandlerStatus.CompetitorActive));
            Assert.That(result.Identity, Is.EqualTo(NxmHandlerIdentity.ModOrganizer2));
            Assert.That(result.DisplayName, Is.EqualTo("Mod Organizer 2"));
        }

        [Test]
        public void Evaluate_LinuxModSyncDesktopNotDefault_ReturnsModSyncRegisteredNotDefault()
        {
            NxmHandlerProbeResult result = NxmHandlerProbe.Evaluate(
                activeHandlerExecutable: Mo2Exe,
                modSyncExecutable: ModSyncExe,
                modSyncDesktopFileExists: true,
                linuxDefaultDesktopFileName: "mo2-nxm.desktop");

            Assert.That(result.Status, Is.EqualTo(NxmHandlerStatus.CompetitorActive));
        }

        [Test]
        public void Evaluate_ModSyncDesktopExistsButNoActiveHandler_ReturnsModSyncRegisteredNotDefault()
        {
            NxmHandlerProbeResult result = NxmHandlerProbe.Evaluate(
                activeHandlerExecutable: null,
                modSyncExecutable: ModSyncExe,
                modSyncDesktopFileExists: true,
                linuxDefaultDesktopFileName: "mo2-nxm.desktop");

            Assert.That(result.Status, Is.EqualTo(NxmHandlerStatus.ModSyncRegisteredNotDefault));
            Assert.That(result.DisplayName, Is.EqualTo("mo2-nxm.desktop"));
        }

        [Test]
        public void Evaluate_NoHandler_ReturnsUnregistered()
        {
            NxmHandlerProbeResult result = NxmHandlerProbe.Evaluate(
                activeHandlerExecutable: null,
                modSyncExecutable: ModSyncExe,
                modSyncDesktopFileExists: false,
                linuxDefaultDesktopFileName: null);

            Assert.That(result.Status, Is.EqualTo(NxmHandlerStatus.Unregistered));
        }

        [Test]
        public void BuildSettingsStatusText_CompetitorActive_IncludesDisplayName()
        {
            var probe = new NxmHandlerProbeResult
            {
                Status = NxmHandlerStatus.CompetitorActive,
                DisplayName = "Vortex",
            };

            string text = NxmHandlerProbe.BuildSettingsStatusText(probe, registrationPreferenceEnabled: false);

            Assert.That(text, Does.Contain("Vortex"));
            Assert.That(text, Does.Contain("currently handles"));
        }

        [Test]
        public void PathsReferToSameExecutable_SamePathDifferentCase_ReturnsTrue()
        {
            bool same = NxmHandlerProbe.PathsReferToSameExecutable(
                @"C:\Apps\ModSync.exe",
                @"c:\apps\modsync.exe");

            Assert.That(same, Is.True);
        }
    }
}
