// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.Fomod;

namespace ModSync.Services
{
    public static class FomodPostDownloadPromptService
    {
        public static Task PromptForDetectedArchivesAsync(
            [NotNull] Window parentWindow,
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> components,
            [NotNull] string modDirectory) =>
            FomodPostDownloadOrchestrator.ProcessAsync(
                components,
                modDirectory,
                new FomodGuiPostDownloadHost(parentWindow));
    }
}
