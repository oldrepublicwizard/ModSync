// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Text.Json;

using ModSync.Core.Services.Validation;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ValidationPipelineJsonFormatterTests
    {
        [Test]
        public void SerializeReport_IncludesStageSummaries()
        {
            var pipelineResult = new ValidationPipelineResult
            {
                IsSuccess = false,
                ErrorCount = 1,
                WarningCount = 0,
                PassedCount = 0,
            };
            pipelineResult.Stages.Add(new ValidationPipelineStageResult
            {
                Stage = ValidationPipelineStage.ComponentValidation,
                Passed = false,
                Summary = "1 component failed validation",
                Messages = { "ERROR: MissingArchiveMod: archive missing" },
            });

            string json = ValidationPipelineJsonFormatter.SerializeReport(pipelineResult, componentCount: 1, inputPath: "test.toml");
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("success").GetBoolean(), Is.False);
                Assert.That(root.GetProperty("exitCode").GetInt32(), Is.EqualTo(1));
                Assert.That(root.GetProperty("componentCount").GetInt32(), Is.EqualTo(1));
                Assert.That(root.GetProperty("inputPath").GetString(), Is.EqualTo("test.toml"));
                JsonElement stages = root.GetProperty("stages");
                Assert.That(stages.GetArrayLength(), Is.EqualTo(1));
                Assert.That(stages[0].GetProperty("stage").GetString(), Is.EqualTo("ComponentValidation"));
                Assert.That(stages[0].GetProperty("passed").GetBoolean(), Is.False);
            });
        }

        [Test]
        public void SerializeError_ReturnsFailureEnvelope()
        {
            string json = ValidationPipelineJsonFormatter.SerializeError("Input file not found: missing.toml");
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(root.GetProperty("success").GetBoolean(), Is.False);
                Assert.That(root.GetProperty("exitCode").GetInt32(), Is.EqualTo(1));
                Assert.That(root.GetProperty("error").GetString(), Does.Contain("missing.toml"));
            });
        }
    }
}
