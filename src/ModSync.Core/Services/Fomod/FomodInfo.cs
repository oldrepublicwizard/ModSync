// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    /// <summary>
    /// Metadata parsed from a FOMOD archive's <c>fomod/info.xml</c> file.
    /// All fields are optional in the wild; missing elements map to empty strings.
    /// </summary>
    public sealed class FomodInfo
    {
        [NotNull] public string Name { get; set; } = string.Empty;
        [NotNull] public string Author { get; set; } = string.Empty;
        [NotNull] public string Version { get; set; } = string.Empty;
        [NotNull] public string Website { get; set; } = string.Empty;
        [NotNull] public string Description { get; set; } = string.Empty;
    }
}
