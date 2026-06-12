// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;

using Avalonia;
using Avalonia.ReactiveUI;

namespace ModSync
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Services.SingleInstanceService singleInstance = null;
            try
            {
                Core.Logger.Initialize();

                // Parse command-line arguments
                CLIArguments.Parse(args);

                // Single-instance coordination for the nxm:// protocol handler.
                // The primary instance listens on a per-user named pipe; later
                // instances launched by the OS for an nxm link forward the URL to
                // the primary and exit instead of opening a second window.
                singleInstance = new Services.SingleInstanceService();
                if (singleInstance.TryBecomePrimary())
                {
                    if (!string.IsNullOrEmpty(CLIArguments.NxmUrl))
                    {
                        Services.NxmHandoffQueue.Enqueue(CLIArguments.NxmUrl);
                    }
                }
                else if (!string.IsNullOrEmpty(CLIArguments.NxmUrl))
                {
                    bool forwarded = singleInstance.SendToPrimaryAsync(CLIArguments.NxmUrl).GetAwaiter().GetResult();
                    Core.Logger.Log(forwarded
                        ? "Forwarded nxm URL to the running ModSync instance; exiting."
                        : "Could not reach the running ModSync instance to forward the nxm URL; exiting.");
                    return;
                }

                // Telemetry is initialized lazily inside MainWindow.InitializeTelemetryIfEnabled
                // (called on window open) so that consent can be obtained from the user first.
                // Record session start only if telemetry is actually enabled after that check.
                Core.Services.TelemetryService.Instance.RecordSessionStart(
                    componentCount: 0,
                    selectedCount: 0
                );

                var startTime = System.Diagnostics.Stopwatch.StartNew();
                _ = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                startTime.Stop();

                // Record session end on exit
                Core.Services.TelemetryService.Instance.RecordSessionEnd(
                    durationMs: startTime.Elapsed.TotalMilliseconds,
                    completed: true
                );
                Core.Services.TelemetryService.Instance.Flush();
            }
            catch (Exception ex)
            {
                Core.Logger.LogException(ex);
                Core.Services.TelemetryService.Instance.RecordSessionEnd(
                    durationMs: 0,
                    completed: false
                );
                Core.Services.TelemetryService.Instance.Flush();

                throw;
            }
            finally
            {
                singleInstance?.Dispose();
            }
        }


        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
    }
}
