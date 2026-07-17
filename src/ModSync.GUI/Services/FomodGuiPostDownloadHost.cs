// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Services.Fomod;
using ModSync.Dialogs;

namespace ModSync.Services
{
    public sealed class FomodGuiPostDownloadHost : IFomodPostDownloadHost
    {
        [NotNull]
        private readonly Window _parentWindow;

        public FomodGuiPostDownloadHost([NotNull] Window parentWindow)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
        }

        public async Task<FomodConfigurePromptResult> AskConfigureAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default)
        {
            bool? configure = await ConfirmationDialog.ShowConfirmationDialogAsync(
                _parentWindow,
                $"A FOMOD installer was detected in '{context.ArchiveFileName}' for mod '{context.Component.Name}'."
                + Environment.NewLine
                + Environment.NewLine
                + "Configure installer options now?"
                + Environment.NewLine
                + Environment.NewLine
                + "Choose No to skip for now. Run Fetch Downloads again later to configure, or use Mod Management → Configure FOMOD Mod with this mod selected.")
                .ConfigureAwait(true);

            if (configure == true)
            {
                return FomodConfigurePromptResult.Configure;
            }

            return FomodConfigurePromptResult.Dismiss;
        }

        public Task<ModComponent> RunWizardAsync(
            string extractedArchiveDirectory,
            FomodPromptContext context,
            CancellationToken cancellationToken = default) =>
            FomodInstallerDialog.ShowForExtractedArchiveAsync(_parentWindow, extractedArchiveDirectory);

        public Task ReportExtractFailureAsync(
            FomodPromptContext context,
            string message,
            CancellationToken cancellationToken = default) =>
            InformationDialog.ShowInformationDialogAsync(_parentWindow, message);

        public Task ReportConfiguredAsync(
            FomodPromptContext context,
            CancellationToken cancellationToken = default) =>
            InformationDialog.ShowInformationDialogAsync(
                _parentWindow,
                $"FOMOD configuration applied to '{context.Component.Name}' from '{context.ArchiveFileName}'.");
    }
}
