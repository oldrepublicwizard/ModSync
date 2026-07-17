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
    [Collection(MainConfigStaticState.CollectionName)]
    public sealed class StepNavigationServiceTests : IDisposable
    {
        private readonly List<string> _directoriesToCleanup = new List<string>();

        [Fact(DisplayName = "GetCurrentIncompleteStep returns 1 when directories are not configured")]
        public void GetCurrentIncompleteStep_MissingPaths_Returns1()
        {
            lock (MainConfigStaticState.Gate)
            {
                MainConfigStaticState.Reset();
                StepNavigationService service = CreateService();

                Assert.Equal(1, service.GetCurrentIncompleteStep());
            }
        }

        [Fact(DisplayName = "GetCurrentIncompleteStep returns 2 when step 1 is complete but no mods are loaded")]
        public void GetCurrentIncompleteStep_NoComponents_Returns2()
        {
            lock (MainConfigStaticState.Gate)
            {
                MainConfigStaticState.Reset();
                ConfigureValidPaths();
                StepNavigationService service = CreateService();

                Assert.Equal(2, service.GetCurrentIncompleteStep());
            }
        }

        [Fact(DisplayName = "GetCurrentIncompleteStep returns 3 when mods are loaded but none are selected")]
        public void GetCurrentIncompleteStep_NoSelection_Returns3()
        {
            lock (MainConfigStaticState.Gate)
            {
                MainConfigStaticState.Reset();
                ConfigureValidPaths();
                MainConfig.AllComponents = new List<ModComponent>
                {
                    new ModComponent { Name = "Unselected Mod", IsSelected = false },
                };
                StepNavigationService service = CreateService();

                Assert.Equal(3, service.GetCurrentIncompleteStep());
            }
        }

        [Fact(DisplayName = "GetCurrentIncompleteStep returns 4 when selected mods are not downloaded")]
        public void GetCurrentIncompleteStep_SelectedNotDownloaded_Returns4()
        {
            lock (MainConfigStaticState.Gate)
            {
                MainConfigStaticState.Reset();
                ConfigureValidPaths();
                MainConfig.AllComponents = new List<ModComponent>
                {
                    new ModComponent { Name = "Pending Mod", IsSelected = true, IsDownloaded = false },
                };
                StepNavigationService service = CreateService();

                Assert.Equal(4, service.GetCurrentIncompleteStep());
            }
        }

        [Fact(DisplayName = "GetCurrentIncompleteStep returns 5 when selected mods are downloaded")]
        public void GetCurrentIncompleteStep_AllDownloaded_Returns5()
        {
            lock (MainConfigStaticState.Gate)
            {
                MainConfigStaticState.Reset();
                ConfigureValidPaths();
                MainConfig.AllComponents = new List<ModComponent>
                {
                    new ModComponent { Name = "Ready Mod", IsSelected = true, IsDownloaded = true },
                };
                StepNavigationService service = CreateService();

                Assert.Equal(5, service.GetCurrentIncompleteStep());
            }
        }

        [Fact(DisplayName = "Constructor rejects null MainConfig")]
        public void Constructor_NullMainConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepNavigationService(null, new ValidationService(new MainConfig())));
        }

        [Fact(DisplayName = "Constructor rejects null ValidationService")]
        public void Constructor_NullValidationService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StepNavigationService(new MainConfig(), null));
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

            MainConfigStaticState.Reset();
        }

        private StepNavigationService CreateService()
        {
            MainConfig config = MainConfig.Instance ?? new MainConfig();
            return new StepNavigationService(config, new ValidationService(config));
        }

        private void ConfigureValidPaths()
        {
            string sourceDirectory = CreateTempDirectory();
            string destinationDirectory = CreateTempDirectory();
            File.WriteAllText(Path.Combine(destinationDirectory, "swkotor.exe"), string.Empty);

            var config = new MainConfig
            {
                sourcePath = new DirectoryInfo(sourceDirectory),
                destinationPath = new DirectoryInfo(destinationDirectory),
            };
            MainConfig.Instance = config;
        }

        private string CreateTempDirectory()
        {
            string path = Path.Combine(Path.GetTempPath(), $"StepNavTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(path);
            _directoriesToCleanup.Add(path);
            return path;
        }
    }
}
