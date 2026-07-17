// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;

namespace ModSync
{
    public static class CLIArguments
    {
        public static string KotorPath { get; set; }
        public static string ModDirectory { get; set; }
        public static string InstructionFile { get; set; }
        public static string NxmUrl { get; set; }
        public static string ModSyncProtocolUrl { get; set; }
        public static bool AllowMultipleInstances { get; set; }

        public static bool HasProtocolHandoffUrl =>
            !string.IsNullOrEmpty(NxmUrl) || !string.IsNullOrEmpty(ModSyncProtocolUrl);

        public static string ProtocolHandoffUrl =>
            !string.IsNullOrEmpty(NxmUrl) ? NxmUrl : ModSyncProtocolUrl;

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

                if (arg.StartsWith("--", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
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
