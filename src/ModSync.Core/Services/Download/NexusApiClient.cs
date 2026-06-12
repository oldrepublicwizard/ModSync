// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace ModSync.Core.Services.Download
{
    /// <summary>
    /// Standalone client for the Nexus Mods REST API (https://api.nexusmods.com/v1).
    /// Provides typed access to mod info, file lists, md5 lookups, and API key
    /// validation, with 429 Retry-After handling and rate-limit budget tracking.
    /// </summary>
    /// <remarks>
    /// This client is intentionally independent of <see cref="NexusModsDownloadHandler"/>;
    /// migrating the download handler onto this client is a planned follow-up.
    /// </remarks>
    public sealed class NexusApiClient
    {
        private const string ApiBaseUrl = "https://api.nexusmods.com/v1";
        private const string UserAgent = "ModSync/2.0.0a1 (https://github.com/th3w1zard1/ModSync)";
        private const string ApplicationName = "ModSync";
        private const string ApplicationVersion = "2.0.0a1";

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public NexusApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey;
        }

        /// <summary>True when an API key was supplied at construction time.</summary>
        public bool HasApiKey => !string.IsNullOrWhiteSpace(_apiKey);

        /// <summary>
        /// Last-seen value of the X-RL-Daily-Remaining response header, or null if
        /// no API response has reported it yet.
        /// </summary>
        public int? DailyRemaining { get; private set; }

        /// <summary>
        /// Last-seen value of the X-RL-Hourly-Remaining response header, or null if
        /// no API response has reported it yet.
        /// </summary>
        public int? HourlyRemaining { get; private set; }

        /// <summary>
        /// True when the last-seen rate-limit headers indicate no remaining requests
        /// in either the hourly or daily budget.
        /// </summary>
        public bool IsRateLimitExhausted =>
            (DailyRemaining.HasValue && DailyRemaining.Value <= 0) ||
            (HourlyRemaining.HasValue && HourlyRemaining.Value <= 0);

        /// <summary>
        /// Fetches mod info from GET /games/{domain}/mods/{id}.json.
        /// </summary>
        public async Task<NexusModInfo> GetModInfoAsync(string gameDomain, int modId, CancellationToken cancellationToken = default)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "{0}/games/{1}/mods/{2}.json", ApiBaseUrl, gameDomain, modId);
            string json = await GetStringAsync(url, cancellationToken).ConfigureAwait(false);
            JObject data = JObject.Parse(json);

            return new NexusModInfo
            {
                Name = (string)data["name"] ?? string.Empty,
                Version = (string)data["version"] ?? string.Empty,
                UpdatedTimestamp = data["updated_timestamp"]?.Type == JTokenType.Integer ? data["updated_timestamp"].Value<long>() : 0L,
                Available = data["available"]?.Type == JTokenType.Boolean ? data["available"].Value<bool>() : true,
            };
        }

        /// <summary>
        /// Fetches the file list from GET /games/{domain}/mods/{id}/files.json.
        /// </summary>
        public async Task<List<NexusModFile>> GetModFilesAsync(string gameDomain, int modId, CancellationToken cancellationToken = default)
        {
            string url = string.Format(CultureInfo.InvariantCulture, "{0}/games/{1}/mods/{2}/files.json", ApiBaseUrl, gameDomain, modId);
            string json = await GetStringAsync(url, cancellationToken).ConfigureAwait(false);
            JObject data = JObject.Parse(json);

            var result = new List<NexusModFile>();
            if (!(data["files"] is JArray files))
            {
                return result;
            }

            foreach (JToken file in files)
            {
                result.Add(new NexusModFile
                {
                    FileId = file["file_id"]?.Type == JTokenType.Integer ? file["file_id"].Value<long>() : 0L,
                    FileName = (string)file["file_name"] ?? string.Empty,
                    Version = (string)file["version"] ?? string.Empty,
                    CategoryName = (string)file["category_name"] ?? string.Empty,
                    Md5 = ((string)file["md5"])?.ToLowerInvariant() ?? string.Empty,
                });
            }

            return result;
        }

        /// <summary>
        /// Looks up a file by md5 via GET /games/{domain}/mods/md5_search/{md5}.json.
        /// Returns one result per matching (mod, file) pair.
        /// </summary>
        public async Task<List<NexusMd5Result>> Md5SearchAsync(string gameDomain, string md5, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                throw new ArgumentException("md5 hash must be provided", nameof(md5));
            }

            string url = string.Format(CultureInfo.InvariantCulture, "{0}/games/{1}/mods/md5_search/{2}.json", ApiBaseUrl, gameDomain, md5.ToLowerInvariant());
            string json = await GetStringAsync(url, cancellationToken).ConfigureAwait(false);

            var result = new List<NexusMd5Result>();
            if (!(JToken.Parse(json) is JArray matches))
            {
                return result;
            }

            foreach (JToken match in matches)
            {
                JToken mod = match["mod"];
                JToken fileDetails = match["file_details"];

                result.Add(new NexusMd5Result
                {
                    ModInfo = mod is null ? null : new NexusModInfo
                    {
                        Name = (string)mod["name"] ?? string.Empty,
                        Version = (string)mod["version"] ?? string.Empty,
                        UpdatedTimestamp = mod["updated_timestamp"]?.Type == JTokenType.Integer ? mod["updated_timestamp"].Value<long>() : 0L,
                        Available = mod["available"]?.Type == JTokenType.Boolean ? mod["available"].Value<bool>() : true,
                    },
                    ModId = mod?["mod_id"]?.Type == JTokenType.Integer ? mod["mod_id"].Value<long>() : 0L,
                    File = fileDetails is null ? null : new NexusModFile
                    {
                        FileId = fileDetails["file_id"]?.Type == JTokenType.Integer ? fileDetails["file_id"].Value<long>() : 0L,
                        FileName = (string)fileDetails["file_name"] ?? string.Empty,
                        Version = (string)fileDetails["version"] ?? string.Empty,
                        CategoryName = (string)fileDetails["category_name"] ?? string.Empty,
                        Md5 = ((string)fileDetails["md5"])?.ToLowerInvariant() ?? string.Empty,
                    },
                });
            }

            return result;
        }

        /// <summary>
        /// Validates the configured API key via GET /users/validate.json.
        /// </summary>
        public async Task<NexusValidateResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            if (!HasApiKey)
            {
                return new NexusValidateResult { IsValid = false, Message = "No Nexus Mods API key configured." };
            }

            string url = ApiBaseUrl + "/users/validate.json";
            using (HttpResponseMessage response = await SendAsync(url, cancellationToken).ConfigureAwait(false))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new NexusValidateResult { IsValid = false, Message = "API key is invalid or unauthorized." };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new NexusValidateResult
                    {
                        IsValid = false,
                        Message = string.Format(CultureInfo.InvariantCulture, "API validation failed with status code: {0}", response.StatusCode),
                    };
                }

                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject data = JObject.Parse(json);
                return new NexusValidateResult
                {
                    IsValid = true,
                    UserName = (string)data["name"] ?? string.Empty,
                    IsPremium = data["is_premium"]?.Type == JTokenType.Boolean && data["is_premium"].Value<bool>(),
                    Message = "API key is valid.",
                };
            }
        }

        private async Task<string> GetStringAsync(string url, CancellationToken cancellationToken)
        {
            using (HttpResponseMessage response = await SendAsync(url, cancellationToken).ConfigureAwait(false))
            {
                _ = response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a GET request with the standard Nexus API headers, retrying once on
        /// HTTP 429 after honoring the Retry-After header (matching the convention in
        /// <see cref="NexusModsDownloadHandler"/>). Updates rate-limit counters from
        /// every response.
        /// </summary>
        private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendOnceAsync(url, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == (System.Net.HttpStatusCode)429)
            {
                TimeSpan delay = TimeSpan.FromSeconds(60);
                RetryConditionHeaderValue retryAfter = response.Headers.RetryAfter;
                if (retryAfter?.Delta != null)
                {
                    delay = retryAfter.Delta.Value;
                }

                await Logger.LogWarningAsync(string.Format(CultureInfo.InvariantCulture, "[NexusApi] Rate limited by API. Waiting {0} seconds before retry...", (int)delay.TotalSeconds)).ConfigureAwait(false);
                response.Dispose();
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                response = await SendOnceAsync(url, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }

        private async Task<HttpResponseMessage> SendOnceAsync(string url, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("Application-Name", ApplicationName);
                request.Headers.Add("Application-Version", ApplicationVersion);
                if (HasApiKey)
                {
                    request.Headers.Add("apikey", _apiKey);
                }

                if (!request.Headers.Contains("User-Agent"))
                {
                    request.Headers.Add("User-Agent", UserAgent);
                }

                if (!request.Headers.Contains("Accept"))
                {
                    request.Headers.Add("Accept", "application/json");
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                UpdateRateLimits(response);
                return response;
            }
        }

        private void UpdateRateLimits(HttpResponseMessage response)
        {
            int? daily = ReadIntHeader(response, "X-RL-Daily-Remaining");
            if (daily.HasValue)
            {
                DailyRemaining = daily;
            }

            int? hourly = ReadIntHeader(response, "X-RL-Hourly-Remaining");
            if (hourly.HasValue)
            {
                HourlyRemaining = hourly;
            }
        }

        private static int? ReadIntHeader(HttpResponseMessage response, string headerName)
        {
            if (!response.Headers.TryGetValues(headerName, out IEnumerable<string> values))
            {
                return null;
            }

            string raw = values.FirstOrDefault();
            if (raw != null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            return null;
        }
    }

    /// <summary>Mod-level info from /games/{domain}/mods/{id}.json.</summary>
    public sealed class NexusModInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        /// <summary>Unix timestamp of the last mod update.</summary>
        public long UpdatedTimestamp { get; set; }
        /// <summary>False when the mod is hidden or removed.</summary>
        public bool Available { get; set; } = true;
    }

    /// <summary>One file entry from /games/{domain}/mods/{id}/files.json.</summary>
    public sealed class NexusModFile
    {
        public long FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>Lowercase hex md5 of the file, when reported by the API.</summary>
        public string Md5 { get; set; } = string.Empty;
    }

    /// <summary>One match from /games/{domain}/mods/md5_search/{md5}.json.</summary>
    public sealed class NexusMd5Result
    {
        public NexusModInfo ModInfo { get; set; }
        public long ModId { get; set; }
        public NexusModFile File { get; set; }
    }

    /// <summary>Result of /users/validate.json.</summary>
    public sealed class NexusValidateResult
    {
        public bool IsValid { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
