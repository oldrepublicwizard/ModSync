// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Core.Services.Download;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class ModUpdateCheckServiceTests
    {
        /// <summary>
        /// Routes mod-info requests to canned responses keyed by "gameDomain/modId"
        /// and counts requests so deduplication can be asserted.
        /// </summary>
        private sealed class RoutingHttpMessageHandler : HttpMessageHandler
        {
            private static readonly Regex s_modInfoRegex = new Regex(@"/games/([^/]+)/mods/(\d+)\.json", RegexOptions.None, TimeSpan.FromSeconds(5));

            private readonly Dictionary<string, Func<HttpResponseMessage>> _routes = new Dictionary<string, Func<HttpResponseMessage>>(StringComparer.OrdinalIgnoreCase);

            public List<string> RequestedModKeys { get; } = new List<string>();

            public void AddModInfo(string gameDomain, long modId, string version, string name = "Some Mod", Action<HttpResponseMessage> configure = null)
            {
                _routes[$"{gameDomain}/{modId}"] = () =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            $@"{{""name"": ""{name}"", ""version"": ""{version}"", ""updated_timestamp"": 1714000000, ""available"": true}}",
                            Encoding.UTF8,
                            "application/json"),
                    };
                    configure?.Invoke(response);
                    return response;
                };
            }

            public void AddError(string gameDomain, long modId, HttpStatusCode statusCode)
            {
                _routes[$"{gameDomain}/{modId}"] = () => new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent("error", Encoding.UTF8, "application/json"),
                };
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Match match = s_modInfoRegex.Match(request.RequestUri?.ToString() ?? string.Empty);
                if (!match.Success)
                {
                    throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
                }

                string key = $"{match.Groups[1].Value}/{match.Groups[2].Value}";
                RequestedModKeys.Add(key);

                if (!_routes.TryGetValue(key, out Func<HttpResponseMessage> factory))
                {
                    throw new InvalidOperationException($"No canned response for mod: {key}");
                }

                return Task.FromResult(factory());
            }
        }

        private static ModUpdateCheckService CreateService(RoutingHttpMessageHandler handler) =>
            new ModUpdateCheckService(new NexusApiClient(new HttpClient(handler), "test-api-key-0123456789"));

        private static ModComponent CreateComponent(string name, params (string Url, ResourceMetadata Metadata)[] resources)
        {
            var registry = new Dictionary<string, ResourceMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach ((string url, ResourceMetadata metadata) in resources)
            {
                registry[url] = metadata;
            }

            return new ModComponent
            {
                Name = name,
                ResourceRegistry = registry,
            };
        }

        [Test]
        public async Task CheckForUpdatesAsync_NexusUrlWithoutStoredVersion_AdoptsProviderVersion()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddModInfo("kotor", 1367, "1.2.0");
            var metadata = new ResourceMetadata();
            ModComponent component = CreateComponent("Ultimate Korriban", ("https://www.nexusmods.com/kotor/mods/1367", metadata));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(result.CheckedCount, Is.EqualTo(1));
                Assert.That(result.UpdatesFound, Is.Empty);
                Assert.That(metadata.ModVersion, Is.EqualTo("1.2.0"));
                Assert.That(metadata.LatestKnownVersion, Is.EqualTo("1.2.0"));
                Assert.That(metadata.UpdateAvailable, Is.False);
                Assert.That(metadata.LastUpdateCheck, Is.Not.Null);
                Assert.That(metadata.LastUpdateCheck.Value, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_NewerProviderVersion_DetectsUpdate()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddModInfo("kotor2", 1100, "2.5");
            var metadata = new ResourceMetadata { ModVersion = "2.0" };
            ModComponent component = CreateComponent("TSLRCM", ("https://www.nexusmods.com/kotor2/mods/1100", metadata));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(result.CheckedCount, Is.EqualTo(1));
                Assert.That(result.UpdatesFound, Has.Count.EqualTo(1));
                Assert.That(result.UpdatesFound[0].ComponentName, Is.EqualTo("TSLRCM"));
                Assert.That(result.UpdatesFound[0].InstalledVersion, Is.EqualTo("2.0"));
                Assert.That(result.UpdatesFound[0].LatestVersion, Is.EqualTo("2.5"));
                Assert.That(metadata.ModVersion, Is.EqualTo("2.0"), "Stored version must not be overwritten by an update check");
                Assert.That(metadata.LatestKnownVersion, Is.EqualTo("2.5"));
                Assert.That(metadata.UpdateAvailable, Is.True);
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_SameVersion_NoUpdateDetected()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddModInfo("kotor", 5, "1.0");
            var metadata = new ResourceMetadata { ModVersion = "1.0", UpdateAvailable = true };
            ModComponent component = CreateComponent("Some Mod", ("https://www.nexusmods.com/kotor/mods/5", metadata));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(result.UpdatesFound, Is.Empty);
                Assert.That(metadata.UpdateAvailable, Is.False, "A check confirming the same version should clear a stale update flag");
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_NonNexusUrls_SkippedWithoutApiCalls()
        {
            var handler = new RoutingHttpMessageHandler();
            var metadata = new ResourceMetadata();
            ModComponent component = CreateComponent(
                "Mega Mod",
                ("https://mega.nz/file/1A4RCLha#Ro2GNVUPRfgot", metadata),
                ("https://deadlystream.com/files/file/2243-some-mod/", new ResourceMetadata()));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(result.CheckedCount, Is.EqualTo(0));
                Assert.That(result.SkippedCount, Is.EqualTo(2));
                Assert.That(handler.RequestedModKeys, Is.Empty);
                Assert.That(metadata.LastUpdateCheck, Is.Null);
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_TwoComponentsShareMod_ApiQueriedOnce()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddModInfo("kotor", 1367, "1.2.0");
            var metadataA = new ResourceMetadata();
            var metadataB = new ResourceMetadata();
            ModComponent componentA = CreateComponent("Component A", ("https://www.nexusmods.com/kotor/mods/1367", metadataA));
            ModComponent componentB = CreateComponent("Component B", ("https://www.nexusmods.com/kotor/mods/1367?tab=files", metadataB));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { componentA, componentB });

            Assert.Multiple(() =>
            {
                Assert.That(handler.RequestedModKeys, Has.Count.EqualTo(1), "Shared mod must be queried only once across components");
                Assert.That(result.CheckedCount, Is.EqualTo(2));
                Assert.That(metadataA.LatestKnownVersion, Is.EqualTo("1.2.0"));
                Assert.That(metadataB.LatestKnownVersion, Is.EqualTo("1.2.0"));
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_RateLimitExhausted_StopsEarly()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddModInfo("kotor", 1, "1.0", configure: response =>
            {
                response.Headers.Add("X-RL-Daily-Remaining", "0");
                response.Headers.Add("X-RL-Hourly-Remaining", "0");
            });
            handler.AddModInfo("kotor", 2, "1.0");
            var metadataChecked = new ResourceMetadata();
            var metadataUnchecked = new ResourceMetadata();
            ModComponent componentA = CreateComponent("First", ("https://www.nexusmods.com/kotor/mods/1", metadataChecked));
            ModComponent componentB = CreateComponent("Second", ("https://www.nexusmods.com/kotor/mods/2", metadataUnchecked));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { componentA, componentB });

            Assert.Multiple(() =>
            {
                Assert.That(result.RateLimitReached, Is.True);
                Assert.That(result.CheckedCount, Is.EqualTo(1));
                Assert.That(handler.RequestedModKeys, Is.EqualTo(new[] { "kotor/1" }), "Second mod must not be queried after the budget is exhausted");
                Assert.That(metadataUnchecked.LastUpdateCheck, Is.Null);
            });
        }

        [Test]
        public async Task CheckForUpdatesAsync_ApiError_RecordedAndOtherModsStillChecked()
        {
            var handler = new RoutingHttpMessageHandler();
            handler.AddError("kotor", 1, HttpStatusCode.NotFound);
            handler.AddModInfo("kotor", 2, "4.0");
            var metadataFailed = new ResourceMetadata();
            var metadataOk = new ResourceMetadata();
            ModComponent component = CreateComponent(
                "Mixed",
                ("https://www.nexusmods.com/kotor/mods/1", metadataFailed),
                ("https://www.nexusmods.com/kotor/mods/2", metadataOk));
            ModUpdateCheckService service = CreateService(handler);

            ModUpdateCheckResult result = await service.CheckForUpdatesAsync(new[] { component });

            Assert.Multiple(() =>
            {
                Assert.That(result.Errors, Has.Count.EqualTo(1));
                Assert.That(result.Errors[0], Does.StartWith("kotor/1:"));
                Assert.That(result.CheckedCount, Is.EqualTo(1));
                Assert.That(metadataFailed.LastUpdateCheck, Is.Null);
                Assert.That(metadataOk.LatestKnownVersion, Is.EqualTo("4.0"));
            });
        }

        [Test]
        public void NexusUpdateAvailable_ReflectsResourceMetadataFlags()
        {
            var metadataWithUpdate = new ResourceMetadata { UpdateAvailable = true };
            var metadataWithoutUpdate = new ResourceMetadata { UpdateAvailable = false };
            ModComponent withUpdate = CreateComponent(
                "Has Update",
                ("https://www.nexusmods.com/kotor/mods/1", metadataWithUpdate));
            ModComponent withoutUpdate = CreateComponent(
                "No Update",
                ("https://www.nexusmods.com/kotor/mods/2", metadataWithoutUpdate));

            Assert.Multiple(() =>
            {
                Assert.That(withUpdate.NexusUpdateAvailable, Is.True);
                Assert.That(withoutUpdate.NexusUpdateAvailable, Is.False);
            });
        }

        [Test]
        public void CheckForUpdatesAsync_NullComponents_Throws()
        {
            ModUpdateCheckService service = CreateService(new RoutingHttpMessageHandler());

            Assert.ThrowsAsync<ArgumentNullException>(async () => await service.CheckForUpdatesAsync(null));
        }
    }
}
