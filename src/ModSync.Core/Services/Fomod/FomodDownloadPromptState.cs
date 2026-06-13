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
            return string.IsNullOrEmpty(status);
        }

        [CanBeNull]
        public static string GetStatus([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            if (TryGetStatusDictionary(component, out Dictionary<string, string> statuses)
                && statuses.TryGetValue(NormalizeFileName(archiveFileName), out string status))
            {
                return status;
            }

            return null;
        }

        public static void MarkDismissed([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            SetStatus(component, archiveFileName, StatusDismissed);
        }

        public static void MarkConfigured([NotNull] ModComponent component, [NotNull] string archiveFileName)
        {
            SetStatus(component, archiveFileName, StatusConfigured);
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

            foreach (ResourceMetadata resource in component.ResourceRegistry.Values)
            {
                if (resource?.Files is null
                    || !resource.Files.ContainsKey(archiveFileName))
                {
                    continue;
                }

                if (resource.HandlerMetadata is null)
                {
                    resource.HandlerMetadata = new Dictionary<string, object>(StringComparer.Ordinal);
                }

                if (!TryGetStatusDictionaryFromMetadata(resource.HandlerMetadata, out Dictionary<string, string> statuses))
                {
                    statuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    resource.HandlerMetadata[HandlerMetadataKey] = statuses;
                }

                statuses[NormalizeFileName(archiveFileName)] = status;
                return;
            }
        }

        private static bool TryGetStatusDictionary(
            [NotNull] ModComponent component,
            out Dictionary<string, string> statuses)
        {
            statuses = null;
            if (component.ResourceRegistry is null)
            {
                return false;
            }

            foreach (ResourceMetadata resource in component.ResourceRegistry.Values)
            {
                if (resource?.HandlerMetadata is null)
                {
                    continue;
                }

                if (TryGetStatusDictionaryFromMetadata(resource.HandlerMetadata, out statuses))
                {
                    return true;
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
