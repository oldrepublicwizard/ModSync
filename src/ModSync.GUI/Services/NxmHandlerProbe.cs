// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ModSync.Core.Utility;

namespace ModSync.Services
{
    public enum NxmHandlerStatus
    {
        ModSyncActive,
        CompetitorActive,
        ModSyncRegisteredNotDefault,
        Unregistered,
        Unknown,
    }

    public enum NxmHandlerIdentity
    {
        None,
        ModSync,
        ModOrganizer2,
        Vortex,
        Other,
    }

    public sealed class NxmHandlerProbeResult
    {
        public NxmHandlerStatus Status { get; set; }

        public NxmHandlerIdentity Identity { get; set; }

        public string ExecutablePath { get; set; }

        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Read-only probe for who owns the nxm:// URL scheme. Pure parsers are unit-tested;
    /// <see cref="Probe"/> shells out on Win/Linux for live status.
    /// </summary>
    public static class NxmHandlerProbe
    {
        private const string SchemeHandlerMimeType = "x-scheme-handler/nxm";
        private const string ModSyncDesktopFileName = "modsync-nxm.desktop";
        private const string WindowsOpenCommandKey = @"HKCU\Software\Classes\nxm\shell\open\command";

        public static string ParseWindowsOpenCommandFromRegQueryOutput(string regOutput)
        {
            if (string.IsNullOrWhiteSpace(regOutput))
            {
                return null;
            }

            foreach (string rawLine in regOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (!line.Contains("REG_SZ", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int regIndex = line.IndexOf("REG_SZ", StringComparison.OrdinalIgnoreCase);
                string valuePart = line.Substring(regIndex + "REG_SZ".Length).Trim();
                return ExtractExecutableFromOpenCommand(valuePart);
            }

            return null;
        }

        public static string ParseDesktopExecLine(string desktopFileContent)
        {
            if (string.IsNullOrWhiteSpace(desktopFileContent))
            {
                return null;
            }

            foreach (string rawLine in desktopFileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (!line.StartsWith("Exec=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string execValue = line.Substring("Exec=".Length).Trim();
                if (execValue.StartsWith("\"", StringComparison.Ordinal))
                {
                    int endQuote = execValue.IndexOf('"', 1);
                    if (endQuote > 0)
                    {
                        return execValue.Substring(1, endQuote - 1);
                    }
                }

                int spaceIndex = execValue.IndexOf(' ');
                return spaceIndex > 0 ? execValue.Substring(0, spaceIndex) : execValue;
            }

            return null;
        }

        public static NxmHandlerIdentity IdentifyHandler(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return NxmHandlerIdentity.None;
            }

            string normalized = executablePath.Replace('\\', '/');
            if (normalized.IndexOf("ModOrganizer", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("/MO2", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("mo2.exe", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NxmHandlerIdentity.ModOrganizer2;
            }

            if (normalized.IndexOf("Vortex", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("NexusClient", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("BlackTreeGaming", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NxmHandlerIdentity.Vortex;
            }

            return NxmHandlerIdentity.Other;
        }

        public static string GetHandlerDisplayName(NxmHandlerIdentity identity, string executablePath)
        {
            switch (identity)
            {
                case NxmHandlerIdentity.ModOrganizer2:
                    return "Mod Organizer 2";
                case NxmHandlerIdentity.Vortex:
                    return "Vortex";
                case NxmHandlerIdentity.ModSync:
                    return "ModSync";
                case NxmHandlerIdentity.Other:
                    return string.IsNullOrWhiteSpace(executablePath)
                        ? "Another app"
                        : $"Another app ({Path.GetFileName(executablePath)})";
                default:
                    return null;
            }
        }

        public static NxmHandlerProbeResult Evaluate(
            string activeHandlerExecutable,
            string modSyncExecutable,
            bool modSyncDesktopFileExists,
            string linuxDefaultDesktopFileName)
        {
            bool modSyncOwnsActive = PathsReferToSameExecutable(activeHandlerExecutable, modSyncExecutable);
            if (modSyncOwnsActive)
            {
                return new NxmHandlerProbeResult
                {
                    Status = NxmHandlerStatus.ModSyncActive,
                    Identity = NxmHandlerIdentity.ModSync,
                    ExecutablePath = activeHandlerExecutable ?? modSyncExecutable,
                    DisplayName = "ModSync",
                };
            }

            if (!string.IsNullOrWhiteSpace(activeHandlerExecutable))
            {
                NxmHandlerIdentity identity = IdentifyHandler(activeHandlerExecutable);
                return new NxmHandlerProbeResult
                {
                    Status = NxmHandlerStatus.CompetitorActive,
                    Identity = identity,
                    ExecutablePath = activeHandlerExecutable,
                    DisplayName = GetHandlerDisplayName(identity, activeHandlerExecutable),
                };
            }

            if (modSyncDesktopFileExists
                && !string.IsNullOrWhiteSpace(linuxDefaultDesktopFileName)
                && !string.Equals(linuxDefaultDesktopFileName, ModSyncDesktopFileName, StringComparison.OrdinalIgnoreCase))
            {
                return new NxmHandlerProbeResult
                {
                    Status = NxmHandlerStatus.ModSyncRegisteredNotDefault,
                    Identity = NxmHandlerIdentity.None,
                    DisplayName = linuxDefaultDesktopFileName,
                };
            }

            if (modSyncDesktopFileExists)
            {
                return new NxmHandlerProbeResult
                {
                    Status = NxmHandlerStatus.ModSyncRegisteredNotDefault,
                    Identity = NxmHandlerIdentity.None,
                    DisplayName = "another application",
                };
            }

            return new NxmHandlerProbeResult
            {
                Status = NxmHandlerStatus.Unregistered,
                Identity = NxmHandlerIdentity.None,
            };
        }

        public static string BuildSettingsStatusText(NxmHandlerProbeResult probe, bool registrationPreferenceEnabled)
        {
            if (probe is null)
            {
                return "Could not verify Nexus Mod Manager handler status.";
            }

            switch (probe.Status)
            {
                case NxmHandlerStatus.ModSyncActive:
                    return "ModSync handles nxm:// links from Nexus Mod Manager Download.";
                case NxmHandlerStatus.CompetitorActive:
                    return $"{probe.DisplayName} currently handles Nexus Mod Manager Download links.";
                case NxmHandlerStatus.ModSyncRegisteredNotDefault:
                    if (registrationPreferenceEnabled)
                    {
                        return "ModSync is registered but another app is still the default nxm:// handler. Save settings to retry.";
                    }

                    return "ModSync has a registration entry, but another app may receive browser links.";
                case NxmHandlerStatus.Unregistered:
                    return registrationPreferenceEnabled
                        ? "Registration is enabled but not yet applied — save settings to register."
                        : "Enable the checkbox above to register ModSync for Nexus Mod Manager Download.";
                case NxmHandlerStatus.Unknown:
                default:
                    return "Could not verify Nexus Mod Manager handler status. Check the log for details.";
            }
        }

        public static NxmHandlerProbeResult Probe(string modSyncExecutable = null)
        {
            try
            {
                modSyncExecutable = modSyncExecutable ?? GetCurrentExecutablePath();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ProbeWindows(modSyncExecutable);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return ProbeLinux(modSyncExecutable);
                }

                return new NxmHandlerProbeResult { Status = NxmHandlerStatus.Unknown };
            }
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[NxmProtocol] Handler probe failed: {ex.Message}");
                return new NxmHandlerProbeResult { Status = NxmHandlerStatus.Unknown };
            }
        }

        private static NxmHandlerProbeResult ProbeWindows(string modSyncExecutable)
        {
            if (!TryRunProcessCaptureOutput("reg.exe", $"query \"{WindowsOpenCommandKey}\" /ve", out string output))
            {
                return new NxmHandlerProbeResult { Status = NxmHandlerStatus.Unknown };
            }

            string activeExe = ParseWindowsOpenCommandFromRegQueryOutput(output);
            return Evaluate(activeExe, modSyncExecutable, modSyncDesktopFileExists: false, linuxDefaultDesktopFileName: null);
        }

        private static NxmHandlerProbeResult ProbeLinux(string modSyncExecutable)
        {
            bool modSyncDesktopExists = File.Exists(GetModSyncDesktopFilePath());
            string defaultDesktop = null;
            string activeExe = null;

            if (TryRunProcessCaptureOutput("xdg-mime", $"query default {SchemeHandlerMimeType}", out string mimeOutput))
            {
                defaultDesktop = mimeOutput.Trim();
                string desktopPath = ResolveLinuxDesktopFilePath(defaultDesktop);
                if (!string.IsNullOrEmpty(desktopPath) && File.Exists(desktopPath))
                {
                    activeExe = ParseDesktopExecLine(File.ReadAllText(desktopPath));
                }
            }

            if (string.IsNullOrWhiteSpace(activeExe) && modSyncDesktopExists)
            {
                activeExe = ParseDesktopExecLine(File.ReadAllText(GetModSyncDesktopFilePath()));
            }

            return Evaluate(
                activeExe,
                modSyncExecutable,
                modSyncDesktopExists,
                defaultDesktop);
        }

        private static string GetModSyncDesktopFilePath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "applications", ModSyncDesktopFileName);
        }

        private static string ResolveLinuxDesktopFilePath(string desktopFileName)
        {
            if (string.IsNullOrWhiteSpace(desktopFileName))
            {
                return null;
            }

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] candidates =
            {
                Path.Combine(home, ".local", "share", "applications", desktopFileName),
                Path.Combine("/usr", "share", "applications", desktopFileName),
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string ExtractExecutableFromOpenCommand(string openCommandValue)
        {
            if (string.IsNullOrWhiteSpace(openCommandValue))
            {
                return null;
            }

            string trimmed = openCommandValue.Trim();
            if (trimmed.StartsWith("\"", StringComparison.Ordinal))
            {
                int endQuote = trimmed.IndexOf('"', 1);
                if (endQuote > 0)
                {
                    return trimmed.Substring(1, endQuote - 1);
                }
            }

            int spaceIndex = trimmed.IndexOf(' ', StringComparison.Ordinal);
            return spaceIndex > 0 ? trimmed.Substring(0, spaceIndex).Trim('"') : trimmed.Trim('"');
        }

        public static bool PathsReferToSameExecutable(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            try
            {
                string normalizedLeft = Path.GetFullPath(left);
                string normalizedRight = Path.GetFullPath(right);
                return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return string.Equals(left.Trim('"'), right.Trim('"'), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string GetCurrentExecutablePath()
        {
            try
            {
                using (Process current = Process.GetCurrentProcess())
                {
                    return current.MainModule?.FileName;
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool TryRunProcessCaptureOutput(string fileName, string arguments, out string output)
        {
            output = null;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process is null)
                    {
                        return false;
                    }

                    var stdout = new StringBuilder();
                    process.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            _ = stdout.AppendLine(e.Data);
                        }
                    };
                    process.BeginOutputReadLine();

                    if (!process.WaitForExit(10000))
                    {
                        try { process.Kill(); } catch { }
                        return false;
                    }

                    output = stdout.ToString();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
