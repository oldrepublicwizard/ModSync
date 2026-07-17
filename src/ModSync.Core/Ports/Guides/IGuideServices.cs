// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Parsing;

namespace ModSync.Core.Ports.Guides
{
    /// <summary>Result of ingesting guide or instruction text into components.</summary>
    public sealed class GuideIngestResult
    {
        public GuideIngestResult(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> components,
            [CanBeNull][ItemNotNull] IReadOnlyList<DraftInstructionResult> draftResults = null,
            [CanBeNull] string detectedFormat = null)
        {
            Components = components ?? throw new ArgumentNullException(nameof(components));
            DraftResults = draftResults ?? Array.Empty<DraftInstructionResult>();
            DetectedFormat = detectedFormat;
        }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<ModComponent> Components { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<DraftInstructionResult> DraftResults { get; }

        [CanBeNull]
        public string DetectedFormat { get; }
    }

    /// <summary>
    /// Guide / instruction ingest port (paste, file, CLI convert).
    /// Parses text into components and optionally drafts instructions from prose.
    /// </summary>
    public interface IGuideIngestService
    {
        [NotNull]
        GuideIngestResult IngestFromText(
            [NotNull] string content,
            [CanBeNull] string formatHint = null,
            bool parseDirections = false);
    }

    /// <summary>Guide emission port: components → human-readable markdown.</summary>
    public interface IGuideEmitService
    {
        [NotNull]
        string EmitMarkdown(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> components,
            [CanBeNull] string preambleContent = null,
            [CanBeNull] string epilogueContent = null);

        [ItemNotNull]
        Task<string> EmitMarkdownAsync(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> components,
            [CanBeNull] string preambleContent = null,
            [CanBeNull] string epilogueContent = null,
            CancellationToken cancellationToken = default);
    }
}
