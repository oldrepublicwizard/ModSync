// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace ModSync.Core.Services.Installation
{
    public static class ManagedInstallSummaryFormatter
    {
        [NotNull]
        public static string FormatWizardSummary([CanBeNull] ManagedInstallResult result)
        {
            if (result is null || !result.ManagedModeUsed)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            builder.AppendLine("Managed deployment recorded manifests for file-operation mods installed in this session.");
            builder.AppendLine($"Profile: {result.ActiveProfileName}");
            builder.AppendLine($"Manifests written: {result.ManifestsWritten}");

            if (result.PatcherProvenanceRecorded)
            {
                builder.AppendLine("Patcher outputs were recorded into deployment manifests for uninstall.");
            }

            if (result.HasPatcherComponents)
            {
                builder.AppendLine();
                builder.AppendLine("Warning: Some patcher-touched mods have no recorded live file provenance:");
                foreach (string name in result.PatcherComponentNames)
                {
                    builder.AppendLine($"• {name}");
                }
            }

            return builder.ToString().TrimEnd();
        }

        [NotNull]
        public static string FormatCliWarningLine([CanBeNull] ManagedInstallResult result)
        {
            if (result is null || !result.HasPatcherComponents)
            {
                return string.Empty;
            }

            string names = string.Join(", ", result.PatcherComponentNames);
            return $"WARN: Patcher-modified files are not tracked for managed uninstall. Mods: {names}";
        }
    }
}
