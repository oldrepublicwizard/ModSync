// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModSync.Services
{
    /// <summary>
    /// Named-pipe single-instance coordination. The first ModSync process claims a
    /// per-user pipe name and listens for messages; later processes detect the
    /// primary and forward their payload (typically an nxm:// URL) over the pipe
    /// instead of starting a second GUI.
    ///
    /// Uses System.IO.Pipes which works on both Windows (real named pipes) and
    /// Unix (domain sockets under the temp directory).
    /// </summary>
    public sealed class SingleInstanceService : IDisposable
    {
        private const int ConnectProbeTimeoutMs = 500;

        private readonly string _pipeName;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private NamedPipeServerStream _pendingServer;
        private Task _listenerTask;
        private bool _disposed;

        /// <summary>Raised for every message received from a secondary instance.</summary>
        public event EventHandler<string> MessageReceived;

        /// <summary>Raised when a secondary instance requests the primary window be activated.</summary>
        public event EventHandler ActivationRequested;

        /// <summary>True after <see cref="TryBecomePrimary"/> succeeded.</summary>
        public bool IsPrimary { get; private set; }

        /// <summary>Pipe name in use (useful for diagnostics and tests).</summary>
        public string PipeName => _pipeName;

        /// <param name="pipeName">
        /// Override for tests. Defaults to a per-user name so different users on the
        /// same machine do not collide.
        /// </param>
        public SingleInstanceService(string pipeName = null)
        {
            _pipeName = string.IsNullOrWhiteSpace(pipeName) ? GetDefaultPipeName() : pipeName;
        }

        /// <summary>
        /// Default per-user pipe name, sanitized to be filesystem-safe because the
        /// Unix implementation backs pipes with socket files.
        /// </summary>
        public static string GetDefaultPipeName()
        {
            string user = Environment.UserName ?? "default";
            var sanitized = new StringBuilder(user.Length);
            foreach (char c in user)
            {
                _ = sanitized.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return $"ModSync-SingleInstance-{sanitized}";
        }

        /// <summary>
        /// Attempts to claim the pipe and become the primary instance. On success the
        /// background listener loop is started and this returns true. Returns false
        /// when another live primary already owns the pipe.
        /// Robust to stale Unix socket files left behind by crashed processes.
        /// </summary>
        public bool TryBecomePrimary()
        {
            if (IsPrimary)
            {
                return true;
            }

            NamedPipeServerStream server = TryCreateServer();
            if (server is null)
            {
                // Creation failed - either a live primary owns the pipe, or a stale
                // Unix socket file is in the way. Probe with a client connect.
                if (CanConnectToPrimary())
                {
                    return false;
                }

                TryRemoveStaleUnixSocket();
                server = TryCreateServer();
                if (server is null)
                {
                    return false;
                }
            }

            _pendingServer = server;
            IsPrimary = true;
            _listenerTask = Task.Run(() => ListenLoopAsync(_cts.Token));
            Core.Logger.LogVerbose($"[SingleInstance] Became primary on pipe '{_pipeName}'");
            return true;
        }

        /// <summary>
        /// Sends a message to the primary instance. Returns true when the message was
        /// written successfully, false when no primary could be reached.
        /// </summary>
        public async Task<bool> SendToPrimaryAsync(string message, int timeoutMs = 3000)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            try
            {
                using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out, PipeOptions.Asynchronous))
                {
                    await client.ConnectAsync(timeoutMs).ConfigureAwait(false);
                    using (var writer = new StreamWriter(client, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
                    {
                        await writer.WriteLineAsync(message).ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                    }
                }

                Core.Logger.LogVerbose($"[SingleInstance] Forwarded message to primary on pipe '{_pipeName}'");
                return true;
            }
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[SingleInstance] Failed to forward message to primary: {ex.Message}");
                return false;
            }
        }

        private NamedPipeServerStream TryCreateServer()
        {
            try
            {
                return new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return null;
            }
        }

        private bool CanConnectToPrimary()
        {
            try
            {
                using (var probe = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out))
                {
                    probe.Connect(ConnectProbeTimeoutMs);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void TryRemoveStaleUnixSocket()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return;
            }

            try
            {
                // The Unix implementation of named pipes places socket files at
                // $TMPDIR/CoreFxPipe_{name} (Path.GetTempPath respects TMPDIR).
                string socketPath = Path.Combine(Path.GetTempPath(), $"CoreFxPipe_{_pipeName}");
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                    Core.Logger.LogVerbose($"[SingleInstance] Removed stale pipe socket: {socketPath}");
                }
            }
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[SingleInstance] Could not remove stale pipe socket: {ex.Message}");
            }
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                NamedPipeServerStream server = Interlocked.Exchange(ref _pendingServer, null) ?? TryCreateServer();
                if (server is null)
                {
                    // Transient race (e.g. previous instance not fully released yet).
                    try
                    {
                        await Task.Delay(250, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    continue;
                }

                try
                {
                    using (server)
                    {
                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);
                        using (var reader = new StreamReader(server, Encoding.UTF8))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                            {
                                HandleMessage(line);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Core.Logger.LogWarning($"[SingleInstance] Listener error (will retry): {ex.Message}");
                }
            }
        }

        private void HandleMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string trimmed = message.Trim();
            Core.Logger.LogVerbose($"[SingleInstance] Received message: {trimmed}");

            if (trimmed.StartsWith("nxm://", StringComparison.OrdinalIgnoreCase))
            {
                NxmHandoffQueue.Enqueue(trimmed);
            }
            else if (trimmed.StartsWith("modsync://", StringComparison.OrdinalIgnoreCase))
            {
                ModSyncHandoffQueue.Enqueue(trimmed);
            }
            else if (string.Equals(trimmed, ApplicationLaunchCoordinator.ActivateMessage, StringComparison.Ordinal))
            {
                ActivationRequested?.Invoke(this, EventArgs.Empty);
            }

            MessageReceived?.Invoke(this, trimmed);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cts.Cancel();

            try
            {
                _pendingServer?.Dispose();
            }
            catch
            {
                // Best effort - the listener loop owns the active server instance.
            }

            try
            {
                _ = _listenerTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Listener exceptions are already logged inside the loop.
            }

            _cts.Dispose();
        }
    }
}
