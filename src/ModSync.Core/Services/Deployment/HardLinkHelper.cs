// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace ModSync.Core.Services.Deployment
{
    /// <summary>
    /// Minimal cross-platform hardlink creation. .NET has no managed hardlink API,
    /// so this wraps CreateHardLink (kernel32) on Windows and link() (libc) on Unix.
    /// Callers are expected to fall back to File.Copy when this returns false.
    /// </summary>
    internal static class HardLinkHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateHardLinkW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateHardLinkWindows(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        [DllImport("libc", SetLastError = true, EntryPoint = "link")]
        private static extern int LinkUnix(string oldPath, string newPath);

        /// <summary>
        /// Attempts to create a hardlink at <paramref name="linkPath"/> pointing to the
        /// same data as <paramref name="existingFilePath"/>. Returns false on any failure
        /// (cross-device, unsupported filesystem, permissions, missing OS support) so the
        /// caller can fall back to a plain copy. Never throws for I/O-level failures.
        /// </summary>
        internal static bool TryCreateHardLink(string existingFilePath, string linkPath)
        {
            if (string.IsNullOrWhiteSpace(existingFilePath) || string.IsNullOrWhiteSpace(linkPath))
            {
                return false;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return CreateHardLinkWindows(linkPath, existingFilePath, IntPtr.Zero);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return LinkUnix(existingFilePath, linkPath) == 0;
                }

                return false;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (EntryPointNotFoundException)
            {
                return false;
            }
        }
    }
}
