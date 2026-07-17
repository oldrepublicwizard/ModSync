// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.Conflicts;

namespace ModSync.Core.Ports.Conflicts
{
    /// <summary>
    /// File-conflict analysis port shared by GUI and CLI.
    /// Default implementation: <see cref="FileConflictAnalyzer"/>.
    /// </summary>
    public interface IConflictAnalyzer
    {
        [NotNull]
        Task<ConflictAnalysisResult> AnalyzeAsync(
            [NotNull][ItemNotNull] IReadOnlyList<ModComponent> componentsInInstallOrder,
            CancellationToken cancellationToken = default);
    }
}
