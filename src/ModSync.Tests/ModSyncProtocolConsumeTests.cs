// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using ModSync.Core.Services.Protocol;
using ModSync.Services;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ModSyncHandoffQueueTests
    {
        [SetUp]
        public void SetUp()
        {
            _ = ModSyncHandoffQueue.DrainAll();
        }

        [TearDown]
        public void TearDown()
        {
            _ = ModSyncHandoffQueue.DrainAll();
        }

        [Test]
        public void Enqueue_ThenDrainAll_ReturnsInOrder()
        {
            ModSyncHandoffQueue.Enqueue("modsync://install?url=https%3A%2F%2Fa.example%2Fa.toml");
            ModSyncHandoffQueue.Enqueue("modsync://open?url=https%3A%2F%2Fb.example%2Fb.toml");

            var drained = ModSyncHandoffQueue.DrainAll();
            Assert.That(drained, Has.Count.EqualTo(2));
            Assert.That(drained[0], Does.Contain("a.example"));
            Assert.That(drained[1], Does.Contain("b.example"));
            Assert.That(ModSyncHandoffQueue.Count, Is.EqualTo(0));
        }

        [Test]
        public void Enqueue_Whitespace_IsIgnored()
        {
            ModSyncHandoffQueue.Enqueue("   ");
            Assert.That(ModSyncHandoffQueue.Count, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class ModSyncInstructionFetcherTests
    {
        [Test]
        public async Task DownloadToTempFileAsync_WritesBodyWithTomlExtension()
        {
            const string body = "[[thisMod]]\nname = \"Demo\"\n";
            using (var handler = new StubHttpHandler(HttpStatusCode.OK, body))
            using (var client = new HttpClient(handler))
            {
                string path = await ModSyncInstructionFetcher.DownloadToTempFileAsync(
                    "https://example.com/builds/demo.toml",
                    client);

                try
                {
                    Assert.That(File.Exists(path), Is.True);
                    Assert.That(Path.GetExtension(path), Is.EqualTo(".toml"));
                    Assert.That(File.ReadAllText(path), Is.EqualTo(body));
                }
                finally
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        [Test]
        public void DownloadToTempFileAsync_RejectsNonHttpUrl()
        {
            using (var client = new HttpClient())
            {
                Assert.That(
                    async () => await ModSyncInstructionFetcher.DownloadToTempFileAsync(
                        "file:///tmp/build.toml",
                        client),
                    Throws.ArgumentException);
            }
        }

        private sealed class StubHttpHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly string _body;

            public StubHttpHandler(HttpStatusCode status, string body)
            {
                _status = status;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body),
                });
            }
        }
    }

    [TestFixture]
    public class ModSyncProtocolRegistrationServiceTests
    {
        private const string LinuxExePath = "/opt/modsync/ModSync";
        private const string WindowsExePath = @"C:\Apps\ModSync\ModSync.exe";

        [Test]
        public void BuildDesktopFileContent_IncludesModSyncScheme()
        {
            string content = ModSyncProtocolRegistrationService.BuildDesktopFileContent(LinuxExePath);
            Assert.That(content, Does.Contain("x-scheme-handler/modsync"));
            Assert.That(content, Does.Contain($"Exec=\"{LinuxExePath}\" %u"));
        }

        [Test]
        public void BuildWindowsRegCommands_TargetsModSyncClassKey()
        {
            string[] commands = ModSyncProtocolRegistrationService.BuildWindowsRegCommands(WindowsExePath);
            Assert.That(commands, Has.Length.EqualTo(4));
            Assert.That(commands[0], Does.Contain(@"HKCU\Software\Classes\modsync"));
            Assert.That(commands[3], Does.Contain(WindowsExePath));
        }

        [Test]
        public void BuildMacOsUrlTypesPlistFragment_DeclaresModSyncScheme()
        {
            string fragment = ModSyncProtocolRegistrationService.BuildMacOsUrlTypesPlistFragment();
            Assert.That(fragment, Does.Contain("<string>modsync</string>"));
        }

        [Test]
        public void BuildDesktopFileContent_EmptyPath_Throws()
        {
            Assert.That(
                () => ModSyncProtocolRegistrationService.BuildDesktopFileContent(" "),
                Throws.ArgumentException);
        }
    }
}
