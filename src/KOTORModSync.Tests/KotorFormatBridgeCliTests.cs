// Copyright 2021-2025 KOTORModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace KOTORModSync.Tests
{
    [TestFixture]
    public sealed class KotorFormatBridgeCliTests
    {
        private static string BridgeScriptPath =>
            Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..", "..", "..",
                "tools", "godot-holocron", "bridge", "kotor_format_bridge.py"));

        private static string SampleTwoDaPath =>
            Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..",
                "Fixtures", "kotor", "sample.2da"));

        private static string SampleTlkPath =>
            Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..",
                "Fixtures", "kotor", "sample.tlk"));

        private static string SampleSsfPath =>
            Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..",
                "Fixtures", "kotor", "sample.ssf"));

        private static string SampleModPath =>
            Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "..", "..", "..",
                "Fixtures", "kotor", "sample.mod"));

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (!File.Exists(BridgeScriptPath))
            {
                Assert.Ignore($"Bridge script not found at {BridgeScriptPath}");
            }

            if (!PyKotorImportWorks())
            {
                Assert.Ignore("PyKotor is not importable; skipping KotorFormatBridgeCliTests.");
            }
        }

        [Test]
        public void Probe_SampleTwoDa_ReturnsMetadata()
        {
            var result = RunBridge("probe", SampleTwoDaPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            Assert.That(result.GetProperty("extension").GetString(), Is.EqualTo("2da"));
            Assert.That(result.GetProperty("resource_type").GetString(), Is.EqualTo("TwoDA"));
            Assert.That(result.GetProperty("editor_kind").GetString(), Is.EqualTo("twoda"));
        }

        [Test]
        public void Probe_UnsupportedResource_ReturnsUnsupportedEditorKind()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_" + Guid.NewGuid() + ".res");
            try
            {
                File.WriteAllBytes(tempPath, new byte[] { 0, 1 });

                var result = RunBridge("probe", tempPath);
                Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(result.GetProperty("editor_kind").GetString(), Is.EqualTo("unsupported"));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void Probe_SampleMod_ReturnsArchiveEditorKind()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            var result = RunBridge("probe", SampleModPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            Assert.That(result.GetProperty("extension").GetString(), Is.EqualTo("mod"));
            Assert.That(result.GetProperty("editor_kind").GetString(), Is.EqualTo("erf"));
        }

        [Test]
        public void Probe_MissingPath_ReturnsError()
        {
            string missingPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_missing_" + Guid.NewGuid() + ".2da");
            var raw = RunBridgeRaw("probe", missingPath);
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
        }

        [Test]
        public void Read_MissingPath_ReturnsError()
        {
            string missingPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_missing_" + Guid.NewGuid() + ".2da");
            var raw = RunBridgeRaw("read", missingPath);
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
        }

        [Test]
        public void Write_InvalidJsonPayload_ReturnsError()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_write_" + Guid.NewGuid() + ".2da");
            var raw = RunBridgeRaw("write", tempPath, "--payload", "not json");
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("Invalid JSON").IgnoreCase);
        }

        [Test]
        public void Write_UnimplementedFormat_ReturnsError()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_write_" + Guid.NewGuid() + ".wav");
            const string payloadJson = "{\"format\":\"mdl\",\"data\":{}}";
            var raw = RunBridgeRaw("write", tempPath, "--payload", payloadJson);
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("not implemented").IgnoreCase);
        }

        [Test]
        public void Read_BinaryFile_ReturnsBase64Payload()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_" + Guid.NewGuid() + ".mdl");
            try
            {
                File.WriteAllBytes(tempPath, new byte[] { 0x41, 0x42, 0x00, 0xFF });

                var probe = RunBridge("probe", tempPath);
                Assert.That(probe.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(probe.GetProperty("editor_kind").GetString(), Is.EqualTo("binary"));

                var result = RunBridge("read", tempPath);
                Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
                var payload = result.GetProperty("payload");
                Assert.That(payload.GetProperty("format").GetString(), Is.EqualTo("binary"));
                Assert.That(payload.GetProperty("size").GetInt32(), Is.EqualTo(4));
                Assert.That(payload.GetProperty("base64").GetString(), Is.Not.Empty);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void Read_SampleTwoDa_ReturnsRows()
        {
            var result = RunBridge("read", SampleTwoDaPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            var payload = result.GetProperty("payload");
            Assert.That(payload.GetProperty("format").GetString(), Is.EqualTo("twoda"));
            var rows = payload.GetProperty("data").GetProperty("rows");
            Assert.That(rows.GetArrayLength(), Is.EqualTo(2));
        }

        [Test]
        public void Write_RoundTripsSampleTwoDa()
        {
            var readResult = RunBridge("read", SampleTwoDaPath);
            Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
            var payload = readResult.GetProperty("payload").GetRawText();

            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_roundtrip_" + Guid.NewGuid() + ".2da");
            try
            {
                var writeResult = RunBridge("write", tempPath, "--payload", payload);
                Assert.That(writeResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(File.Exists(tempPath), Is.True);

                var reread = RunBridge("read", tempPath);
                Assert.That(reread.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    reread.GetProperty("payload").GetProperty("data").GetProperty("rows").GetArrayLength(),
                    Is.EqualTo(2));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void SupportedTypes_ReturnsNonEmptyList()
        {
            var result = RunBridge("supported-types");
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            Assert.That(result.GetProperty("types").GetArrayLength(), Is.GreaterThan(10));
        }

        [Test]
        public void Installations_ReturnsOk()
        {
            var result = RunBridge("installations");
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            Assert.That(result.GetProperty("installations").ValueKind, Is.EqualTo(JsonValueKind.Array));
        }

        [Test]
        public void Read_SampleTlk_ReturnsStrings()
        {
            if (!File.Exists(SampleTlkPath))
            {
                Assert.Ignore($"Fixture not found at {SampleTlkPath}");
            }

            var result = RunBridge("read", SampleTlkPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            var payload = result.GetProperty("payload");
            Assert.That(payload.GetProperty("format").GetString(), Is.EqualTo("tlk"));
            Assert.That(
                payload.GetProperty("data").GetProperty("strings").GetArrayLength(),
                Is.EqualTo(2));
        }

        [Test]
        public void Read_SampleSsf_ReturnsSounds()
        {
            if (!File.Exists(SampleSsfPath))
            {
                Assert.Ignore($"Fixture not found at {SampleSsfPath}");
            }

            var result = RunBridge("read", SampleSsfPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            var payload = result.GetProperty("payload");
            Assert.That(payload.GetProperty("format").GetString(), Is.EqualTo("ssf"));
            Assert.That(
                payload.GetProperty("data").GetProperty("sounds").GetArrayLength(),
                Is.GreaterThan(0));
        }

        [Test]
        public void Write_RoundTripsSampleSsf()
        {
            if (!File.Exists(SampleSsfPath))
            {
                Assert.Ignore($"Fixture not found at {SampleSsfPath}");
            }

            var readResult = RunBridge("read", SampleSsfPath);
            Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
            var payload = readResult.GetProperty("payload").GetRawText();

            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_ssf_" + Guid.NewGuid() + ".ssf");
            try
            {
                var writeResult = RunBridge("write", tempPath, "--payload", payload);
                Assert.That(writeResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(File.Exists(tempPath), Is.True);

                var reread = RunBridge("read", tempPath);
                Assert.That(reread.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    reread.GetProperty("payload").GetProperty("data").GetProperty("sounds").GetArrayLength(),
                    Is.GreaterThan(0));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void Extract_SampleMod_EmbeddedTwoDa_MatchesDirectRead()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_extract_" + Guid.NewGuid() + ".2da");
            try
            {
                var extractResult = RunBridge(
                    "extract",
                    SampleModPath,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--output",
                    tempPath);
                Assert.That(extractResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(File.Exists(tempPath), Is.True);

                var directRead = RunBridge("read", SampleTwoDaPath);
                var extractedRead = RunBridge("read", tempPath);
                Assert.That(extractedRead.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    extractedRead.GetProperty("payload").GetProperty("data").GetProperty("rows").GetArrayLength(),
                    Is.EqualTo(
                        directRead.GetProperty("payload").GetProperty("data").GetProperty("rows").GetArrayLength()));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void Probe_ExtractedTwoDa_ReturnsTwodaEditorKind()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_extract_" + Guid.NewGuid() + ".2da");
            try
            {
                var extractResult = RunBridge(
                    "extract",
                    SampleModPath,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--output",
                    tempPath);
                Assert.That(extractResult.GetProperty("ok").GetBoolean(), Is.True);

                var probe = RunBridge("probe", tempPath);
                Assert.That(probe.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(probe.GetProperty("extension").GetString(), Is.EqualTo("2da"));
                Assert.That(probe.GetProperty("editor_kind").GetString(), Is.EqualTo("twoda"));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        [Test]
        public void Extract_MissingArchive_ReturnsError()
        {
            string missingArchive = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            string outputPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_extract_" + Guid.NewGuid() + ".2da");
            var raw = RunBridgeRaw(
                "extract",
                missingArchive,
                "--resref",
                "test2da",
                "--restype",
                "2da",
                "--output",
                outputPath);
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
        }

        [Test]
        public void Extract_MissingMember_ReturnsError()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string outputPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_extract_" + Guid.NewGuid() + ".2da");
            try
            {
                var raw = RunBridgeRaw(
                    "extract",
                    SampleModPath,
                    "--resref",
                    "not_in_archive",
                    "--restype",
                    "2da",
                    "--output",
                    outputPath);
                Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
                Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
                Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("not found"));
            }
            finally
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }

        [Test]
        public void Inject_MissingArchive_ReturnsError()
        {
            string missingArchive = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            string sourcePath = Path.Combine(Path.GetTempPath(), "kotor_bridge_src_" + Guid.NewGuid() + ".2da");
            if (File.Exists(SampleTwoDaPath))
            {
                File.Copy(SampleTwoDaPath, sourcePath);
            }
            else
            {
                File.WriteAllText(sourcePath, "placeholder");
            }

            try
            {
                var raw = RunBridgeRaw(
                    "inject",
                    missingArchive,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--source",
                    sourcePath);
                Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
                Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
                Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
            }
            finally
            {
                if (File.Exists(sourcePath))
                {
                    File.Delete(sourcePath);
                }
            }
        }

        [Test]
        public void Inject_MissingSource_ReturnsError()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            string missingSource = Path.Combine(Path.GetTempPath(), "kotor_bridge_missing_" + Guid.NewGuid() + ".2da");
            try
            {
                File.Copy(SampleModPath, archiveCopy);

                var raw = RunBridgeRaw(
                    "inject",
                    archiveCopy,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--source",
                    missingSource);
                Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
                Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
                Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }
            }
        }

        [Test]
        public void Read_SampleMod_ReturnsResourceList()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            var result = RunBridge("read", SampleModPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            var resources = result.GetProperty("payload").GetProperty("resources");
            Assert.That(resources.GetArrayLength(), Is.GreaterThan(0));
        }

        [Test]
        public void Read_SampleMod_ResourcesIncludeTest2da()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            var result = RunBridge("read", SampleModPath);
            Assert.That(result.GetProperty("ok").GetBoolean(), Is.True);
            var resources = result.GetProperty("payload").GetProperty("resources");
            bool found = false;
            for (int i = 0; i < resources.GetArrayLength(); i++)
            {
                var entry = resources[i];
                if (entry.GetProperty("resref").GetString() == "test2da"
                    && string.Equals(entry.GetProperty("restype").GetString(), "2da", StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            Assert.That(found, Is.True, "sample.mod fixture should list embedded test2da.2da");
        }

        [Test]
        public void Inject_ReplacesMemberInArchiveCopy()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            string extractPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_extract_" + Guid.NewGuid() + ".2da");
            try
            {
                File.Copy(SampleModPath, archiveCopy);

                var injectResult = RunBridge(
                    "inject",
                    archiveCopy,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--source",
                    SampleTwoDaPath);
                Assert.That(injectResult.GetProperty("ok").GetBoolean(), Is.True);

                var extractResult = RunBridge(
                    "extract",
                    archiveCopy,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--output",
                    extractPath);
                Assert.That(extractResult.GetProperty("ok").GetBoolean(), Is.True);

                var directRead = RunBridge("read", SampleTwoDaPath);
                var extractedRead = RunBridge("read", extractPath);
                Assert.That(
                    extractedRead.GetProperty("payload").GetProperty("data").GetProperty("rows").GetArrayLength(),
                    Is.EqualTo(
                        directRead.GetProperty("payload").GetProperty("data").GetProperty("rows").GetArrayLength()));
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }

                if (File.Exists(extractPath))
                {
                    File.Delete(extractPath);
                }
            }
        }

        [Test]
        public void Remove_DeletesMemberFromArchiveCopy()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            try
            {
                File.Copy(SampleModPath, archiveCopy);

                var removeResult = RunBridge(
                    "remove",
                    archiveCopy,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da");
                Assert.That(removeResult.GetProperty("ok").GetBoolean(), Is.True);

                var readResult = RunBridge("read", archiveCopy);
                Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    readResult.GetProperty("payload").GetProperty("resources").GetArrayLength(),
                    Is.EqualTo(0));
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }
            }
        }

        [Test]
        public void Remove_MissingArchive_ReturnsError()
        {
            string missingArchive = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            var raw = RunBridgeRaw(
                "remove",
                missingArchive,
                "--resref",
                "test2da",
                "--restype",
                "2da");
            Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
            Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
            Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("does not exist").IgnoreCase);
        }

        [Test]
        public void Remove_MissingMember_ReturnsError()
        {
            if (!File.Exists(SampleModPath))
            {
                Assert.Ignore($"Fixture not found at {SampleModPath}");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            try
            {
                File.Copy(SampleModPath, archiveCopy);

                var raw = RunBridgeRaw(
                    "remove",
                    archiveCopy,
                    "--resref",
                    "not_in_archive",
                    "--restype",
                    "2da");
                Assert.That(raw.ExitCode, Is.Not.EqualTo(0));
                Assert.That(raw.Json.GetProperty("ok").GetBoolean(), Is.False);
                Assert.That(raw.Json.GetProperty("error").GetString(), Does.Contain("not found"));
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }
            }
        }

        [Test]
        public void Add_AppendsNewMemberToArchiveCopy()
        {
            if (!File.Exists(SampleModPath) || !File.Exists(SampleTwoDaPath))
            {
                Assert.Ignore("Holocron fixtures not found");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            try
            {
                File.Copy(SampleModPath, archiveCopy);

                var injectResult = RunBridge(
                    "inject",
                    archiveCopy,
                    "--resref",
                    "extra2da",
                    "--restype",
                    "2da",
                    "--source",
                    SampleTwoDaPath);
                Assert.That(injectResult.GetProperty("ok").GetBoolean(), Is.True);

                var readResult = RunBridge("read", archiveCopy);
                Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    readResult.GetProperty("payload").GetProperty("resources").GetArrayLength(),
                    Is.EqualTo(2));

                bool foundExtra = false;
                foreach (JsonElement resource in readResult.GetProperty("payload").GetProperty("resources").EnumerateArray())
                {
                    if (resource.GetProperty("resref").GetString() == "extra2da")
                    {
                        foundExtra = true;
                        break;
                    }
                }

                Assert.That(foundExtra, Is.True);
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }
            }
        }

        [Test]
        public void Inject_UpdatesResourceListingSize()
        {
            if (!File.Exists(SampleModPath) || !File.Exists(SampleTwoDaPath))
            {
                Assert.Ignore("Holocron fixtures not found");
            }

            string archiveCopy = Path.Combine(Path.GetTempPath(), "kotor_bridge_mod_" + Guid.NewGuid() + ".mod");
            try
            {
                File.Copy(SampleModPath, archiveCopy);
                long expectedSize = new FileInfo(SampleTwoDaPath).Length;

                var injectResult = RunBridge(
                    "inject",
                    archiveCopy,
                    "--resref",
                    "test2da",
                    "--restype",
                    "2da",
                    "--source",
                    SampleTwoDaPath);
                Assert.That(injectResult.GetProperty("ok").GetBoolean(), Is.True);

                var readResult = RunBridge("read", archiveCopy);
                Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
                int? listingSize = null;
                foreach (JsonElement resource in readResult.GetProperty("payload").GetProperty("resources").EnumerateArray())
                {
                    if (resource.GetProperty("resref").GetString() == "test2da"
                        && resource.GetProperty("restype").GetString() == "2da")
                    {
                        listingSize = resource.GetProperty("size").GetInt32();
                        break;
                    }
                }

                Assert.That(listingSize, Is.Not.Null);
                Assert.That(listingSize!.Value, Is.EqualTo(expectedSize));
            }
            finally
            {
                if (File.Exists(archiveCopy))
                {
                    File.Delete(archiveCopy);
                }
            }
        }

        [Test]
        public void Write_RoundTripsSampleTlk()
        {
            if (!File.Exists(SampleTlkPath))
            {
                Assert.Ignore($"Fixture not found at {SampleTlkPath}");
            }

            var readResult = RunBridge("read", SampleTlkPath);
            Assert.That(readResult.GetProperty("ok").GetBoolean(), Is.True);
            var payload = readResult.GetProperty("payload").GetRawText();

            string tempPath = Path.Combine(Path.GetTempPath(), "kotor_bridge_tlk_" + Guid.NewGuid() + ".tlk");
            try
            {
                var writeResult = RunBridge("write", tempPath, "--payload", payload);
                Assert.That(writeResult.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(File.Exists(tempPath), Is.True);

                var reread = RunBridge("read", tempPath);
                Assert.That(reread.GetProperty("ok").GetBoolean(), Is.True);
                Assert.That(
                    reread.GetProperty("payload").GetProperty("data").GetProperty("strings").GetArrayLength(),
                    Is.EqualTo(2));
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        private static bool PyKotorImportWorks()
        {
            var probe = RunBridgeRaw("supported-types");
            return probe.ExitCode == 0 && probe.Json.TryGetProperty("ok", out var ok) && ok.GetBoolean();
        }

        private static JsonElement RunBridge(params string[] args)
        {
            var raw = RunBridgeRaw(args);
            Assert.That(raw.ExitCode, Is.EqualTo(0), () => raw.StdOut + raw.StdErr);
            return raw.Json;
        }

        private static (int ExitCode, string StdOut, string StdErr, JsonElement Json) RunBridgeRaw(params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            psi.ArgumentList.Add(BridgeScriptPath);
            foreach (string arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            using var process = Process.Start(psi);
            Assert.That(process, Is.Not.Null);
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string jsonLine = ExtractJsonLine(stdout);
            using var doc = JsonDocument.Parse(jsonLine);
            return (process.ExitCode, stdout, stderr, doc.RootElement.Clone());
        }

        private static string ExtractJsonLine(string stdout)
        {
            string[] lines = stdout.Split('\n');
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.StartsWith("{", StringComparison.Ordinal))
                {
                    return trimmed;
                }
            }

            throw new InvalidOperationException("Bridge did not emit JSON on stdout: " + stdout);
        }
    }
}
