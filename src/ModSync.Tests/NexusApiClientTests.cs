// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModSync.Core.Services.Download;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public class NexusApiClientTests
    {
        private const string TestApiKey = "test-api-key-0123456789";

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            public void Enqueue(HttpResponseMessage response) => _responses.Enqueue(response);

            public void EnqueueJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK, Action<HttpResponseMessage> configure = null)
            {
                var response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                configure?.Invoke(response);
                _responses.Enqueue(response);
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                if (_responses.Count == 0)
                {
                    throw new InvalidOperationException($"No canned response queued for request: {request.RequestUri}");
                }

                return Task.FromResult(_responses.Dequeue());
            }
        }

        private static NexusApiClient CreateClient(FakeHttpMessageHandler handler, string apiKey = TestApiKey) =>
            new NexusApiClient(new HttpClient(handler), apiKey);

        [Test]
        public async Task GetModInfoAsync_ValidResponse_ParsesFields()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{
                ""name"": ""Ultimate Korriban"",
                ""version"": ""1.2.0"",
                ""updated_timestamp"": 1714000000,
                ""available"": true
            }");
            NexusApiClient client = CreateClient(handler);

            NexusModInfo info = await client.GetModInfoAsync("kotor", 1367);

            Assert.Multiple(() =>
            {
                Assert.That(info.Name, Is.EqualTo("Ultimate Korriban"));
                Assert.That(info.Version, Is.EqualTo("1.2.0"));
                Assert.That(info.UpdatedTimestamp, Is.EqualTo(1714000000L));
                Assert.That(info.Available, Is.True);
            });
        }

        [Test]
        public async Task GetModInfoAsync_SendsApiHeaders()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""name"": ""x"", ""version"": ""1.0""}");
            NexusApiClient client = CreateClient(handler);

            _ = await client.GetModInfoAsync("kotor", 1);

            HttpRequestMessage request = handler.Requests.Single();
            Assert.Multiple(() =>
            {
                Assert.That(request.RequestUri?.ToString(), Is.EqualTo("https://api.nexusmods.com/v1/games/kotor/mods/1.json"));
                Assert.That(request.Headers.GetValues("apikey").Single(), Is.EqualTo(TestApiKey));
                Assert.That(request.Headers.GetValues("Application-Name").Single(), Is.EqualTo("ModSync"));
                Assert.That(request.Headers.GetValues("Application-Version").Single(), Is.EqualTo("2.0.0a1"));
                Assert.That(request.Headers.UserAgent.ToString(), Does.Contain("ModSync/2.0.0a1"));
            });
        }

        [Test]
        public async Task GetModFilesAsync_ValidResponse_ParsesFiles()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{
                ""files"": [
                    {
                        ""file_id"": 5001,
                        ""file_name"": ""Mod-Main-1.0.zip"",
                        ""version"": ""1.0"",
                        ""category_name"": ""MAIN"",
                        ""md5"": ""ABCDEF0123456789ABCDEF0123456789""
                    },
                    {
                        ""file_id"": 5002,
                        ""file_name"": ""Mod-Patch-1.1.zip"",
                        ""version"": ""1.1"",
                        ""category_name"": ""UPDATE"",
                        ""md5"": null
                    }
                ]
            }");
            NexusApiClient client = CreateClient(handler);

            List<NexusModFile> files = await client.GetModFilesAsync("kotor2", 42);

            Assert.That(files, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(files[0].FileId, Is.EqualTo(5001L));
                Assert.That(files[0].FileName, Is.EqualTo("Mod-Main-1.0.zip"));
                Assert.That(files[0].Version, Is.EqualTo("1.0"));
                Assert.That(files[0].CategoryName, Is.EqualTo("MAIN"));
                Assert.That(files[0].Md5, Is.EqualTo("abcdef0123456789abcdef0123456789"));
                Assert.That(files[1].FileId, Is.EqualTo(5002L));
                Assert.That(files[1].CategoryName, Is.EqualTo("UPDATE"));
                Assert.That(files[1].Md5, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public async Task GetModFilesAsync_EmptyFilesArray_ReturnsEmptyList()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""files"": []}");
            NexusApiClient client = CreateClient(handler);

            List<NexusModFile> files = await client.GetModFilesAsync("kotor", 7);

            Assert.That(files, Is.Empty);
        }

        [Test]
        public async Task Md5SearchAsync_ValidResponse_ParsesResults()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"[
                {
                    ""mod"": {
                        ""mod_id"": 1367,
                        ""name"": ""Ultimate Korriban"",
                        ""version"": ""1.2.0"",
                        ""updated_timestamp"": 1714000000,
                        ""available"": true
                    },
                    ""file_details"": {
                        ""file_id"": 5001,
                        ""file_name"": ""Mod-Main-1.0.zip"",
                        ""version"": ""1.0"",
                        ""category_name"": ""MAIN"",
                        ""md5"": ""abcdef0123456789abcdef0123456789""
                    }
                }
            ]");
            NexusApiClient client = CreateClient(handler);

            List<NexusMd5Result> results = await client.Md5SearchAsync("kotor", "ABCDEF0123456789ABCDEF0123456789");

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(results[0].ModId, Is.EqualTo(1367L));
                Assert.That(results[0].ModInfo?.Name, Is.EqualTo("Ultimate Korriban"));
                Assert.That(results[0].ModInfo?.Version, Is.EqualTo("1.2.0"));
                Assert.That(results[0].File?.FileId, Is.EqualTo(5001L));
                Assert.That(results[0].File?.Md5, Is.EqualTo("abcdef0123456789abcdef0123456789"));
                Assert.That(handler.Requests.Single().RequestUri?.ToString(),
                    Is.EqualTo("https://api.nexusmods.com/v1/games/kotor/mods/md5_search/abcdef0123456789abcdef0123456789.json"));
            });
        }

        [Test]
        public void Md5SearchAsync_EmptyMd5_Throws()
        {
            NexusApiClient client = CreateClient(new FakeHttpMessageHandler());

            Assert.ThrowsAsync<ArgumentException>(async () => await client.Md5SearchAsync("kotor", ""));
        }

        [Test]
        public async Task GetModInfoAsync_RateLimited429_RetriesAfterDelayAndSucceeds()
        {
            var handler = new FakeHttpMessageHandler();
            var rateLimitedResponse = new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent("rate limited"),
            };
            rateLimitedResponse.Headers.Add("Retry-After", "0");
            handler.Enqueue(rateLimitedResponse);
            handler.EnqueueJson(@"{""name"": ""Recovered Mod"", ""version"": ""3.0""}");
            NexusApiClient client = CreateClient(handler);

            NexusModInfo info = await client.GetModInfoAsync("kotor", 99);

            Assert.Multiple(() =>
            {
                Assert.That(info.Name, Is.EqualTo("Recovered Mod"));
                Assert.That(info.Version, Is.EqualTo("3.0"));
                Assert.That(handler.Requests, Has.Count.EqualTo(2), "Expected the 429 response to trigger exactly one retry");
            });
        }

        [Test]
        public async Task GetModInfoAsync_RateLimitHeaders_TrackedOnClient()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""name"": ""x"", ""version"": ""1.0""}", HttpStatusCode.OK, response =>
            {
                response.Headers.Add("X-RL-Daily-Remaining", "2400");
                response.Headers.Add("X-RL-Hourly-Remaining", "95");
            });
            handler.EnqueueJson(@"{""name"": ""y"", ""version"": ""2.0""}", HttpStatusCode.OK, response =>
            {
                response.Headers.Add("X-RL-Daily-Remaining", "2399");
                response.Headers.Add("X-RL-Hourly-Remaining", "94");
            });
            NexusApiClient client = CreateClient(handler);

            Assert.Multiple(() =>
            {
                Assert.That(client.DailyRemaining, Is.Null);
                Assert.That(client.HourlyRemaining, Is.Null);
                Assert.That(client.IsRateLimitExhausted, Is.False);
            });

            _ = await client.GetModInfoAsync("kotor", 1);
            Assert.Multiple(() =>
            {
                Assert.That(client.DailyRemaining, Is.EqualTo(2400));
                Assert.That(client.HourlyRemaining, Is.EqualTo(95));
            });

            _ = await client.GetModInfoAsync("kotor", 2);
            Assert.Multiple(() =>
            {
                Assert.That(client.DailyRemaining, Is.EqualTo(2399));
                Assert.That(client.HourlyRemaining, Is.EqualTo(94));
                Assert.That(client.IsRateLimitExhausted, Is.False);
            });
        }

        [Test]
        public async Task IsRateLimitExhausted_ZeroHourlyRemaining_ReturnsTrue()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""name"": ""x"", ""version"": ""1.0""}", HttpStatusCode.OK, response =>
            {
                response.Headers.Add("X-RL-Daily-Remaining", "100");
                response.Headers.Add("X-RL-Hourly-Remaining", "0");
            });
            NexusApiClient client = CreateClient(handler);

            _ = await client.GetModInfoAsync("kotor", 1);

            Assert.That(client.IsRateLimitExhausted, Is.True);
        }

        [Test]
        public async Task ValidateAsync_NoApiKey_ReturnsInvalidWithoutRequest()
        {
            var handler = new FakeHttpMessageHandler();
            NexusApiClient client = CreateClient(handler, apiKey: null);

            NexusValidateResult result = await client.ValidateAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.False);
                Assert.That(handler.Requests, Is.Empty);
            });
        }

        [Test]
        public async Task ValidateAsync_ValidKey_ParsesUserInfo()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""user_id"": 12345, ""name"": ""testuser"", ""is_premium"": true}");
            NexusApiClient client = CreateClient(handler);

            NexusValidateResult result = await client.ValidateAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.UserName, Is.EqualTo("testuser"));
                Assert.That(result.IsPremium, Is.True);
            });
        }

        [Test]
        public async Task ValidateAsync_Unauthorized_ReturnsInvalid()
        {
            var handler = new FakeHttpMessageHandler();
            handler.EnqueueJson(@"{""message"": ""invalid key""}", HttpStatusCode.Unauthorized);
            NexusApiClient client = CreateClient(handler);

            NexusValidateResult result = await client.ValidateAsync();

            Assert.That(result.IsValid, Is.False);
        }
    }
}
