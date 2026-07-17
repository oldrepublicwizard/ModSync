// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Parsing;
using ModSync.Core.Services;

namespace ModSync.Core.Ports.Guides
{
    /// <summary>
    /// Default guide ingest: deserialize via <see cref="ModComponentSerializationService"/>
    /// and optionally draft instructions via <see cref="DraftInstructionService"/>.
    /// </summary>
    public sealed class GuideIngestService : IGuideIngestService
    {
        public static GuideIngestService Instance { get; } = new GuideIngestService();

        public GuideIngestResult IngestFromText(string content, string formatHint = null, bool parseDirections = false)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string format = string.IsNullOrWhiteSpace(formatHint) ? null : formatHint.Trim().ToLowerInvariant();
            IReadOnlyList<ModComponent> components =
                ModComponentSerializationService.DeserializeModComponentFromString(content, format);

            IReadOnlyList<DraftInstructionResult> drafts = Array.Empty<DraftInstructionResult>();
            if (parseDirections && components != null && components.Count > 0)
            {
                drafts = DraftInstructionService.GenerateDraftInstructions(components);
            }

            return new GuideIngestResult(components ?? Array.Empty<ModComponent>(), drafts, format);
        }
    }

    /// <summary>Default guide emit over <see cref="ModComponentSerializationService.GenerateModDocumentation"/>.</summary>
    public sealed class GuideEmitService : IGuideEmitService
    {
        public static GuideEmitService Instance { get; } = new GuideEmitService();

        public string EmitMarkdown(
            IReadOnlyList<ModComponent> components,
            string preambleContent = null,
            string epilogueContent = null)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            return ModComponentSerializationService.GenerateModDocumentation(
                components,
                preambleContent,
                epilogueContent);
        }

        public Task<string> EmitMarkdownAsync(
            IReadOnlyList<ModComponent> components,
            string preambleContent = null,
            string epilogueContent = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(EmitMarkdown(components, preambleContent, epilogueContent));
        }
    }
}
