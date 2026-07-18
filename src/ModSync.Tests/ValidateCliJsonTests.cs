// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

using ModSync.Core;
using ModSync.Core.CLI;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ValidateCliJsonTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "ModSync_ValidateJson_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            string gameDir = Path.Combine(_tempDir, "game");
            string modDir = Path.Combine(_tempDir, "mods");
            Directory.CreateDirectory(gameDir);
            Directory.CreateDirectory(modDir);
            File.WriteAllText(Path.Combine(gameDir, "swkotor.exe"), string.Empty);
            File.WriteAllText(Path.Combine(modDir, "payload.txt"), "payload");

            MainConfig.Instance = new MainConfig
            {
                destinationPath = new DirectoryInfo(gameDir),
                sourcePath = new DirectoryInfo(modDir),
            };

            EnsureHolopatcherInTestResources();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Test]
        public void Validate_OutputJson_DryRunOnly_WritesOnlyJsonDocumentToStdout()
        {
            string componentGuid = Guid.NewGuid().ToString();
            string instructionGuid = Guid.NewGuid().ToString();
            string tomlPath = Path.Combine(_tempDir, "dry_run_mod.toml");
            File.WriteAllText(
                tomlPath,
                $@"[[thisMod]]
Guid = ""{componentGuid}""
Name = ""JsonDryRunMod""
IsSelected = true
Tier = ""1 - Essential""
Category = [""Test""]

[[thisMod.Instructions]]
Guid = ""{instructionGuid}""
Action = ""Move""
Source = [""<<modDirectory>>\\payload.txt""]
Destination = ""<<kotorDirectory>>\\Override""
");

            string gameDir = MainConfig.DestinationPath.FullName;
            string modDir = MainConfig.SourcePath.FullName;

            int exitCode = RunValidateWithCapturedStdout(new[]
            {
                "validate",
                "-i", tomlPath,
                "-g", gameDir,
                "-s", modDir,
                "--dry-run-only",
                "--output", "json",
                "--use-file-selection",
            }, out string stdout);

            Assert.That(stdout, Does.StartWith("{"));
            Assert.That(stdout, Does.EndWith("}"));

            using JsonDocument document = JsonDocument.Parse(stdout);
            JsonElement root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(0));
                Assert.That(root.GetProperty("success").GetBoolean(), Is.True);
                Assert.That(root.GetProperty("inputPath").GetString(), Is.EqualTo(tomlPath));
                Assert.That(root.GetProperty("stages").GetArrayLength(), Is.GreaterThan(0));
            });
        }

        [Test]
        public void Validate_OutputJson_MissingInputFile_ReturnsErrorEnvelope()
        {
            string missingPath = Path.Combine(_tempDir, "definitely_missing.toml");

            int exitCode = RunValidateWithCapturedStdout(new[]
            {
                "validate",
                "-i", missingPath,
                "--output", "json",
            }, out string stdout);

            Assert.That(stdout, Does.StartWith("{"));

            using JsonDocument document = JsonDocument.Parse(stdout);
            JsonElement root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(root.GetProperty("success").GetBoolean(), Is.False);
                Assert.That(root.GetProperty("error").GetString(), Does.Contain("definitely_missing.toml"));
            });
        }

        private static int RunValidateWithCapturedStdout(string[] args, out string stdout)
        {
            var writer = new StringWriter();
            TextWriter previousOut = Console.Out;
            Console.SetOut(writer);
            try
            {
                return ModBuildConverter.Run(args);
            }
            finally
            {
                Console.SetOut(previousOut);
                stdout = writer.ToString().Trim();
            }
        }

        private static void EnsureHolopatcherInTestResources()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesDir = Path.Combine(baseDir, "Resources");
            Directory.CreateDirectory(resourcesDir);
            string targetPath = Path.Combine(resourcesDir, "holopatcher");
            if (File.Exists(targetPath))
            {
                return;
            }

            string vendorHolopatcher = Path.GetFullPath(Path.Combine(
                baseDir,
                "..", "..", "..", "..", "..",
                "vendor", "bin", "HoloPatcher_linux"));
            if (!File.Exists(vendorHolopatcher))
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                File.Copy(vendorHolopatcher, targetPath, overwrite: true);
            }
            else
            {
                File.CreateSymbolicLink(targetPath, vendorHolopatcher);
            }
        }
    }
}
