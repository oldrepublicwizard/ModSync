// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    public interface IFomodPostDownloadHost
    {
        Task<FomodConfigurePromptResult> AskConfigureAsync(
            [NotNull] FomodPromptContext context,
            CancellationToken cancellationToken = default);

        [CanBeNull]
        Task<ModComponent> RunWizardAsync(
            [NotNull] string extractedArchiveDirectory,
            [NotNull] FomodPromptContext context,
            CancellationToken cancellationToken = default);

        Task ReportExtractFailureAsync(
            [NotNull] FomodPromptContext context,
            [NotNull] string message,
            CancellationToken cancellationToken = default);

        Task ReportConfiguredAsync(
            [NotNull] FomodPromptContext context,
            CancellationToken cancellationToken = default);
    }
}
