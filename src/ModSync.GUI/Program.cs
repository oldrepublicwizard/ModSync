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

                // Single-instance coordination for nxm:// and modsync:// protocol handlers.
                // The primary instance listens on a per-user named pipe; later
                // instances launched by the OS for a protocol link forward the URL to
                // the primary and exit instead of opening a second window.
                if (!CLIArguments.AllowMultipleInstances)
                {
                    singleInstance = new Services.SingleInstanceService();
                    if (singleInstance.TryBecomePrimary())
                    {
                        Services.ApplicationSingleInstanceContext.PrimaryInstance = singleInstance;
                        EnqueueProtocolHandoffFromCli();
                    }
                    else
                    {
                        Services.SecondaryLaunchAction action = Services.ApplicationLaunchCoordinator.DecideSecondaryAction(
                            CLIArguments.HasProtocolHandoffUrl,
                            CLIArguments.AllowMultipleInstances);

                        if (action == Services.SecondaryLaunchAction.ForwardProtocolUrlAndExit)
                        {
                            bool forwarded = singleInstance.SendToPrimaryAsync(CLIArguments.ProtocolHandoffUrl).GetAwaiter().GetResult();
                            Core.Logger.Log(forwarded
                                ? "Forwarded protocol URL to the running ModSync instance; exiting."
                                : "Could not reach the running ModSync instance to forward the protocol URL; exiting.");
                            return;
                        }

                        if (action == Services.SecondaryLaunchAction.ForwardActivateAndExit)
                        {
                            bool forwarded = singleInstance.SendToPrimaryAsync(
                                Services.ApplicationLaunchCoordinator.ActivateMessage).GetAwaiter().GetResult();
                            Core.Logger.Log(forwarded
                                ? "Forwarded activate request to the running ModSync instance; exiting."
                                : "Could not reach the running ModSync instance to activate; exiting.");
                            return;
                        }
                    }
                }
                else
                {
                    EnqueueProtocolHandoffFromCli();
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

        private static void EnqueueProtocolHandoffFromCli()
        {
            if (!string.IsNullOrEmpty(CLIArguments.NxmUrl))
            {
                Services.NxmHandoffQueue.Enqueue(CLIArguments.NxmUrl);
            }

            if (!string.IsNullOrEmpty(CLIArguments.ModSyncProtocolUrl))
            {
                Services.ModSyncHandoffQueue.Enqueue(CLIArguments.ModSyncProtocolUrl);
            }
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseReactiveUI();
    }
}
