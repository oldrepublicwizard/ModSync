// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using ModSync.Core.Services.Fomod;

namespace ModSync.Core.CLI
{
    public sealed class FomodWarnContinuePostDownloadHost : IFomodPostDownloadHost
    {
        public Task<FomodConfigurePromptResult> AskConfigureAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            string message =
                $"WARN: FOMOD installer detected in '{context.ArchiveFileName}' for mod '{context.Component.Name}'. "
                + "Download continues, but validate/install will fail until configured. "
                + FomodConfigurationGate.RecoveryHint;
            Console.Error.WriteLine(message);
            FomodDownloadPromptState.MarkWarned(context.Component, context.ArchiveFileName);
            return Task.FromResult(FomodConfigurePromptResult.AlreadyHandled);
        }

        public Task<ModComponent> RunWizardAsync(
            string extractedArchiveDirectory,
            FomodPromptContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ModComponent>(null);

        public Task ReportExtractFailureAsync(
            FomodPromptContext context,
            string message,
            CancellationToken cancellationToken = default)
        {
            Console.Error.WriteLine($"{message} {FomodConfigurationGate.RecoveryHint}");
            return Task.CompletedTask;
        }

        public Task ReportConfiguredAsync(FomodPromptContext context, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class FomodSkipPostDownloadHost : IFomodPostDownloadHost
    {
        public Task<FomodConfigurePromptResult> AskConfigureAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            Console.Error.WriteLine(
                $"WARN: Skipping FOMOD configuration for '{context.ArchiveFileName}' "
                + $"(mod '{context.Component.Name}') because --fomod-skip / skip mode is active. "
                + "Validate/install will fail until this archive is configured. "
                + FomodConfigurationGate.RecoveryHint);
            return Task.FromResult(FomodConfigurePromptResult.Dismiss);
        }

        public Task<ModComponent> RunWizardAsync(
            string extractedArchiveDirectory,
            FomodPromptContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<ModComponent>(null);

        public Task ReportExtractFailureAsync(
            FomodPromptContext context,
            string message,
            CancellationToken cancellationToken = default)
        {
            Console.Error.WriteLine($"{message} {FomodConfigurationGate.RecoveryHint}");
            return Task.CompletedTask;
        }

        public Task ReportConfiguredAsync(FomodPromptContext context, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class FomodConsolePostDownloadHost : IFomodPostDownloadHost
    {
        public async Task<FomodConfigurePromptResult> AskConfigureAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            await Console.Out.WriteLineAsync(
                $"A FOMOD installer was detected in '{context.ArchiveFileName}' for mod '{context.Component.Name}'.")
                .ConfigureAwait(false);
            await Console.Out.WriteLineAsync("Configure installer options now? [y/N]: ").ConfigureAwait(false);

            string response = Console.ReadLine()?.Trim();
            if (string.Equals(response, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(response, "yes", StringComparison.OrdinalIgnoreCase))
            {
                return FomodConfigurePromptResult.Configure;
            }

            return FomodConfigurePromptResult.Dismiss;
        }

        public Task<ModComponent> RunWizardAsync(
            string extractedArchiveDirectory,
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            ModComponent configured = FomodConsoleWizard.Run(
                extractedArchiveDirectory,
                context.Component.Name,
                context.ArchiveFileName);
            return Task.FromResult(configured);
        }

        public Task ReportExtractFailureAsync(
            FomodPromptContext context,
            string message,
            CancellationToken cancellationToken = default)
        {
            Console.Error.WriteLine(message);
            return Task.CompletedTask;
        }

        public async Task ReportConfiguredAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            await Console.Out.WriteLineAsync(
                $"FOMOD configuration applied to '{context.Component.Name}' from '{context.ArchiveFileName}'.")
                .ConfigureAwait(false);
        }
    }
}
