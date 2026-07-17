// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ModSync.Core.Services.Protocol
{
    /// <summary>
    /// Parsed representation of a ModSync deep-link URL for sharing whole builds
    /// ("Install with ModSync").
    /// Accepted forms include:
    /// <c>modsync://install?url=&lt;https&gt;</c>,
    /// <c>modsync://open?instruction=&lt;https&gt;&amp;game=kotor</c>,
    /// <c>modsync://kotor/install?url=&lt;https&gt;</c>.
    /// Local file paths are rejected in this slice.
    /// </summary>
    public sealed class ModSyncUrl
    {
        private static readonly HashSet<string> s_actions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "install",
            "open",
        };

        private static readonly HashSet<string> s_games = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "kotor",
            "kotor2",
        };

        public string Action { get; }
        public string Game { get; }
        public string InstructionUrl { get; }
        public string OriginalUrl { get; }

        private ModSyncUrl(string action, string game, string instructionUrl, string originalUrl)
        {
            Action = action;
            Game = game;
            InstructionUrl = instructionUrl;
            OriginalUrl = originalUrl;
        }

        public static bool IsModSyncUrl(string url)
        {
            return url != null && url.TrimStart().StartsWith("modsync://", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TryParse(string url, out ModSyncUrl result)
        {
            result = null;

            if (!IsModSyncUrl(url))
            {
                return false;
            }

            string trimmed = url.Trim();
            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri)
                || !uri.Scheme.Equals("modsync", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string host = uri.Host;
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            string action;
            string game = null;
            string[] segments = uri.AbsolutePath.Trim('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (s_actions.Contains(host))
            {
                action = host.ToLowerInvariant();
                if (segments.Length > 1)
                {
                    return false;
                }

                if (segments.Length == 1)
                {
                    if (!s_games.Contains(segments[0]))
                    {
                        return false;
                    }

                    game = segments[0].ToLowerInvariant();
                }
            }
            else if (s_games.Contains(host))
            {
                game = host.ToLowerInvariant();
                if (segments.Length != 1 || !s_actions.Contains(segments[0]))
                {
                    return false;
                }

                action = segments[0].ToLowerInvariant();
            }
            else
            {
                return false;
            }

            if (!TryReadInstructionUrl(uri.Query, out string instructionUrl))
            {
                return false;
            }

            if (!Uri.TryCreate(instructionUrl, UriKind.Absolute, out Uri instructionUri)
                || (instructionUri.Scheme != Uri.UriSchemeHttp && instructionUri.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }

            if (TryReadQueryValue(uri.Query, "game", out string gameFromQuery))
            {
                if (!s_games.Contains(gameFromQuery))
                {
                    return false;
                }

                string normalizedGameQuery = gameFromQuery.ToLowerInvariant();
                if (game != null && !game.Equals(normalizedGameQuery, StringComparison.Ordinal))
                {
                    return false;
                }

                game = normalizedGameQuery;
            }

            result = new ModSyncUrl(action, game, instructionUri.AbsoluteUri, trimmed);
            return true;
        }

        public override string ToString()
        {
            string encoded = Uri.EscapeDataString(InstructionUrl);
            if (string.IsNullOrEmpty(Game))
            {
                return $"modsync://{Action}?url={encoded}";
            }

            return $"modsync://{Action}?url={encoded}&game={Game}";
        }

        private static bool TryReadInstructionUrl(string query, out string instructionUrl)
        {
            if (TryReadQueryValue(query, "url", out instructionUrl)
                || TryReadQueryValue(query, "instruction", out instructionUrl))
            {
                return !string.IsNullOrWhiteSpace(instructionUrl);
            }

            instructionUrl = null;
            return false;
        }

        private static bool TryReadQueryValue(string query, string name, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            string[] pairs = query.TrimStart('?').Split('&');
            foreach (string pair in pairs)
            {
                int eq = pair.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                string key = Uri.UnescapeDataString(pair.Substring(0, eq));
                if (!key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = Uri.UnescapeDataString(pair.Substring(eq + 1));
                return true;
            }

            return false;
        }
    }
}
