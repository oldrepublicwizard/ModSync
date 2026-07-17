// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;

namespace ModSync
{
    /// <summary>
    /// Holds command-line arguments passed to the application.
    ///
    /// Usage examples:
    /// --kotorPath="C:\Path\To\Game"
    /// --modDirectory="C:\Path\To\Mods"
    /// --instructionFile="C:\Path\To\Instructions.toml"
    /// --nxm=nxm://...
    /// --modsync=modsync://install?url=...
    /// </summary>
    public static class CLIArguments
    {
        /// <summary>
        /// Path to the KOTOR game directory (destination path).
        /// </summary>
        public static string KotorPath { get; set; }

        /// <summary>
        /// Path to the mod directory (source path).
        /// </summary>
        public static string ModDirectory { get; set; }

        /// <summary>
        /// Path to the instruction file to load automatically.
        /// </summary>
        public static string InstructionFile { get; set; }

        /// <summary>
        /// nxm:// protocol URL handed to us by the OS (Nexus Mods "Mod Manager Download").
        /// Set via --nxm=&lt;url&gt; or a bare positional argument starting with nxm://.
        /// </summary>
        public static string NxmUrl { get; set; }

        /// <summary>
        /// modsync:// protocol URL for "Install with ModSync" build deep links.
        /// Set via --modsync=&lt;url&gt; or a bare positional argument starting with modsync://.
        /// </summary>
        public static string ModSyncProtocolUrl { get; set; }

        /// <summary>
        /// When true, skip single-instance enforcement so a second GUI can start (dev/agent workflows).
        /// </summary>
        public static bool AllowMultipleInstances { get; set; }

        /// <summary>True when either protocol handoff URL was provided on the CLI.</summary>
        public static bool HasProtocolHandoffUrl =>
            !string.IsNullOrEmpty(NxmUrl) || !string.IsNullOrEmpty(ModSyncProtocolUrl);

        /// <summary>
        /// Prefer nxm when both are present (single secondary-launch forward payload).
        /// </summary>
        public static string ProtocolHandoffUrl =>
            !string.IsNullOrEmpty(NxmUrl) ? NxmUrl : ModSyncProtocolUrl;

        /// <summary>
        /// Parses command-line arguments.
        /// Supports formats: --kotorPath=value, --modDirectory=value, --instructionFile=value,
        /// --nxm=value, --modsync=value, and bare nxm:// / modsync:// positional arguments.
        /// </summary>
        public static void Parse(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            foreach (string arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    continue;
                }

                // Skip argument names
                if (arg.StartsWith("--", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    // Support --key=value format
                    int equalsIndex = arg.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = arg.Substring(0, equalsIndex).TrimStart('-', ' ');
                        string value = arg.Substring(equalsIndex + 1).Trim('"', '\'');

                        switch (key.ToLowerInvariant())
                        {
                            case "kotorpath":
                                KotorPath = value;
                                Core.Logger.Log($"CLI: Set KotorPath to '{value}'");
                                break;
                            case "moddirectory":
                                ModDirectory = value;
                                Core.Logger.Log($"CLI: Set ModDirectory to '{value}'");
                                break;
                            case "instructionfile":
                                InstructionFile = value;
                                Core.Logger.Log($"CLI: Set InstructionFile to '{value}'");
                                break;
                            case "nxm":
                                NxmUrl = value;
                                Core.Logger.Log("CLI: Received nxm URL via --nxm argument");
                                break;
                            case "modsync":
                                ModSyncProtocolUrl = value;
                                Core.Logger.Log("CLI: Received modsync URL via --modsync argument");
                                break;
                        }
                    }
                    else if (string.Equals(arg, "--allow-multiple-instances", StringComparison.OrdinalIgnoreCase))
                    {
                        AllowMultipleInstances = true;
                        Core.Logger.LogVerbose("CLI: AllowMultipleInstances enabled");
                    }
                }
                else if (arg.TrimStart().StartsWith("nxm://", StringComparison.OrdinalIgnoreCase))
                {
                    // The OS protocol handler passes the nxm URL as a bare positional argument.
                    NxmUrl = arg.Trim().Trim('"', '\'');
                    Core.Logger.Log("CLI: Received nxm URL via positional argument");
                }
                else if (arg.TrimStart().StartsWith("modsync://", StringComparison.OrdinalIgnoreCase))
                {
                    ModSyncProtocolUrl = arg.Trim().Trim('"', '\'');
                    Core.Logger.Log("CLI: Received modsync URL via positional argument");
                }
            }
        }
    }
}
