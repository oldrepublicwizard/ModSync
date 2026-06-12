// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModSync.Core;
using ModSync.Core.Services;
using Xunit;

using GuiComponentEditorService = ModSync.Services.ComponentEditorService;

namespace ModSync.Tests
{
    public sealed class ComponentEditorServiceTests
    {
        [Fact(DisplayName = "HasUnsavedChanges returns false for null component")]
        public void HasUnsavedChanges_NullComponent_ReturnsFalse()
        {
            Assert.False(GuiComponentEditorService.HasUnsavedChanges(null, "content", "toml"));
        }

        [Fact(DisplayName = "HasUnsavedChanges returns false for blank raw text")]
        public void HasUnsavedChanges_BlankRawText_ReturnsFalse()
        {
            ModComponent component = CreateComponent();

            Assert.False(GuiComponentEditorService.HasUnsavedChanges(component, string.Empty, "toml"));
            Assert.False(GuiComponentEditorService.HasUnsavedChanges(component, "   ", "toml"));
        }

        [Fact(DisplayName = "HasUnsavedChanges returns false when raw text matches serialized TOML")]
        public void HasUnsavedChanges_MatchingToml_ReturnsFalse()
        {
            ModComponent component = CreateComponent();
            string serialized = Serialize(component, "toml");

            Assert.False(GuiComponentEditorService.HasUnsavedChanges(component, serialized, "toml"));
        }

        [Fact(DisplayName = "HasUnsavedChanges returns true when raw text differs from serialized TOML")]
        public void HasUnsavedChanges_DifferentToml_ReturnsTrue()
        {
            ModComponent component = CreateComponent();
            string serialized = Serialize(component, "toml");

            Assert.True(GuiComponentEditorService.HasUnsavedChanges(component, serialized + "# edited", "toml"));
        }

        [Fact(DisplayName = "HasUnsavedChanges compares using requested serialization format")]
        public void HasUnsavedChanges_JsonFormat_UsesJsonSerialization()
        {
            ModComponent component = CreateComponent();
            string serialized = Serialize(component, "json");

            Assert.False(GuiComponentEditorService.HasUnsavedChanges(component, serialized, "json"));
            Assert.True(GuiComponentEditorService.HasUnsavedChanges(component, serialized + " ", "json"));
        }

        private static ModComponent CreateComponent()
        {
            return new ModComponent
            {
                Guid = Guid.NewGuid(),
                Name = "Editor Test Mod",
            };
        }

        private static string Serialize(ModComponent component, string format)
        {
            return ModComponentSerializationService.SerializeModComponentAsString(
                new List<ModComponent> { component },
                format);
        }
    }
}
