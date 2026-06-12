// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NxmProtocolRegistrationServiceTests
    {
        private const string LinuxExePath = "/opt/modsync/ModSync";
        private const string WindowsExePath = @"C:\Program Files\ModSync\ModSync.exe";

        [Test]
        public void BuildDesktopFileContent_ContainsSchemeHandlerMimeType()
        {
            string content = NxmProtocolRegistrationService.BuildDesktopFileContent(LinuxExePath);

            Assert.That(content, Does.Contain("MimeType=x-scheme-handler/nxm;"));
        }

        [Test]
        public void BuildDesktopFileContent_ExecLinePassesUrlPlaceholder()
        {
            string content = NxmProtocolRegistrationService.BuildDesktopFileContent(LinuxExePath);

            Assert.That(content, Does.Contain($"Exec=\"{LinuxExePath}\" %u"));
        }

        [Test]
        public void BuildDesktopFileContent_IsWellFormedDesktopEntry()
        {
            string content = NxmProtocolRegistrationService.BuildDesktopFileContent(LinuxExePath);

            Assert.That(content, Does.StartWith("[Desktop Entry]"));
            Assert.That(content, Does.Contain("Type=Application"));
            Assert.That(content, Does.Contain("Name=ModSync"));
            Assert.That(content, Does.Contain("Terminal=false"));
            Assert.That(content, Does.Contain("NoDisplay=true"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void BuildDesktopFileContent_MissingExePath_Throws(string exePath)
        {
            Assert.That(() => NxmProtocolRegistrationService.BuildDesktopFileContent(exePath), Throws.ArgumentException);
        }

        [Test]
        public void BuildWindowsRegCommands_TargetsHkcuClassesNxm()
        {
            string[] commands = NxmProtocolRegistrationService.BuildWindowsRegCommands(WindowsExePath);

            Assert.That(commands, Is.Not.Empty);
            Assert.That(commands, Has.All.Contain(@"HKCU\Software\Classes\nxm"));
        }

        [Test]
        public void BuildWindowsRegCommands_DeclaresUrlProtocol()
        {
            string[] commands = NxmProtocolRegistrationService.BuildWindowsRegCommands(WindowsExePath);

            Assert.That(commands.Any(c => c.Contains("\"URL Protocol\"")), Is.True);
        }

        [Test]
        public void BuildWindowsRegCommands_RegistersOpenCommandWithExeAndArgument()
        {
            string[] commands = NxmProtocolRegistrationService.BuildWindowsRegCommands(WindowsExePath);

            string openCommand = commands.FirstOrDefault(c => c.Contains(@"shell\open\command"));
            Assert.That(openCommand, Is.Not.Null);
            Assert.That(openCommand, Does.Contain(WindowsExePath));
            Assert.That(openCommand, Does.Contain("%1"));
        }

        [Test]
        public void BuildWindowsRegCommands_AllCommandsAreForcedAdds()
        {
            string[] commands = NxmProtocolRegistrationService.BuildWindowsRegCommands(WindowsExePath);

            Assert.That(commands, Has.All.StartWith("add "));
            Assert.That(commands, Has.All.Contain("/f"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void BuildWindowsRegCommands_MissingExePath_Throws(string exePath)
        {
            Assert.That(() => NxmProtocolRegistrationService.BuildWindowsRegCommands(exePath), Throws.ArgumentException);
        }

        [Test]
        public void BuildMacOsUrlTypesPlistFragment_DeclaresNxmScheme()
        {
            string fragment = NxmProtocolRegistrationService.BuildMacOsUrlTypesPlistFragment();

            Assert.That(fragment, Does.Contain("CFBundleURLTypes"));
            Assert.That(fragment, Does.Contain("CFBundleURLSchemes"));
            Assert.That(fragment, Does.Contain("<string>nxm</string>"));
        }
    }
}

