// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ModSync.Core;
using ModSync.Core.Services;
using ModSync.Core.Services.Download;
using ModSync.Dialogs;

namespace ModSync.Services
{
    /// <summary>
    /// Drains <see cref="NxmHandoffQueue"/>, matches nxm URLs to loaded components, and
    /// downloads the linked Nexus file into the mod workspace.
    /// </summary>
    public sealed class NxmHandoffService : IDisposable
    {
        private readonly Window _parentWindow;
        private readonly MainConfig _mainConfig;
        private bool _subscribed;
        private bool _disposed;
        private bool _processing;

        public NxmHandoffService(Window parentWindow, MainConfig mainConfig)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            _mainConfig = mainConfig ?? throw new ArgumentNullException(nameof(mainConfig));
        }

        public void EnsureSubscribed()
        {
            if (_subscribed || _disposed)
            {
                return;
            }

            _subscribed = true;
            NxmHandoffQueue.UrlEnqueued += OnUrlEnqueued;
        }

        /// <summary>
        /// Processes all URLs currently in the hand-off queue. Safe to call after instruction load.
        /// </summary>
        public async Task ProcessPendingAsync()
        {
            EnsureSubscribed();

            if (_processing)
            {
                return;
            }

            _processing = true;
            try
            {
                foreach (string nxmUrl in NxmHandoffQueue.DrainAll())
                {
                    await ProcessUrlAsync(nxmUrl).ConfigureAwait(true);
                }
            }
            finally
            {
                _processing = false;
            }
        }

        private void OnUrlEnqueued(object sender, string nxmUrl)
        {
            _ = Dispatcher.UIThread.InvokeAsync(async () => await ProcessPendingAsync().ConfigureAwait(true));
        }

        private async Task ProcessUrlAsync(string rawUrl)
        {
            if (!NxmUrl.TryParse(rawUrl, out NxmUrl nxmUrl))
            {
                await ShowInfoAsync(
                    "Invalid Nexus Mod Manager link",
                    "ModSync could not parse the nxm URL handed off by the browser.")
                    .ConfigureAwait(true);
                return;
            }

            if (_mainConfig.sourcePath is null || !_mainConfig.sourcePath.Exists)
            {
                await ShowInfoAsync(
                    "Mod workspace required",
                    "Set your mod download directory before using Nexus Mod Manager Download.\n\n" +
                    $"Mod page: {nxmUrl.ToModPageUrl()}")
                    .ConfigureAwait(true);
                return;
            }

            if (_mainConfig.allComponents is null || _mainConfig.allComponents.Count == 0)
            {
                await Logger.LogVerboseAsync(
                    "[NxmHandoff] Components not loaded yet; re-queuing nxm URL for later.")
                    .ConfigureAwait(true);
                NxmHandoffQueue.Enqueue(rawUrl);
                await ShowInfoAsync(
                    "Waiting for instruction file",
                    "ModSync received a Nexus Mod Manager link but no instruction file is loaded yet.\n\n" +
                    "Load your instruction file and the download will start automatically.\n\n" +
                    "If the link expires before then, click Mod Manager Download again on Nexus.\n\n" +
                    $"Mod page: {nxmUrl.ToModPageUrl()}")
                    .ConfigureAwait(true);
                return;
            }

            NxmComponentResolveStatus resolveStatus = NxmComponentResolver.TryResolve(
                nxmUrl,
                _mainConfig.allComponents,
                out NxmComponentMatch match);

            if (resolveStatus == NxmComponentResolveStatus.Ambiguous)
            {
                await ShowInfoAsync(
                    "Ambiguous mod match",
                    "More than one mod in the loaded instruction file matches this Nexus link.\n\n" +
                    "Remove duplicate entries or load a more specific instruction file, then click " +
                    "Mod Manager Download again on Nexus.\n\n" +
                    $"Mod page: {nxmUrl.ToModPageUrl()}")
                    .ConfigureAwait(true);
                return;
            }

            if (resolveStatus != NxmComponentResolveStatus.Matched || match?.Component is null)
            {
                await ShowInfoAsync(
                    "Mod not found in loaded instructions",
                    "No mod in the currently loaded instruction file matches this Nexus link.\n\n" +
                    "Load the correct instruction file, then click Mod Manager Download again on Nexus.\n\n" +
                    $"Mod page: {nxmUrl.ToModPageUrl()}")
                    .ConfigureAwait(true);
                return;
            }

            ModComponent component = match.Component;
            string registryUrl = match.RegistryUrl;

            await Logger.LogAsync(
                $"[NxmHandoff] Downloading Nexus file for '{component.Name}' from nxm link...")
                .ConfigureAwait(true);

            string tempPath = await DownloadOrchestrationService.DownloadModFromUrlAsync(
                nxmUrl.OriginalUrl,
                component).ConfigureAwait(true);

            if (string.IsNullOrEmpty(tempPath) || !File.Exists(tempPath))
            {
                await ShowInfoAsync(
                    "Download failed",
                    $"ModSync could not download the file for '{component.Name}'.\n\n" +
                    $"Try downloading manually from:\n{nxmUrl.ToFileUrl()}")
                    .ConfigureAwait(true);
                return;
            }

            string destinationPath = Path.Combine(
                _mainConfig.sourcePath.FullName,
                Path.GetFileName(tempPath));

            try
            {
                Directory.CreateDirectory(_mainConfig.sourcePath.FullName);
                File.Copy(tempPath, destinationPath, overwrite: true);
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[NxmHandoff] Failed to copy downloaded file to mod workspace")
                    .ConfigureAwait(true);
                await ShowInfoAsync(
                    "Download saved to temp only",
                    $"The file was downloaded but could not be copied into your mod workspace.\n\n" +
                    $"Temp file: {tempPath}\n\n{ex.Message}")
                    .ConfigureAwait(true);
                return;
            }

            string fileName = Path.GetFileName(destinationPath);
            if (!string.IsNullOrEmpty(registryUrl))
            {
                await DownloadCacheService.UpdateResourceMetadataWithFilenamesAsync(
                    component,
                    registryUrl,
                    new List<string> { fileName }).ConfigureAwait(true);

                if (component.ResourceRegistry != null
                    && component.ResourceRegistry.TryGetValue(registryUrl, out ResourceMetadata resourceMeta)
                    && resourceMeta.Files != null)
                {
                    resourceMeta.Files[fileName] = true;
                }
            }

            await ShowInfoAsync(
                "Nexus download complete",
                $"Downloaded '{Path.GetFileName(destinationPath)}' for '{component.Name}' into your mod workspace:\n\n" +
                $"{_mainConfig.sourcePath.FullName}")
                .ConfigureAwait(true);
        }

        private Task ShowInfoAsync(string title, string message)
        {
            return InformationDialog.ShowInformationDialogAsync(_parentWindow, message, title);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_subscribed)
            {
                NxmHandoffQueue.UrlEnqueued -= OnUrlEnqueued;
                _subscribed = false;
            }
        }
    }
}
