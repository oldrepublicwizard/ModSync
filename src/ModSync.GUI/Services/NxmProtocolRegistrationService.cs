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
    /// Registers ModSync as the operating-system handler for the nxm:// URL scheme
    /// (the Nexus Mods "Mod Manager Download" button).
    ///
    /// Windows: HKCU\Software\Classes\nxm via reg.exe (per-user, no elevation, no
    /// registry package dependency).
    /// Linux: a desktop entry at ~/.local/share/applications/modsync-nxm.desktop
    /// plus xdg-mime default for x-scheme-handler/nxm.
    /// macOS: declarative registration via CFBundleURLTypes in the app bundle Info.plist.
    ///
    /// Content builders are pure static methods so they stay unit-testable.
    /// </summary>
    public static class NxmProtocolRegistrationService
    {
        private const string DesktopFileName = "modsync-nxm.desktop";
        private const string SchemeHandlerMimeType = "x-scheme-handler/nxm";
        private const string WindowsClassKey = @"HKCU\Software\Classes\nxm";

        /// <summary>
        /// Builds the .desktop entry content registering the given executable as the
        /// nxm scheme handler on Linux. %u receives the nxm URL as a positional
        /// argument, which CLIArguments parses.
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
                   "Comment=ModSync nxm protocol handler for Nexus Mods downloads\n" +
                   $"Exec=\"{exePath}\" %u\n" +
                   "Terminal=false\n" +
                   "NoDisplay=true\n" +
                   $"MimeType={SchemeHandlerMimeType};\n";
        }

        /// <summary>
        /// Returns the CFBundleURLTypes XML fragment declaring the nxm URL scheme for macOS bundles.
        /// </summary>
        public static string BuildMacOsUrlTypesPlistFragment()
        {
            return "<key>CFBundleURLTypes</key>\n" +
                   "<array>\n" +
                   "  <dict>\n" +
                   "    <key>CFBundleURLName</key>\n" +
                   "    <string>Nexus Mods Protocol</string>\n" +
                   "    <key>CFBundleURLSchemes</key>\n" +
                   "    <array>\n" +
                   "      <string>nxm</string>\n" +
                   "    </array>\n" +
                   "  </dict>\n" +
                   "</array>";
        }

        /// <summary>
        /// macOS status copy for Settings when not using the Win/Linux runtime toggle.
        /// </summary>
        public static string GetMacOsSettingsStatusText()
        {
            return IsRunningInsideAppBundle()
                ? "ModSync handles Nexus Mod Manager Download links."
                : "Nexus Mod Manager Download requires the ModSync app from GitHub Releases. " +
                  "Terminal and dev builds cannot receive browser links — use --nxm=<url> instead.";
        }

        /// <summary>
        /// True when the current process is running inside a macOS .app bundle.
        /// </summary>
        public static bool IsRunningInsideAppBundle()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return false;
            }

            return UtilityHelper.IsRunningInsideAppBundle();
        }

        /// <summary>
        /// Builds the reg.exe argument lists that register the given executable as
        /// the nxm scheme handler under HKCU (no elevation needed). Each entry is a
        /// complete argument string for one reg.exe invocation.
        /// </summary>
        public static string[] BuildWindowsRegCommands(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new ArgumentException("Executable path is required", nameof(exePath));
            }

            return new[]
            {
                $"add {WindowsClassKey} /ve /d \"URL:Nexus Mods Protocol\" /f",
                $"add {WindowsClassKey} /v \"URL Protocol\" /d \"\" /f",
                $"add {WindowsClassKey}\\DefaultIcon /ve /d \"\\\"{exePath}\\\",0\" /f",
                $"add {WindowsClassKey}\\shell\\open\\command /ve /d \"\\\"{exePath}\\\" \\\"%1\\\"\" /f",
            };
        }

        /// <summary>
        /// Registers the current executable as the nxm:// handler for the current
        /// user. Returns true on success.
        /// </summary>
        public static bool Register()
        {
            try
            {
                string exePath = GetCurrentExecutablePath();
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    Core.Logger.LogWarning("[NxmProtocol] Could not determine executable path; skipping registration");
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

                Core.Logger.LogWarning("[NxmProtocol] nxm protocol registration is not supported on this platform");
                return false;
            }
            catch (Exception ex)
            {
                Core.Logger.LogException(ex, "[NxmProtocol] Failed to register nxm protocol handler");
                return false;
            }
        }

        /// <summary>
        /// Removes the nxm:// handler registration for the current user.
        /// </summary>
        public static bool Unregister()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RunRegCommand($"delete {WindowsClassKey} /f");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string desktopFilePath = GetLinuxDesktopFilePath();
                    if (File.Exists(desktopFilePath))
                    {
                        File.Delete(desktopFilePath);
                        Core.Logger.LogVerbose($"[NxmProtocol] Deleted {desktopFilePath}");
                    }

                    return true;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return UnregisterMacOs();
                }

                return false;
            }
            catch (Exception ex)
            {
                Core.Logger.LogException(ex, "[NxmProtocol] Failed to unregister nxm protocol handler");
                return false;
            }
        }

        /// <summary>
        /// Checks whether an nxm:// handler registration exists for the current user.
        /// </summary>
        public static bool IsRegistered()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return RunRegCommand($"query {WindowsClassKey}\\shell\\open\\command /ve");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return File.Exists(GetLinuxDesktopFilePath());
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return IsRegisteredMacOs();
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
                if (!RunRegCommand(arguments))
                {
                    Core.Logger.LogWarning($"[NxmProtocol] reg.exe failed for: {arguments}");
                    return false;
                }
            }

            Core.Logger.Log("[NxmProtocol] Registered nxm:// handler in HKCU\\Software\\Classes\\nxm");
            return true;
        }

        private static bool RegisterMacOs()
        {
            if (IsRunningInsideAppBundle())
            {
                Core.Logger.Log("[NxmProtocol] macOS nxm handling is declared in the app bundle Info.plist");
                return true;
            }

            Core.Logger.LogWarning("[NxmProtocol] macOS nxm links require running from ModSync.app");
            return false;
        }

        private static bool UnregisterMacOs()
        {
            Core.Logger.LogVerbose("[NxmProtocol] macOS nxm registration is bundle-declared; unregister is a no-op");
            return true;
        }

        private static bool IsRegisteredMacOs()
        {
            return IsRunningInsideAppBundle();
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
            Core.Logger.LogVerbose($"[NxmProtocol] Wrote {desktopFilePath}");

            // Associate the scheme. Failure is non-fatal: many environments pick up
            // the MimeType from the desktop entry alone.
            if (!RunProcess("xdg-mime", $"default {DesktopFileName} {SchemeHandlerMimeType}"))
            {
                Core.Logger.LogWarning("[NxmProtocol] xdg-mime default failed; desktop entry was still written");
            }

            // Refresh the desktop database when available (best effort).
            _ = RunProcess("update-desktop-database", applicationsDir ?? string.Empty);

            Core.Logger.Log("[NxmProtocol] Registered nxm:// handler via desktop entry");
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
                Core.Logger.LogWarning($"[NxmProtocol] Could not resolve process path: {ex.Message}");
                return null;
            }
        }

        private static bool RunRegCommand(string arguments)
        {
            return RunProcess("reg.exe", arguments);
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
                        try { process.Kill(); } catch { }
                        return false;
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Core.Logger.LogWarning($"[NxmProtocol] Failed to run '{fileName} {arguments}': {ex.Message}");
                return false;
            }
        }
    }
}
