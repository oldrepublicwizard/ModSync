// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Globalization;

namespace ModSync.Core.Services.Download
{
    /// <summary>
    /// Parsed representation of a Nexus Mods <c>nxm://</c> protocol URL, as produced by the
    /// "Mod Manager Download" button on nexusmods.com.
    ///
    /// Format: <c>nxm://{gameDomain}/mods/{modId}/files/{fileId}?key=...&amp;expires=...&amp;user_id=...</c>
    ///
    /// The <c>key</c>/<c>expires</c> pair is a one-time download authorization that the Nexus API
    /// <c>download_link.json</c> endpoint accepts even for non-Premium users.
    /// </summary>
    public sealed class NxmUrl
    {
        /// <summary>Nexus game domain, e.g. "kotor" or "kotor2".</summary>
        public string GameDomain { get; }

        /// <summary>Numeric mod identifier on Nexus Mods.</summary>
        public long ModId { get; }

        /// <summary>Numeric file identifier within the mod.</summary>
        public long FileId { get; }

        /// <summary>One-time download key (free-user authorization). May be null.</summary>
        public string Key { get; }

        /// <summary>Unix timestamp (seconds) when <see cref="Key"/> expires. Zero when absent.</summary>
        public long Expires { get; }

        /// <summary>Nexus user id embedded in the link. Zero when absent.</summary>
        public long UserId { get; }

        /// <summary>The original nxm:// URL string this instance was parsed from.</summary>
        public string OriginalUrl { get; }

        private NxmUrl(string gameDomain, long modId, long fileId, string key, long expires, long userId, string originalUrl)
        {
            GameDomain = gameDomain;
            ModId = modId;
            FileId = fileId;
            Key = key;
            Expires = expires;
            UserId = userId;
            OriginalUrl = originalUrl;
        }

        /// <summary>True when <see cref="Key"/> and <see cref="Expires"/> are both present.</summary>
        public bool HasDownloadAuthorization => !string.IsNullOrEmpty(Key) && Expires > 0;

        /// <summary>
        /// Returns true when the string looks like an nxm:// protocol URL.
        /// </summary>
        public static bool IsNxmUrl(string url)
        {
            return url != null && url.TrimStart().StartsWith("nxm://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Attempts to parse an nxm:// URL. Returns false for anything that is not a
        /// well-formed <c>nxm://{game}/mods/{modId}/files/{fileId}</c> URL.
        /// </summary>
        public static bool TryParse(string url, out NxmUrl result)
        {
            result = null;

            if (!IsNxmUrl(url))
            {
                return false;
            }

            string trimmed = url.Trim();
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            string gameDomain = uri.Host;
            if (string.IsNullOrWhiteSpace(gameDomain))
            {
                return false;
            }

            // Expected path: /mods/{modId}/files/{fileId}
            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length != 4
                || !segments[0].Equals("mods", StringComparison.OrdinalIgnoreCase)
                || !segments[2].Equals("files", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!long.TryParse(segments[1], NumberStyles.None, CultureInfo.InvariantCulture, out long modId)
                || !long.TryParse(segments[3], NumberStyles.None, CultureInfo.InvariantCulture, out long fileId))
            {
                return false;
            }

            string key = null;
            long expires = 0;
            long userId = 0;

            string query = uri.Query;
            if (!string.IsNullOrEmpty(query))
            {
                string[] pairs = query.TrimStart('?').Split('&');
                foreach (string pair in pairs)
                {
                    int eq = pair.IndexOf('=');
                    if (eq <= 0)
                    {
                        continue;
                    }

                    string name = Uri.UnescapeDataString(pair.Substring(0, eq));
                    string value = Uri.UnescapeDataString(pair.Substring(eq + 1));

                    if (name.Equals("key", StringComparison.OrdinalIgnoreCase))
                    {
                        key = value;
                    }
                    else if (name.Equals("expires", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out expires);
                    }
                    else if (name.Equals("user_id", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out userId);
                    }
                }
            }

            result = new NxmUrl(gameDomain.ToLowerInvariant(), modId, fileId, key, expires, userId, trimmed);
            return true;
        }

        /// <summary>
        /// Canonical mod page URL on nexusmods.com for this nxm link.
        /// </summary>
        public string ToModPageUrl()
        {
            return $"https://www.nexusmods.com/{GameDomain}/mods/{ModId}";
        }

        /// <summary>
        /// Canonical file-tab URL on nexusmods.com for this nxm link.
        /// </summary>
        public string ToFileUrl()
        {
            return $"https://www.nexusmods.com/{GameDomain}/mods/{ModId}?tab=files&file_id={FileId}";
        }

        /// <summary>
        /// Returns true when the given https nexusmods.com URL refers to the same game domain
        /// and mod id as this nxm link. Used to match an incoming nxm hand-off against the
        /// download URLs registered on loaded <c>ModComponent.ResourceRegistry</c> entries.
        /// </summary>
        public bool MatchesNexusUrl(string nexusUrl)
        {
            if (string.IsNullOrWhiteSpace(nexusUrl))
            {
                return false;
            }

            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(
                nexusUrl,
                @"nexusmods\.com/([^/?#]+)/mods/(\d+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }

            return match.Groups[1].Value.Equals(GameDomain, StringComparison.OrdinalIgnoreCase)
                && long.TryParse(match.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out long modId)
                && modId == ModId;
        }

        public override string ToString()
        {
            return $"nxm://{GameDomain}/mods/{ModId}/files/{FileId}";
        }
    }
}
