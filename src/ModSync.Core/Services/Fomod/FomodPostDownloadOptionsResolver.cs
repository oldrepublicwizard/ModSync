// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;

using ModSync.Core.CLI;

namespace ModSync.Core.Services.Fomod
{
    public static class FomodPostDownloadOptionsResolver
    {
        public const string EnvChoicesPath = "MODSYNC_FOMOD_CHOICES";

        public const string EnvPostDownloadMode = "MODSYNC_FOMOD_POST_DOWNLOAD_MODE";

        public const string SettingsKey = "fomodPostDownloadMode";

        public static FomodPostDownloadOptions Resolve(
            bool fomodSkip,
            string fomodChoicesPath,
            bool forceInteractive,
            bool forceNonInteractive,
            string settingsMode)
        {
            if (fomodSkip)
            {
                return new FomodPostDownloadOptions { Mode = FomodPostDownloadMode.SkipAll };
            }

            string choicesPath = !string.IsNullOrWhiteSpace(fomodChoicesPath)
                ? fomodChoicesPath
                : Environment.GetEnvironmentVariable(EnvChoicesPath);

            if (!string.IsNullOrWhiteSpace(choicesPath))
            {
                return new FomodPostDownloadOptions
                {
                    Mode = FomodPostDownloadMode.ApplyChoicesFile,
                    ChoicesFilePath = choicesPath,
                    ForceInteractive = forceInteractive,
                    ForceNonInteractive = forceNonInteractive,
                };
            }

            string mode = !string.IsNullOrWhiteSpace(settingsMode)
                ? settingsMode
                : Environment.GetEnvironmentVariable(EnvPostDownloadMode);

            if (string.Equals(mode, "skip", StringComparison.OrdinalIgnoreCase))
            {
                return new FomodPostDownloadOptions { Mode = FomodPostDownloadMode.SkipAll };
            }

            if (forceInteractive)
            {
                return new FomodPostDownloadOptions
                {
                    Mode = FomodPostDownloadMode.Interactive,
                    ForceInteractive = forceInteractive,
                    ForceNonInteractive = forceNonInteractive,
                };
            }

            if (string.Equals(mode, "warn-continue", StringComparison.OrdinalIgnoreCase))
            {
                return new FomodPostDownloadOptions
                {
                    Mode = FomodPostDownloadMode.WarnContinue,
                    ForceInteractive = forceInteractive,
                    ForceNonInteractive = forceNonInteractive,
                };
            }

            if (ConsoleInteractionCapabilities.IsInteractiveTerminal(false, forceNonInteractive))
            {
                return new FomodPostDownloadOptions
                {
                    Mode = FomodPostDownloadMode.Interactive,
                    ForceInteractive = forceInteractive,
                    ForceNonInteractive = forceNonInteractive,
                };
            }

            return new FomodPostDownloadOptions
            {
                Mode = FomodPostDownloadMode.WarnContinue,
                ForceInteractive = forceInteractive,
                ForceNonInteractive = forceNonInteractive,
            };
        }

        public static IFomodPostDownloadHost CreateHost(FomodPostDownloadOptions options)
        {
            switch (options.Mode)
            {
                case FomodPostDownloadMode.SkipAll:
                    return new FomodSkipPostDownloadHost();
                case FomodPostDownloadMode.ApplyChoicesFile:
                    if (string.IsNullOrWhiteSpace(options.ChoicesFilePath) || !File.Exists(options.ChoicesFilePath))
                    {
                        throw new FileNotFoundException(
                            $"FOMOD choices file not found: '{options.ChoicesFilePath}'.");
                    }

                    FomodChoicesFile choicesFile = FomodChoicesApplier.LoadFromFile(options.ChoicesFilePath);
                    return new FomodChoicesFileHost(choicesFile);
                case FomodPostDownloadMode.Interactive:
                    return new FomodConsolePostDownloadHost();
                case FomodPostDownloadMode.WarnContinue:
                default:
                    return new FomodWarnContinuePostDownloadHost();
            }
        }
    }
}
