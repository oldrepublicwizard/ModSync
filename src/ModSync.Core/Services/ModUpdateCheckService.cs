// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using ModSync.Core.Services.Download;

namespace ModSync.Core.Services
{
    /// <summary>
    /// Checks tracked Nexus Mods components for newer versions using the
    /// <see cref="NexusApiClient"/> and records the results on each component's
    /// <see cref="ResourceMetadata"/> entries (ModVersion, LatestKnownVersion,
    /// LastUpdateCheck, UpdateAvailable).
    /// </summary>
    public sealed class ModUpdateCheckService
    {
        private static readonly Regex s_nexusUrlRegex = new Regex(
            @"nexusmods\.com/([^/]+)/mods/(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(5));

        private readonly NexusApiClient _apiClient;

        public ModUpdateCheckService(NexusApiClient apiClient) => _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));

        /// <summary>
        /// Checks every Nexus Mods resource in the given components for updates.
        /// Mod info is fetched once per unique (gameDomain, modId) pair across all
        /// components. Stops early when the API rate-limit budget is exhausted.
        /// </summary>
        public async Task<ModUpdateCheckResult> CheckForUpdatesAsync(
            IEnumerable<ModComponent> components,
            CancellationToken cancellationToken = default)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            var result = new ModUpdateCheckResult();
            var modInfoCache = new Dictionary<string, NexusModInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (ModComponent component in components)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (component is null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, ResourceMetadata> entry in component.ResourceRegistry)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string url = entry.Key;
                    ResourceMetadata metadata = entry.Value;
                    if (metadata is null)
                    {
                        continue;
                    }

                    Match match = s_nexusUrlRegex.Match(url ?? string.Empty);
                    if (!match.Success)
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    string gameDomain = match.Groups[1].Value;
                    if (!long.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long modId))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    string cacheKey = gameDomain + "/" + modId.ToString(CultureInfo.InvariantCulture);
                    NexusModInfo modInfo;
                    if (!modInfoCache.TryGetValue(cacheKey, out modInfo))
                    {
                        if (_apiClient.IsRateLimitExhausted)
                        {
                            result.RateLimitReached = true;
                            await Logger.LogWarningAsync("[ModUpdateCheck] Nexus API rate limit exhausted; stopping update check early.").ConfigureAwait(false);
                            return result;
                        }

                        try
                        {
                            modInfo = await _apiClient.GetModInfoAsync(gameDomain, (int)modId, cancellationToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            modInfo = null;
                            result.Errors.Add(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", cacheKey, ex.Message));
                            await Logger.LogWarningAsync(string.Format(CultureInfo.InvariantCulture, "[ModUpdateCheck] Failed to fetch mod info for {0}: {1}", cacheKey, ex.Message)).ConfigureAwait(false);
                        }

                        // Cache failures too so a broken mod is only queried once per run.
                        modInfoCache[cacheKey] = modInfo;
                    }

                    if (modInfo is null)
                    {
                        continue;
                    }

                    ApplyCheckResult(component, url, metadata, modInfo, result);
                }
            }

            return result;
        }

        private static void ApplyCheckResult(
            ModComponent component,
            string url,
            ResourceMetadata metadata,
            NexusModInfo modInfo,
            ModUpdateCheckResult result)
        {
            string apiVersion = modInfo.Version?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(apiVersion))
            {
                // Indeterminate API response — preserve existing metadata and badge state.
                return;
            }

            metadata.LastUpdateCheck = DateTime.UtcNow;
            metadata.LatestKnownVersion = apiVersion;
            result.CheckedCount++;

            if (string.IsNullOrWhiteSpace(metadata.ModVersion))
            {
                // First check: adopt the provider-reported version as the baseline.
                metadata.ModVersion = apiVersion;
                metadata.UpdateAvailable = false;
                return;
            }

            string installedVersion = metadata.ModVersion.Trim();
            bool updateAvailable = !string.Equals(installedVersion, apiVersion, StringComparison.OrdinalIgnoreCase);
            metadata.UpdateAvailable = updateAvailable;

            if (updateAvailable)
            {
                result.UpdatesFound.Add(new ModUpdateInfo
                {
                    ComponentName = component.Name,
                    Url = url,
                    InstalledVersion = installedVersion,
                    LatestVersion = apiVersion,
                });
            }
        }
    }

    /// <summary>Summary of one <see cref="ModUpdateCheckService.CheckForUpdatesAsync"/> run.</summary>
    public sealed class ModUpdateCheckResult
    {
        /// <summary>Number of Nexus resources whose metadata was refreshed.</summary>
        public int CheckedCount { get; set; }

        /// <summary>Number of resource URLs skipped because they are not Nexus Mods URLs.</summary>
        public int SkippedCount { get; set; }

        /// <summary>True when the check stopped early because the API budget ran out.</summary>
        public bool RateLimitReached { get; set; }

        /// <summary>Resources with a newer provider version than the stored ModVersion.</summary>
        public List<ModUpdateInfo> UpdatesFound { get; } = new List<ModUpdateInfo>();

        /// <summary>Per-mod fetch errors, formatted as "domain/modId: message".</summary>
        public List<string> Errors { get; } = new List<string>();
    }

    /// <summary>One detected update.</summary>
    public sealed class ModUpdateInfo
    {
        public string ComponentName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string InstalledVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
    }
}
