// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Tracks per-archive FOMOD prompt outcomes on <see cref="ResourceMetadata.HandlerMetadata"/>.
    /// </summary>
    public static class FomodDownloadPromptState
    {
        public const string HandlerMetadataKey = "fomodPromptStatus";

        public const string StatusDismissed = "dismissed";

        public const string StatusConfigured = "configured";

        public const string StatusWarned = "warned";

        public static bool ShouldPrompt([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (string.IsNullOrWhiteSpace(archiveFileName))
            {
                return false;
            }

            string status = GetStatus(component, archiveFileName);

            // Only "configured" permanently skips the post-download prompt.
            // "dismissed" must re-prompt on Fetch Downloads (documented recovery path).
            // "warned" stays quiet so CLI warn-continue does not spam every re-download.
            if (string.Equals(status, StatusConfigured, StringComparison.Ordinal)
                || string.Equals(status, StatusWarned, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        [CanBeNull]
        public static string GetStatus([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            if (!TryGetResourceForArchive(component, archiveFileName, out ResourceMetadata resource))
            {
                return null;
            }

            if (resource.HandlerMetadata is null
                || !TryGetStatusDictionaryFromMetadata(resource.HandlerMetadata, out Dictionary<string, string> statuses))
            {
                return null;
            }

            string statusKey = NormalizeFileName(System.IO.Path.GetFileName(archiveFileName));
            return statuses.TryGetValue(statusKey, out string status)
                ? status
                : null;
        }

        public static void MarkDismissed([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            SetStatus(component, archiveFileName, StatusDismissed);
        }

        public static void MarkConfigured([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            SetStatus(component, archiveFileName, StatusConfigured);
        }

        public static void MarkWarned([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            SetStatus(component, archiveFileName, StatusWarned);
        }

        private static void SetStatus(
            [NotNull] ModComponent component,
            [NotNull] string archiveFileName,
            [NotNull] string status)
        {
            if (component is null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (string.IsNullOrWhiteSpace(archiveFileName))
            {
                throw new ArgumentException("Archive file name cannot be null or whitespace.", nameof(archiveFileName));
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status cannot be null or whitespace.", nameof(status));
            }

            if (!TryGetResourceForArchive(component, archiveFileName, out ResourceMetadata resource))
            {
                return;
            }

            if (resource.HandlerMetadata is null)
            {
                resource.HandlerMetadata = new Dictionary<string, object>(StringComparer.Ordinal);
            }

            if (!TryGetStatusDictionaryFromMetadata(resource.HandlerMetadata, out Dictionary<string, string> statuses))
            {
                statuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            statuses[NormalizeFileName(System.IO.Path.GetFileName(archiveFileName))] = status;
            resource.HandlerMetadata[HandlerMetadataKey] = statuses;
        }

        private static bool TryGetResourceForArchive(
            [NotNull] ModComponent component,
            [NotNull] string archiveFileName,
            out ResourceMetadata resource)
        {
            resource = null;
            if (component.ResourceRegistry is null)
            {
                return false;
            }

            string needle = NormalizeFileName(System.IO.Path.GetFileName(archiveFileName));
            if (string.IsNullOrEmpty(needle))
            {
                return false;
            }

            foreach (ResourceMetadata candidate in component.ResourceRegistry.Values)
            {
                if (candidate?.Files is null)
                {
                    continue;
                }

                foreach (string registeredName in candidate.Files.Keys)
                {
                    if (string.IsNullOrWhiteSpace(registeredName))
                    {
                        continue;
                    }

                    if (string.Equals(NormalizeFileName(registeredName), needle, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(
                            NormalizeFileName(System.IO.Path.GetFileName(registeredName)),
                            needle,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        resource = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetStatusDictionaryFromMetadata(
            [NotNull] IDictionary<string, object> handlerMetadata,
            out Dictionary<string, string> statuses)
        {
            statuses = null;
            if (!handlerMetadata.TryGetValue(HandlerMetadataKey, out object raw)
                || raw is null)
            {
                return false;
            }

            if (raw is Dictionary<string, string> stringDict)
            {
                statuses = stringDict;
                return true;
            }

            if (raw is IDictionary<string, object> objectDict)
            {
                statuses = objectDict.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.ToString() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
                return true;
            }

            return false;
        }

        [NotNull]
        private static string NormalizeFileName([NotNull] string archiveFileName) =>
            archiveFileName.Trim();
    }
}
