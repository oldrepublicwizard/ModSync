// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using ModSync.Core;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class MarkdownRenderingServiceTests
    {
        [Fact(DisplayName = "RenderMarkdownToString returns empty for whitespace input")]
        public void RenderMarkdownToString_Whitespace_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, MarkdownRenderingService.RenderMarkdownToString("   "));
        }

        [Fact(DisplayName = "RenderMarkdownToInlines returns empty list for whitespace input")]
        public void RenderMarkdownToInlines_Whitespace_ReturnsEmptyList()
        {
            List<Inline> inlines = MarkdownRenderingService.RenderMarkdownToInlines("  ");

            Assert.Empty(inlines);
        }

        [AvaloniaFact(DisplayName = "RenderMarkdownToInlines returns inlines for formatted markdown")]
        public void RenderMarkdownToInlines_FormattedMarkdown_ReturnsInlines()
        {
            List<Inline> inlines = MarkdownRenderingService.RenderMarkdownToInlines("Hello **world**");

            Assert.NotEmpty(inlines);
        }

        [AvaloniaFact(DisplayName = "RenderMarkdownToTextBlock populates target inlines")]
        public void RenderMarkdownToTextBlock_Markdown_PopulatesInlines()
        {
            var target = new TextBlock();
            var service = new MarkdownRenderingService();

            service.RenderMarkdownToTextBlock(target, "Sample **markdown**");

            Assert.NotNull(target.Inlines);
            Assert.NotEmpty(target.Inlines);
        }

        [AvaloniaFact(DisplayName = "RenderMarkdownToTextBlock clears inlines for blank content")]
        public void RenderMarkdownToTextBlock_Blank_ClearsInlines()
        {
            var target = new TextBlock();
            target.Inlines.Add(new Run { Text = "stale" });
            var service = new MarkdownRenderingService();

            service.RenderMarkdownToTextBlock(target, "   ");

            Assert.Empty(target.Inlines);
        }

        [AvaloniaFact(DisplayName = "RenderComponentMarkdown renders description markdown")]
        public void RenderComponentMarkdown_Description_PopulatesTextBlock()
        {
            var component = new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Markdown Mod",
                Description = "**Bold** description",
            };
            var description = new TextBlock();
            var directions = new TextBlock();
            var service = new MarkdownRenderingService();

            service.RenderComponentMarkdown(component, description, directions, spoilerFreeMode: false);

            Assert.NotEmpty(description.Inlines);
        }
    }
}
