// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ApplicationLaunchCoordinatorTests
    {
        [Test]
        public void DecideSecondaryAction_WithProtocolUrl_ForwardsProtocolUrl()
        {
            SecondaryLaunchAction action = ApplicationLaunchCoordinator.DecideSecondaryAction(true, false);
            Assert.That(action, Is.EqualTo(SecondaryLaunchAction.ForwardProtocolUrlAndExit));
        }

        [Test]
        public void DecideSecondaryAction_WithoutProtocolUrl_ForwardsActivate()
        {
            SecondaryLaunchAction action = ApplicationLaunchCoordinator.DecideSecondaryAction(false, false);
            Assert.That(action, Is.EqualTo(SecondaryLaunchAction.ForwardActivateAndExit));
        }

        [Test]
        public void DecideSecondaryAction_AllowMultiple_StartsNewInstance()
        {
            SecondaryLaunchAction action = ApplicationLaunchCoordinator.DecideSecondaryAction(false, true);
            Assert.That(action, Is.EqualTo(SecondaryLaunchAction.StartNewInstance));
        }
    }
}
