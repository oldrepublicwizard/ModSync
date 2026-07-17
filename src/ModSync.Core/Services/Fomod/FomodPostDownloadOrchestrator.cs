// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodPostDownloadOrchestrator
    {
        public static async Task ProcessAsync(
            [NotNull][ItemNotNull] IEnumerable<ModComponent> components,
            [NotNull] string modDirectory,
            [NotNull] IFomodPostDownloadHost host,
            CancellationToken cancellationToken = default)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (string.IsNullOrWhiteSpace(modDirectory))
            {
                throw new ArgumentException("Mod directory cannot be null or whitespace.", nameof(modDirectory));
            }

            if (host is null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            foreach (ModComponent component in components.Where(c => c.IsSelected))
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (string archivePath in FomodDownloadedArchivePaths.GetPaths(component, modDirectory))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string archiveFileName = System.IO.Path.GetFileName(archivePath);
                    if (!FomodArchiveProbe.TryDetectInArchive(archivePath, out _))
                    {
                        continue;
                    }

                    if (!FomodDownloadPromptState.ShouldPrompt(component, archiveFileName))
                    {
                        continue;
                    }

                    var context = new FomodPromptContext(component, archivePath, archiveFileName);
                    FomodConfigurePromptResult promptResult = await host.AskConfigureAsync(context, cancellationToken)
                        .ConfigureAwait(false);

                    if (promptResult == FomodConfigurePromptResult.Dismiss)
                    {
                        FomodDownloadPromptState.MarkDismissed(component, archiveFileName);
                        continue;
                    }

                    if (promptResult == FomodConfigurePromptResult.AlreadyHandled)
                    {
                        continue;
                    }

                    string extractedDirectory = await FomodArchiveExtractService
                        .ExtractAsync(archivePath, modDirectory, cancellationToken)
                        .ConfigureAwait(false);

                    if (extractedDirectory is null)
                    {
                        await host.ReportExtractFailureAsync(
                            context,
                            $"Failed to extract '{archiveFileName}' for FOMOD configuration.",
                            cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    ModComponent configured = await host.RunWizardAsync(extractedDirectory, context, cancellationToken)
                        .ConfigureAwait(false);

                    if (configured is null)
                    {
                        await host.ReportExtractFailureAsync(
                            context,
                            $"FOMOD configuration was not applied for '{archiveFileName}'.",
                            cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    FomodConfiguredComponentMerger.MergeInto(component, configured, archiveFileName);
                    FomodDownloadPromptState.MarkConfigured(component, archiveFileName);
                    await host.ReportConfiguredAsync(context, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
