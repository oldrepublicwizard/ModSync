// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using JetBrains.Annotations;

using ModSync.Core;
using ModSync.Core.Ports.Protocol;
using ModSync.Core.Services.Protocol;
using ModSync.Dialogs;

namespace ModSync.Services
{
    /// <summary>
    /// Drains <see cref="ModSyncHandoffQueue"/>, fetches the instruction URL from a
    /// modsync:// deep link, and loads it through the provided file-loading callback.
    /// </summary>
    public sealed class ModSyncHandoffService : IDisposable
    {
        private readonly Window _parentWindow;
        private readonly Func<string, Task<bool>> _loadInstructionFileAsync;
        private readonly Action _activateWindow;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly IProtocolHandlerRegistry _protocolRegistry;
        private bool _subscribed;
        private bool _disposed;
        private bool _processing;

        public ModSyncHandoffService(
            [NotNull] Window parentWindow,
            [NotNull] Func<string, Task<bool>> loadInstructionFileAsync,
            [NotNull] Action activateWindow,
            [CanBeNull] Func<HttpClient> httpClientFactory = null,
            [CanBeNull] IProtocolHandlerRegistry protocolRegistry = null)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            _loadInstructionFileAsync = loadInstructionFileAsync
                ?? throw new ArgumentNullException(nameof(loadInstructionFileAsync));
            _activateWindow = activateWindow ?? throw new ArgumentNullException(nameof(activateWindow));
            _httpClientFactory = httpClientFactory ?? (() => new HttpClient { Timeout = TimeSpan.FromSeconds(60) });
            _protocolRegistry = protocolRegistry ?? ProtocolHandlerRegistry.CreateDefault();
        }

        public void EnsureSubscribed()
        {
            if (_subscribed || _disposed)
            {
                return;
            }

            _subscribed = true;
            ModSyncHandoffQueue.UrlEnqueued += OnUrlEnqueued;
        }

        /// <summary>
        /// Processes all URLs currently in the hand-off queue, re-draining until empty
        /// so URLs enqueued during an active pass are not stranded.
        /// </summary>
        public async Task ProcessPendingAsync()
        {
            EnsureSubscribed();

            if (_processing || _disposed)
            {
                return;
            }

            _processing = true;
            try
            {
                while (!_disposed)
                {
                    var batch = ModSyncHandoffQueue.DrainAll();
                    if (batch.Count == 0)
                    {
                        break;
                    }

                    foreach (string rawUrl in batch)
                    {
                        await ProcessUrlAsync(rawUrl).ConfigureAwait(true);
                    }
                }
            }
            finally
            {
                _processing = false;
            }

            // Catch a race where enqueue landed between the last empty drain and clearing the flag.
            if (!_disposed && ModSyncHandoffQueue.Count > 0)
            {
                await ProcessPendingAsync().ConfigureAwait(true);
            }
        }

        private void OnUrlEnqueued(object sender, string rawUrl)
        {
            _ = Dispatcher.UIThread.InvokeAsync(async () => await ProcessPendingAsync().ConfigureAwait(true));
        }

        private async Task ProcessUrlAsync(string rawUrl)
        {
            _activateWindow();

            ProtocolHandleResult handleResult = await _protocolRegistry
                .HandleAsync(rawUrl)
                .ConfigureAwait(true);

            ModSyncUrl modSyncUrl = handleResult.Payload as ModSyncUrl;
            if (!handleResult.Accepted || modSyncUrl is null)
            {
                await ShowInfoAsync(
                        "Invalid ModSync link",
                        "ModSync could not parse the modsync:// URL handed off by the browser or CLI.")
                    .ConfigureAwait(true);
                return;
            }

            bool? confirmed = await ConfirmationDialog.ShowConfirmationDialogAsync(
                    _parentWindow,
                    confirmText:
                    "Open this ModSync build link and download its instruction file?\n\n" +
                    $"{modSyncUrl.InstructionUrl}\n\n" +
                    "Only continue for links you trust.",
                    yesButtonText: "Download & Open",
                    noButtonText: "Cancel")
                .ConfigureAwait(true);
            if (confirmed != true)
            {
                await Logger.LogAsync("[ModSyncHandoff] User declined modsync:// instruction download.")
                    .ConfigureAwait(true);
                return;
            }

            string tempPath = null;
            try
            {
                await Logger.LogAsync(
                        $"[ModSyncHandoff] Fetching instruction from '{modSyncUrl.InstructionUrl}'...")
                    .ConfigureAwait(true);

                using (HttpClient client = _httpClientFactory())
                {
                    tempPath = await ModSyncInstructionFetcher
                        .DownloadToTempFileAsync(modSyncUrl.InstructionUrl, client)
                        .ConfigureAwait(true);
                }

                bool loaded = await _loadInstructionFileAsync(tempPath).ConfigureAwait(true);
                if (!loaded)
                {
                    await ShowInfoAsync(
                            "Could not load instruction file",
                            "ModSync downloaded the build link but failed to load the instruction file.\n\n" +
                            $"URL: {modSyncUrl.InstructionUrl}")
                        .ConfigureAwait(true);
                    return;
                }

                await Logger.LogAsync(
                        $"[ModSyncHandoff] Loaded instruction from modsync:// ({modSyncUrl.Action}" +
                        (string.IsNullOrEmpty(modSyncUrl.Game) ? string.Empty : ", game=" + modSyncUrl.Game) +
                        ").")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await Logger.LogExceptionAsync(ex, "[ModSyncHandoff] Failed to consume modsync:// URL")
                    .ConfigureAwait(true);
                await ShowInfoAsync(
                        "ModSync link failed",
                        "ModSync could not download or open the instruction file from the build link.\n\n" +
                        $"{ex.Message}\n\nURL: {modSyncUrl.InstructionUrl}")
                    .ConfigureAwait(true);
            }
            finally
            {
                TryDeleteTempFile(tempPath);
            }
        }

        private static void TryDeleteTempFile(string tempPath)
        {
            if (string.IsNullOrEmpty(tempPath))
            {
                return;
            }

            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogVerbose($"[ModSyncHandoff] Could not delete temp file '{tempPath}': {ex.Message}");
            }
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
                ModSyncHandoffQueue.UrlEnqueued -= OnUrlEnqueued;
                _subscribed = false;
            }
        }
    }
}
