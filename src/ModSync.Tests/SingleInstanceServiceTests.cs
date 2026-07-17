// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class SingleInstanceServiceTests
    {
        private static string UniquePipeName()
        {
            return $"ModSync-Test-{Guid.NewGuid():N}";
        }

        [Test]
        public void TryBecomePrimary_FirstClaim_Succeeds()
        {
            using (var service = new SingleInstanceService(UniquePipeName()))
            {
                Assert.That(service.TryBecomePrimary(), Is.True);
                Assert.That(service.IsPrimary, Is.True);
            }
        }

        [Test]
        public void TryBecomePrimary_SecondClaimOnSamePipe_Fails()
        {
            string pipeName = UniquePipeName();
            using (var primary = new SingleInstanceService(pipeName))
            using (var secondary = new SingleInstanceService(pipeName))
            {
                Assert.That(primary.TryBecomePrimary(), Is.True);
                Assert.That(secondary.TryBecomePrimary(), Is.False);
                Assert.That(secondary.IsPrimary, Is.False);
            }
        }

        [Test]
        public void TryBecomePrimary_CalledTwiceOnSameInstance_StaysPrimary()
        {
            using (var service = new SingleInstanceService(UniquePipeName()))
            {
                Assert.That(service.TryBecomePrimary(), Is.True);
                Assert.That(service.TryBecomePrimary(), Is.True);
            }
        }

        [Test]
        public async Task SendToPrimaryAsync_RoundTrip_DeliversMessage()
        {
            string pipeName = UniquePipeName();
            string sentMessage = $"hello-{Guid.NewGuid():N}";
            string receivedMessage = null;
            using (var received = new ManualResetEventSlim(false))
            using (var primary = new SingleInstanceService(pipeName))
            using (var secondary = new SingleInstanceService(pipeName))
            {
                primary.MessageReceived += (sender, message) =>
                {
                    receivedMessage = message;
                    received.Set();
                };

                Assert.That(primary.TryBecomePrimary(), Is.True);

                bool sent = await secondary.SendToPrimaryAsync(sentMessage);

                Assert.That(sent, Is.True);
                Assert.That(received.Wait(TimeSpan.FromSeconds(10)), Is.True, "Primary did not receive the forwarded message in time");
                Assert.That(receivedMessage, Is.EqualTo(sentMessage));
            }
        }

        [Test]
        public async Task SendToPrimaryAsync_NxmMessage_FlowsIntoHandoffQueue()
        {
            string pipeName = UniquePipeName();
            string nxmUrl = $"nxm://kotor/mods/777/files/{Environment.TickCount}?key=abc&expires=123";
            using (var received = new ManualResetEventSlim(false))
            using (var primary = new SingleInstanceService(pipeName))
            using (var secondary = new SingleInstanceService(pipeName))
            {
                _ = NxmHandoffQueue.DrainAll();
                primary.MessageReceived += (sender, message) => received.Set();

                Assert.That(primary.TryBecomePrimary(), Is.True);

                bool sent = await secondary.SendToPrimaryAsync(nxmUrl);

                Assert.That(sent, Is.True);
                Assert.That(received.Wait(TimeSpan.FromSeconds(10)), Is.True, "Primary did not receive the forwarded message in time");
                Assert.That(NxmHandoffQueue.TryDequeue(out string queued), Is.True);
                Assert.That(queued, Is.EqualTo(nxmUrl));
            }
        }

        [Test]
        public async Task SendToPrimaryAsync_ModSyncMessage_FlowsIntoHandoffQueue()
        {
            string pipeName = UniquePipeName();
            string modSyncUrl = "modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml";
            using (var received = new ManualResetEventSlim(false))
            using (var primary = new SingleInstanceService(pipeName))
            using (var secondary = new SingleInstanceService(pipeName))
            {
                _ = ModSyncHandoffQueue.DrainAll();
                primary.MessageReceived += (sender, message) => received.Set();

                Assert.That(primary.TryBecomePrimary(), Is.True);

                bool sent = await secondary.SendToPrimaryAsync(modSyncUrl);

                Assert.That(sent, Is.True);
                Assert.That(received.Wait(TimeSpan.FromSeconds(10)), Is.True, "Primary did not receive the forwarded message in time");
                Assert.That(ModSyncHandoffQueue.TryDequeue(out string queued), Is.True);
                Assert.That(queued, Is.EqualTo(modSyncUrl));
            }
        }

        [Test]
        public async Task SendToPrimaryAsync_NoPrimaryListening_ReturnsFalse()
        {
            using (var orphan = new SingleInstanceService(UniquePipeName()))
            {
                bool sent = await orphan.SendToPrimaryAsync("nxm://kotor/mods/1/files/2", timeoutMs: 500);

                Assert.That(sent, Is.False);
            }
        }

        [Test]
        public async Task SendToPrimaryAsync_ActivateMessage_RaisesActivationRequested()
        {
            string pipeName = UniquePipeName();
            bool activationRaised = false;
            using (var received = new ManualResetEventSlim(false))
            using (var primary = new SingleInstanceService(pipeName))
            using (var secondary = new SingleInstanceService(pipeName))
            {
                primary.ActivationRequested += (sender, args) =>
                {
                    activationRaised = true;
                    received.Set();
                };

                Assert.That(primary.TryBecomePrimary(), Is.True);

                bool sent = await secondary.SendToPrimaryAsync(ApplicationLaunchCoordinator.ActivateMessage);

                Assert.That(sent, Is.True);
                Assert.That(received.Wait(TimeSpan.FromSeconds(10)), Is.True);
                Assert.That(activationRaised, Is.True);
            }
        }

        [Test]
        public void TryBecomePrimary_AfterPreviousPrimaryDisposed_Succeeds()
        {
            string pipeName = UniquePipeName();

            using (var first = new SingleInstanceService(pipeName))
            {
                Assert.That(first.TryBecomePrimary(), Is.True);
            }

            using (var second = new SingleInstanceService(pipeName))
            {
                Assert.That(second.TryBecomePrimary(), Is.True);
            }
        }
    }
}
