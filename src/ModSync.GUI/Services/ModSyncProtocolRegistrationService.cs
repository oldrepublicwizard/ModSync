// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using ModSync.Core.Utility;

namespace ModSync.Services
{
    /// <summary>
    /// Registers ModSync as the OS handler for the modsync:// URL scheme
    /// ("Install with ModSync" build deep links). Mirrors
    /// <see cref="NxmProtocolRegistrationService"/> patterns.
    /// </summary>
    public static class ModSyncProtocolRegistrationService
    {
        private const string DesktopFileName = "modsync-protocol.desktop";
        private const string SchemeHandlerMimeType = "x-scheme-handler/modsync";
        private const string WindowsClassKey = @"HKCU\Software\Classes\modsync";

        /// <summary>
        /// Builds the .desktop entry content registering the given executable as the
        /// modsync scheme handler on Linux.
        /// </summary>
        public static string BuildDesktopFileContent(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new ArgumentException("Executable path is required", nameof(exePath));
            }

            return "[Desktop Entry]\n" +
                   "Type=Application\n" +
                   "Name=ModSync\n" +
                   "Comment=ModSync protocol handler for Install with ModSync build links\n" +
                   $"Exec=\"{exePath}\" %u\n" +
                   "Terminal=false\n" +
                   "NoDisplay=true\n" +
                   $"MimeType={SchemeHandlerMimeType};\n";
        }

        /// <summary>
        /// Returns the CFBundleURLTypes XML fragment declaring the modsync URL scheme for macOS bundles.
        /// </summary>
        public static string BuildMacOsUrlTypesPlistFragment()
        {
            return "<key>CFBundleURLTypes</key>\n" +
                   "<array>\n" +
                   "  <dict>\n" +
                   "    <key>CFBundleURLName</key>\n" +
                   "    <string>ModSync Protocol</string>\n" +
                   "    <key>CFBundleURLSchemes</key>\n" +
                   "    <array>\n" +
                   "      <string>modsync</string>\n" +
                   "    </array>\n" +
                   "  </dict>\n" +
                   "</array>";
        }

        /// <summary>
        /// Builds the reg.exe argument lists that register the given executable as
        /// the modsync scheme handler under HKCU.
        /// </summary>
        public static string[] BuildWindowsRegCommands(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new ArgumentException("Executable path is required", nameof(exePath));
            }

            return new[]
            {
                $"add {WindowsClassKey} /ve /d \"URL:ModSync Protocol\" /f",
                $"add {WindowsClassKey} /v \"URL Protocol\" /d \"\" /f",
                $"add {WindowsClassKey}\\DefaultIcon /ve /d \"\\\"{exePath}\\\",0\" /f",
                $"add {WindowsClassKey}\\shell\\open\\command /ve /d \"\\\"{exePath}\\\" \\\"%1\\\"\" /f",
            };
        }

        /// <summary>
        /// Registers the current executable as the modsync:// handler for the current user.
        /// </summary>
        public static bool Register()
        {
            try
            {
                string exePath = GetCurrentExecutablePath();
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    Core.Logger.LogWarning("[ModSyncProtocol] Could not determine executable path; skipping registration");
                    return false;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RegisterWindows(exePath);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return RegisterLinux(exePath);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return RegisterMacOs();
                }

                Core.Logger.LogWarning("[ModSyncProtocol] modsync protocol registration is not supported on this platform");
                return false;
            }
            catch (Exception ex)
            {
                Core.Logger.LogException(ex, "[ModSyncProtocol] Failed to register modsync protocol handler");
                return false;
            }
        }

        /// <summary>
        /// Removes the modsync:// handler registration for the current user.
        /// </summary>
        public static bool Unregister()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RunProcess("reg.exe", $"delete {WindowsClassKey} /f");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string desktopFilePath = GetLinuxDesktopFilePath();
                    if (File.Exists(desktopFilePath))
                    {
                        File.Delete(desktopFilePath);
                        Core.Logger.LogVerbose($"[ModSyncProtocol] Deleted {desktopFilePath}");
                    }

                    return true;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Core.Logger.LogVerbose("[ModSyncProtocol] macOS registration is bundle-declared; unregister is a no-op");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Core.Logger.LogException(ex, "[ModSyncProtocol] Failed to unregister modsync protocol handler");
                return false;
            }
        }

        /// <summary>
        /// Checks whether a modsync:// handler registration exists for the current user.
        /// </summary>
        public static bool IsRegistered()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RunProcess("reg.exe", $"query {WindowsClassKey}\\shell\\open\\command /ve");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return File.Exists(GetLinuxDesktopFilePath());
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return UtilityHelper.IsRunningInsideAppBundle();
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool RegisterWindows(string exePath)
        {
            foreach (string arguments in BuildWindowsRegCommands(exePath))
            {
                if (!RunProcess("reg.exe", arguments))
                {
                    Core.Logger.LogWarning($"[ModSyncProtocol] reg.exe failed for: {arguments}");
                    return false;
                }
            }

            Core.Logger.Log("[ModSyncProtocol] Registered modsync:// handler in HKCU\\Software\\Classes\\modsync");
            return true;
        }

        private static bool RegisterMacOs()
        {
            if (UtilityHelper.IsRunningInsideAppBundle())
            {
                Core.Logger.Log("[ModSyncProtocol] macOS modsync handling is declared in the app bundle Info.plist");
                return true;
            }

            Core.Logger.LogWarning("[ModSyncProtocol] macOS modsync links require running from ModSync.app");
            return false;
        }

        private static bool RegisterLinux(string exePath)
        {
            string desktopFilePath = GetLinuxDesktopFilePath();
            string applicationsDir = Path.GetDirectoryName(desktopFilePath);
            if (!string.IsNullOrEmpty(applicationsDir))
            {
                _ = Directory.CreateDirectory(applicationsDir);
            }

            File.WriteAllText(desktopFilePath, BuildDesktopFileContent(exePath));
            Core.Logger.LogVerbose($"[ModSyncProtocol] Wrote {desktopFilePath}");

            if (!RunProcess("xdg-mime", $"default {DesktopFileName} {SchemeHandlerMimeType}"))
            {
                Core.Logger.LogWarning("[ModSyncProtocol] xdg-mime default failed; desktop entry was still written");
            }

            _ = RunProcess("update-desktop-database", applicationsDir ?? string.Empty);
            Core.Logger.Log("[ModSyncProtocol] Registered modsync:// handler via desktop entry");
            return true;
        }

        private static string GetLinuxDesktopFilePath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "applications", DesktopFileName);
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
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[ModSyncProtocol] Could not resolve process path: {ex.Message}");
                return null;
            }
        }

        private static bool RunProcess(string fileName, string arguments)
        {
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

                    if (!process.WaitForExit(10000))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignored
                        }

                        return false;
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[ModSyncProtocol] Failed to run '{fileName} {arguments}': {ex.Message}");
                return false;
            }
        }
    }
}
