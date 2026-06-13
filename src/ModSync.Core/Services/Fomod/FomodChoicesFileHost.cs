// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    public sealed class FomodChoicesFileHost : IFomodPostDownloadHost
    {
        [NotNull]
        private readonly FomodChoicesFile _choicesFile;

        public FomodChoicesFileHost([NotNull] FomodChoicesFile choicesFile)
        {
            _choicesFile = choicesFile ?? throw new ArgumentNullException(nameof(choicesFile));
        }

        public Task<FomodConfigurePromptResult> AskConfigureAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            FomodArchiveChoices archiveChoices = FomodChoicesApplier.FindArchiveChoices(_choicesFile, context.ArchiveFileName);
            if (archiveChoices is null)
            {
                Console.Error.WriteLine(
                    $"WARN: No FOMOD choices entry for archive '{context.ArchiveFileName}' in the choices file.");
                FomodDownloadPromptState.MarkWarned(context.Component, context.ArchiveFileName);
                return Task.FromResult(FomodConfigurePromptResult.AlreadyHandled);
            }

            return Task.FromResult(FomodConfigurePromptResult.Configure);
        }

        public Task<ModComponent> RunWizardAsync(
            string extractedArchiveDirectory,
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            FomodArchiveChoices archiveChoices = FomodChoicesApplier.FindArchiveChoices(_choicesFile, context.ArchiveFileName);
            if (archiveChoices is null)
            {
                return Task.FromResult<ModComponent>(null);
            }

            try
            {
                ModComponent configured = FomodChoicesApplier.ApplyChoices(extractedArchiveDirectory, archiveChoices);
                return Task.FromResult(configured);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: Failed to apply FOMOD choices for '{context.ArchiveFileName}': {ex.Message}");
                return Task.FromResult<ModComponent>(null);
            }
        }

        public Task ReportExtractFailureAsync(
            FomodPromptContext context,
            string message,
            CancellationToken cancellationToken = default)
        {
            Console.Error.WriteLine(message);
            return Task.CompletedTask;
        }

        public Task ReportConfiguredAsync(FomodPromptContext context, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
