// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ModSync.Core;
using ModSync.Core.Services.Profiles;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ProfileServiceTests
    {
        private string _storageDir;
        private ProfileService _service;

        private DirectoryInfo _savedSourcePath;
        private DirectoryInfo _savedDestinationPath;

        [SetUp]
        public void SetUp()
        {
            _storageDir = Path.Combine(Path.GetTempPath(), "ModSync_ProfileTests_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(_storageDir);
            _service = new ProfileService(_storageDir);

            // Other tests mutate MainConfig statics too; save and restore around each test.
            _savedSourcePath = MainConfig.SourcePath;
            _savedDestinationPath = MainConfig.DestinationPath;
        }

        [TearDown]
        public void TearDown()
        {
            MainConfig.Instance.sourcePath = _savedSourcePath;
            MainConfig.Instance.destinationPath = _savedDestinationPath;

            try
            {
                if (Directory.Exists(_storageDir))
                {
                    Directory.Delete(_storageDir, recursive: true);
                }
            }
            catch
            {
                // Best-effort temp cleanup.
            }
        }

        private static ModComponent MakeComponent(string name, bool isSelected, params bool[] optionSelections)
        {
            var component = new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = name,
                IsSelected = isSelected,
            };
            for (int i = 0; i < optionSelections.Length; i++)
            {
                component.Options.Add(new Option
                {
                    Guid = Guid.NewGuid(),
                    Name = $"{name} option {i}",
                    IsSelected = optionSelections[i],
                });
            }

            return component;
        }

        [Test]
        public void CreateProfile_PersistsFile_AndListProfilesReturnsIt()
        {
            Profile created = _service.CreateProfile("My First Profile");

            Assert.That(created.Name, Is.EqualTo("My First Profile"));
            Assert.That(created.CreatedUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(File.Exists(Path.Combine(_service.ProfilesDirectory, "My First Profile.json")), Is.True);

            List<Profile> listed = _service.ListProfiles();
            Assert.That(listed, Has.Count.EqualTo(1));
            Assert.That(listed[0].Name, Is.EqualTo("My First Profile"));
        }

        [Test]
        public void CreateProfile_DuplicateName_Throws()
        {
            _ = _service.CreateProfile("Duplicate");
            _ = Assert.Throws<InvalidOperationException>(() => _service.CreateProfile("Duplicate"));
        }

        [Test]
        public void ListProfiles_EmptyDirectory_ReturnsEmptyList()
        {
            Assert.That(_service.ListProfiles(), Is.Empty);
        }

        [Test]
        public void ListProfiles_SkipsCorruptFiles_AndSortsByName()
        {
            _ = _service.CreateProfile("Bravo");
            _ = _service.CreateProfile("alpha");
            File.WriteAllText(Path.Combine(_service.ProfilesDirectory, "corrupt.json"), "{ not valid json !");

            List<Profile> listed = _service.ListProfiles();

            Assert.That(listed.Select(p => p.Name), Is.EqualTo(new[] { "alpha", "Bravo" }));
        }

        [Test]
        public void SaveProfile_LoadProfile_RoundTripsAllFields()
        {
            Guid componentGuid = Guid.NewGuid();
            Guid optionGuid = Guid.NewGuid();
            var profile = new Profile
            {
                Name = "RoundTrip",
                KotorDirectory = "/games/kotor",
                ModDirectory = "/mods/kotor",
                InstructionFilePath = "/mods/kotor/instructions.toml",
                CreatedUtc = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                LastUsedUtc = new DateTime(2026, 6, 7, 8, 9, 10, DateTimeKind.Utc),
            };
            profile.ComponentSelections[componentGuid] = new ProfileComponentSelection
            {
                IsSelected = true,
                SelectedOptionGuids = new List<Guid> { optionGuid },
            };

            _service.SaveProfile(profile);
            Profile loaded = _service.LoadProfile("RoundTrip");

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.Name, Is.EqualTo("RoundTrip"));
            Assert.That(loaded.KotorDirectory, Is.EqualTo("/games/kotor"));
            Assert.That(loaded.ModDirectory, Is.EqualTo("/mods/kotor"));
            Assert.That(loaded.InstructionFilePath, Is.EqualTo("/mods/kotor/instructions.toml"));
            Assert.That(loaded.CreatedUtc, Is.EqualTo(profile.CreatedUtc));
            Assert.That(loaded.LastUsedUtc, Is.EqualTo(profile.LastUsedUtc));
            Assert.That(loaded.ComponentSelections, Has.Count.EqualTo(1));
            Assert.That(loaded.ComponentSelections[componentGuid].IsSelected, Is.True);
            Assert.That(loaded.ComponentSelections[componentGuid].SelectedOptionGuids, Is.EqualTo(new[] { optionGuid }));
        }

        [Test]
        public void SaveProfile_OverwritesExistingFile_Atomically()
        {
            Profile profile = _service.CreateProfile("Overwrite");
            profile.KotorDirectory = "/first";
            _service.SaveProfile(profile);
            profile.KotorDirectory = "/second";
            _service.SaveProfile(profile);

            Profile loaded = _service.LoadProfile("Overwrite");
            Assert.That(loaded.KotorDirectory, Is.EqualTo("/second"));

            // No temp files may be left behind by the atomic write.
            Assert.That(Directory.GetFiles(_service.ProfilesDirectory, "*.tmp"), Is.Empty);
        }

        [Test]
        public void LoadProfile_MissingProfile_ReturnsNull()
        {
            Assert.That(_service.LoadProfile("DoesNotExist"), Is.Null);
        }

        [Test]
        public void CloneProfile_CopiesData_IndependentOfSource()
        {
            Guid componentGuid = Guid.NewGuid();
            Profile source = _service.CreateProfile("Source");
            source.ModDirectory = "/mods";
            source.ComponentSelections[componentGuid] = new ProfileComponentSelection { IsSelected = true };
            _service.SaveProfile(source);

            Profile clone = _service.CloneProfile(source, "Copy");

            Assert.That(clone.Name, Is.EqualTo("Copy"));
            Assert.That(clone.ModDirectory, Is.EqualTo("/mods"));
            Assert.That(clone.ComponentSelections[componentGuid].IsSelected, Is.True);

            // Mutating the clone must not affect the persisted source.
            clone.ComponentSelections[componentGuid].IsSelected = false;
            _service.SaveProfile(clone);
            Profile reloadedSource = _service.LoadProfile("Source");
            Assert.That(reloadedSource.ComponentSelections[componentGuid].IsSelected, Is.True);
            Assert.That(_service.ListProfiles(), Has.Count.EqualTo(2));
        }

        [Test]
        public void CloneProfile_ExistingTargetName_Throws()
        {
            Profile source = _service.CreateProfile("CloneSource");
            _ = _service.CreateProfile("Taken");
            _ = Assert.Throws<InvalidOperationException>(() => _service.CloneProfile(source, "Taken"));
        }

        [Test]
        public void RenameProfile_MovesFile_AndKeepsData()
        {
            Profile profile = _service.CreateProfile("OldName");
            profile.KotorDirectory = "/kept";
            _service.SaveProfile(profile);

            Profile renamed = _service.RenameProfile("OldName", "NewName");

            Assert.That(renamed.Name, Is.EqualTo("NewName"));
            Assert.That(renamed.KotorDirectory, Is.EqualTo("/kept"));
            Assert.That(_service.LoadProfile("OldName"), Is.Null);
            Assert.That(_service.LoadProfile("NewName"), Is.Not.Null);
            Assert.That(_service.ListProfiles(), Has.Count.EqualTo(1));
        }

        [Test]
        public void RenameProfile_TargetExists_Throws()
        {
            _ = _service.CreateProfile("RenameMe");
            _ = _service.CreateProfile("AlreadyHere");
            _ = Assert.Throws<InvalidOperationException>(() => _service.RenameProfile("RenameMe", "AlreadyHere"));
        }

        [Test]
        public void DeleteProfile_RemovesFile_AndReturnsFalseWhenMissing()
        {
            _ = _service.CreateProfile("Doomed");
            Assert.That(_service.DeleteProfile("Doomed"), Is.True);
            Assert.That(_service.ListProfiles(), Is.Empty);
            Assert.That(_service.DeleteProfile("Doomed"), Is.False);
        }

        [Test]
        public void CaptureFromCurrentState_ReadsMainConfigAndComponentSelections()
        {
            string modDir = Path.Combine(_storageDir, "mods");
            string kotorDir = Path.Combine(_storageDir, "kotor");
            MainConfig.Instance.sourcePath = new DirectoryInfo(modDir);
            MainConfig.Instance.destinationPath = new DirectoryInfo(kotorDir);

            ModComponent selectedWithOption = MakeComponent("Selected", isSelected: true, true, false);
            ModComponent unselected = MakeComponent("Unselected", isSelected: false);
            var components = new List<ModComponent> { selectedWithOption, unselected };

            Profile captured = _service.CaptureFromCurrentState("Captured", components, "/path/to/instructions.toml");

            Assert.That(captured.ModDirectory, Is.EqualTo(modDir));
            Assert.That(captured.KotorDirectory, Is.EqualTo(kotorDir));
            Assert.That(captured.InstructionFilePath, Is.EqualTo("/path/to/instructions.toml"));
            Assert.That(captured.ComponentSelections, Has.Count.EqualTo(2));
            Assert.That(captured.ComponentSelections[selectedWithOption.Guid].IsSelected, Is.True);
            Assert.That(
                captured.ComponentSelections[selectedWithOption.Guid].SelectedOptionGuids,
                Is.EqualTo(new[] { selectedWithOption.Options[0].Guid }));
            Assert.That(captured.ComponentSelections[unselected.Guid].IsSelected, Is.False);
            Assert.That(captured.ComponentSelections[unselected.Guid].SelectedOptionGuids, Is.Empty);

            // Capture also persists.
            Assert.That(_service.LoadProfile("Captured"), Is.Not.Null);
        }

        [Test]
        public void ApplyProfile_WritesMainConfigPaths_AndSelectionFlags()
        {
            string modDir = Path.Combine(_storageDir, "applied_mods");
            string kotorDir = Path.Combine(_storageDir, "applied_kotor");

            ModComponent component = MakeComponent("Comp", isSelected: false, false, true);
            ModComponent untouched = MakeComponent("Untouched", isSelected: true);

            var profile = new Profile
            {
                Name = "Apply",
                ModDirectory = modDir,
                KotorDirectory = kotorDir,
            };
            profile.ComponentSelections[component.Guid] = new ProfileComponentSelection
            {
                IsSelected = true,
                SelectedOptionGuids = new List<Guid> { component.Options[0].Guid },
            };

            DateTime beforeApply = DateTime.UtcNow;
            _service.ApplyProfile(profile, new List<ModComponent> { component, untouched });

            Assert.That(MainConfig.SourcePath?.FullName, Is.EqualTo(modDir));
            Assert.That(MainConfig.DestinationPath?.FullName, Is.EqualTo(kotorDir));
            Assert.That(component.IsSelected, Is.True);
            Assert.That(component.Options[0].IsSelected, Is.True);
            Assert.That(component.Options[1].IsSelected, Is.False, "Options not in the profile must be deselected");
            Assert.That(untouched.IsSelected, Is.True, "Components without a profile entry are left untouched");
            Assert.That(profile.LastUsedUtc, Is.GreaterThanOrEqualTo(beforeApply));

            // ApplyProfile persists the LastUsedUtc bump.
            Profile reloaded = _service.LoadProfile("Apply");
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded.LastUsedUtc, Is.GreaterThanOrEqualTo(beforeApply));
        }

        [Test]
        public void CaptureThenApply_RoundTripsSelectionState()
        {
            MainConfig.Instance.sourcePath = new DirectoryInfo(Path.Combine(_storageDir, "rt_mods"));
            MainConfig.Instance.destinationPath = new DirectoryInfo(Path.Combine(_storageDir, "rt_kotor"));

            ModComponent component = MakeComponent("RoundTrip", isSelected: true, true, false);
            var components = new List<ModComponent> { component };
            Profile captured = _service.CaptureFromCurrentState("RT", components);

            // Mutate live state, then re-apply the profile to restore it.
            component.IsSelected = false;
            component.Options[0].IsSelected = false;
            component.Options[1].IsSelected = true;

            Profile reloaded = _service.LoadProfile("RT");
            _service.ApplyProfile(reloaded, components);

            Assert.That(component.IsSelected, Is.True);
            Assert.That(component.Options[0].IsSelected, Is.True);
            Assert.That(component.Options[1].IsSelected, Is.False);
            Assert.That(captured.ComponentSelections, Has.Count.EqualTo(1));
        }

        [Test]
        public void SanitizeProfileFileName_ReplacesInvalidCharacters()
        {
            string sanitized = ProfileService.SanitizeProfileFileName("My/Profile:With*Bad?Chars");

            Assert.That(sanitized, Does.Not.Contain("/"));
            Assert.That(sanitized.IndexOfAny(Path.GetInvalidFileNameChars()), Is.EqualTo(-1));
            Assert.That(sanitized, Is.EqualTo("My_Profile_With_Bad_Chars").Or.EqualTo("My_Profile:With*Bad?Chars".Replace('/', '_')));
        }

        [Test]
        public void SanitizeProfileFileName_PathTraversalName_ProducesSafeFileInsideProfilesDir()
        {
            Profile profile = _service.CreateProfile("..\\..\\evil/../name");

            string[] files = Directory.GetFiles(_service.ProfilesDirectory);
            Assert.That(files, Has.Length.EqualTo(1));
            Assert.That(Path.GetDirectoryName(files[0]), Is.EqualTo(_service.ProfilesDirectory));
            Assert.That(_service.LoadProfile(profile.Name), Is.Not.Null);
        }

        [Test]
        public void SanitizeProfileFileName_WhitespaceOrUnusableNames_Throw()
        {
            _ = Assert.Throws<ArgumentException>(() => ProfileService.SanitizeProfileFileName("   "));
            _ = Assert.Throws<ArgumentException>(() => ProfileService.SanitizeProfileFileName(".."));
            _ = Assert.Throws<ArgumentException>(() => ProfileService.SanitizeProfileFileName("///"));
        }
    }
}
