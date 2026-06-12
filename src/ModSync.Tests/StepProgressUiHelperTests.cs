// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    public sealed class StepProgressUiHelperTests : IDisposable
    {
        private readonly List<string> _directoriesToCleanup = new List<string>();

        [Fact(DisplayName = "GetCurrentIncompleteStep maps preparation flags to step numbers")]
        public void GetCurrentIncompleteStep_MapsFlags()
        {
            Assert.Equal(1, StepProgressUiHelper.GetCurrentIncompleteStep(false, false, false, false));
            Assert.Equal(2, StepProgressUiHelper.GetCurrentIncompleteStep(true, false, false, false));
            Assert.Equal(3, StepProgressUiHelper.GetCurrentIncompleteStep(true, true, false, false));
            Assert.Equal(4, StepProgressUiHelper.GetCurrentIncompleteStep(true, true, true, false));
            Assert.Equal(5, StepProgressUiHelper.GetCurrentIncompleteStep(true, true, true, true));
        }

        [Fact(DisplayName = "FormatGettingStartedProgressMessage clamps to final ready message")]
        public void FormatGettingStartedProgressMessage_ClampsToReadyMessage()
        {
            Assert.Equal(
                "🎉 All preparation steps completed! You're ready to install mods",
                StepProgressUiHelper.FormatGettingStartedProgressMessage(completedSteps: 99));
        }

        [Fact(DisplayName = "ComputeStep5Complete requires validation checkbox and passing mods")]
        public void ComputeStep5Complete_RequiresCheckboxAndValidMods()
        {
            var components = new List<ModComponent>
            {
                new ModComponent { Name = "Valid", IsSelected = true },
            };

            Assert.False(StepProgressUiHelper.ComputeStep5Complete(
                step4Complete: true,
                components,
                _ => true,
                validationCheckboxChecked: false));

            Assert.True(StepProgressUiHelper.ComputeStep5Complete(
                step4Complete: true,
                components,
                _ => true,
                validationCheckboxChecked: true));

            Assert.False(StepProgressUiHelper.ComputeStep5Complete(
                step4Complete: true,
                components,
                _ => false,
                validationCheckboxChecked: true));
        }

        [Fact(DisplayName = "ComputePreparationSteps returns step 2 when paths are configured")]
        public void ComputePreparationSteps_WithPaths_ReturnsStep2Ready()
        {
            ResetMainConfigState();
            ConfigureValidPaths();

            (bool step1, bool step2, bool _, bool _) =
                StepProgressUiHelper.ComputePreparationSteps(new List<ModComponent>());

            Assert.True(step1);
            Assert.False(step2);
        }

        public void Dispose()
        {
            foreach (string directory in _directoriesToCleanup)
            {
                if (Directory.Exists(directory))
                {
                    try
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                    catch
                    {
                        // Best-effort test cleanup.
                    }
                }
            }

            ResetMainConfigState();
        }

        private void ConfigureValidPaths()
        {
            string sourceDirectory = CreateTempDirectory();
            string destinationDirectory = CreateTempDirectory();
            File.WriteAllText(Path.Combine(destinationDirectory, "swkotor.exe"), string.Empty);

            MainConfig.Instance = new MainConfig
            {
                sourcePath = new DirectoryInfo(sourceDirectory),
                destinationPath = new DirectoryInfo(destinationDirectory),
            };
        }

        private string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), $"StepProgressTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            _directoriesToCleanup.Add(path);
            return path;
        }

        private static void ResetMainConfigState()
        {
            MainConfig.AllComponents = new List<ModComponent>();
            MainConfig.Instance = new MainConfig();
        }
    }
}
