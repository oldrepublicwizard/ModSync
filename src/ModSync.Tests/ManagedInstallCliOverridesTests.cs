// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Core.Services.Installation;
using ModSync.Core.Services.Settings;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ManagedInstallCliOverridesTests
    {
        [Test]
        public void Apply_EnableManaged_SetsFlagAndRequiresProfile()
        {
            var settings = new ModSyncSettings();
            string error = ManagedInstallCliOverrides.Apply(settings, enableManaged: true, disableManaged: false, profileName: null);

            Assert.That(settings.ManagedDeploymentEnabled, Is.True);
            Assert.That(error, Does.Contain("no active profile"));
        }

        [Test]
        public void Apply_EnableManagedWithProfile_Succeeds()
        {
            var settings = new ModSyncSettings { ActiveProfileName = "Old" };
            string error = ManagedInstallCliOverrides.Apply(
                settings,
                enableManaged: true,
                disableManaged: false,
                profileName: "CLI Profile");

            Assert.That(error, Is.Null);
            Assert.That(settings.ManagedDeploymentEnabled, Is.True);
            Assert.That(settings.ActiveProfileName, Is.EqualTo("CLI Profile"));
        }

        [Test]
        public void Apply_DisableManaged_ClearsManagedEvenIfSettingsHadItOn()
        {
            var settings = new ModSyncSettings
            {
                ManagedDeploymentEnabled = true,
                ActiveProfileName = "Alpha",
            };

            string error = ManagedInstallCliOverrides.Apply(
                settings,
                enableManaged: false,
                disableManaged: true,
                profileName: null);

            Assert.That(error, Is.Null);
            Assert.That(settings.ManagedDeploymentEnabled, Is.False);
        }

        [Test]
        public void Apply_ConflictingFlags_ReturnsError()
        {
            var settings = new ModSyncSettings();
            string error = ManagedInstallCliOverrides.Apply(
                settings,
                enableManaged: true,
                disableManaged: true,
                profileName: "X");

            Assert.That(error, Does.Contain("--managed").And.Contain("--no-managed"));
        }

        [Test]
        public void ResolveManagedOverride_MapsFlags()
        {
            Assert.That(ManagedInstallCliOverrides.ResolveManagedOverride(true, false), Is.True);
            Assert.That(ManagedInstallCliOverrides.ResolveManagedOverride(false, true), Is.False);
            Assert.That(ManagedInstallCliOverrides.ResolveManagedOverride(false, false), Is.Null);
            Assert.That(ManagedInstallCliOverrides.ResolveManagedOverride(true, true), Is.Null);
        }

        [Test]
        public void Apply_ProfileOnly_UpdatesNameWithoutForcingManaged()
        {
            var settings = new ModSyncSettings { ManagedDeploymentEnabled = false };
            string error = ManagedInstallCliOverrides.Apply(
                settings,
                enableManaged: false,
                disableManaged: false,
                profileName: "Beta");

            Assert.That(error, Is.Null);
            Assert.That(settings.ManagedDeploymentEnabled, Is.False);
            Assert.That(settings.ActiveProfileName, Is.EqualTo("Beta"));
        }
    }
}
