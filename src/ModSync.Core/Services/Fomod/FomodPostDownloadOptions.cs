// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

namespace ModSync.Core.Services.Fomod
{
    public enum FomodPostDownloadMode
    {
        Interactive,
        WarnContinue,
        SkipAll,
        ApplyChoicesFile,
    }

    public sealed class FomodPostDownloadOptions
    {
        public FomodPostDownloadMode Mode { get; set; } = FomodPostDownloadMode.WarnContinue;

        public string ChoicesFilePath { get; set; }

        public bool ForceInteractive { get; set; }

        public bool ForceNonInteractive { get; set; }
    }
}
