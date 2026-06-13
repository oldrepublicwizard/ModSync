// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Core.CLI;
using ModSync.Core.Services.Fomod;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class FomodPostDownloadOptionsResolverTests
    {
        [Test]
        public void Resolve_FomodSkip_ReturnsSkipAll()
        {
            FomodPostDownloadOptions options = FomodPostDownloadOptionsResolver.Resolve(
                fomodSkip: true,
                fomodChoicesPath: null,
                forceInteractive: false,
                forceNonInteractive: false,
                settingsMode: null);

            Assert.That(options.Mode, Is.EqualTo(FomodPostDownloadMode.SkipAll));
        }

        [Test]
        public void Resolve_NonInteractiveDefault_ReturnsWarnContinue()
        {
            FomodPostDownloadOptions options = FomodPostDownloadOptionsResolver.Resolve(
                fomodSkip: false,
                fomodChoicesPath: null,
                forceInteractive: false,
                forceNonInteractive: true,
                settingsMode: null);

            Assert.That(options.Mode, Is.EqualTo(FomodPostDownloadMode.WarnContinue));
        }
    }
}
