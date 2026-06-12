// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using ModSync.Core.Services.Conflicts;

namespace ModSync.Services
{
    /// <summary>
    /// Maps <see cref="ConflictAnalysisResult"/> to dialog row models (no Avalonia types).
    /// </summary>
    public static class ConflictsDialogPresenter
    {
        [NotNull]
        public static ConflictsDialogSummary BuildSummary(
            [NotNull] ConflictAnalysisResult result,
            [CanBeNull] IReadOnlyDictionary<Guid, string> componentNames = null)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.HasConflicts)
            {
                return new ConflictsDialogSummary(
                    summaryLine: result.AnalyzedComponentCount == 0
                        ? "No selected mods to analyze. Select mods and ensure the game directory is configured."
                        : $"No file conflicts among {result.AnalyzedComponentCount} selected mod(s).",
                    conflictRows: Array.Empty<ConflictRowModel>(),
                    componentCountLines: Array.Empty<string>());
            }

            string summaryLine =
                $"{result.Conflicts.Count} conflicting file path(s) across {result.AnalyzedComponentCount} selected mod(s).";

            List<ConflictRowModel> rows = result.Conflicts
                .Select(conflict => new ConflictRowModel(
                    conflict.DestinationPath,
                    conflict.Writers
                        .Select(writer => new ConflictWriterRowModel(
                            writer.ComponentName,
                            writer.ComponentGuid == conflict.WinnerComponentGuid))
                        .ToList()))
                .ToList();

            List<string> componentCountLines = result.ConflictCountsByComponent
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => ResolveComponentName(pair.Key, componentNames))
                .Select(pair =>
                {
                    string name = ResolveComponentName(pair.Key, componentNames);
                    return $"{name}: {pair.Value} conflicting path(s)";
                })
                .ToList();

            return new ConflictsDialogSummary(summaryLine, rows, componentCountLines);
        }

        [NotNull]
        private static string ResolveComponentName(Guid componentGuid, [CanBeNull] IReadOnlyDictionary<Guid, string> componentNames)
        {
            if (componentNames != null && componentNames.TryGetValue(componentGuid, out string name) && !string.IsNullOrEmpty(name))
            {
                return name;
            }

            return componentGuid.ToString();
        }
    }

    public sealed class ConflictsDialogSummary
    {
        [NotNull]
        public string SummaryLine { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<ConflictRowModel> ConflictRows { get; }

        [NotNull]
        public IReadOnlyList<string> ComponentCountLines { get; }

        public bool HasConflicts => ConflictRows.Count > 0;

        public ConflictsDialogSummary(
            [NotNull] string summaryLine,
            [NotNull][ItemNotNull] IReadOnlyList<ConflictRowModel> conflictRows,
            [NotNull] IReadOnlyList<string> componentCountLines)
        {
            SummaryLine = summaryLine ?? throw new ArgumentNullException(nameof(summaryLine));
            ConflictRows = conflictRows ?? throw new ArgumentNullException(nameof(conflictRows));
            ComponentCountLines = componentCountLines ?? throw new ArgumentNullException(nameof(componentCountLines));
        }
    }

    public sealed class ConflictRowModel
    {
        [NotNull]
        public string DestinationPath { get; }

        [NotNull]
        [ItemNotNull]
        public IReadOnlyList<ConflictWriterRowModel> Writers { get; }

        public ConflictRowModel(
            [NotNull] string destinationPath,
            [NotNull][ItemNotNull] IReadOnlyList<ConflictWriterRowModel> writers)
        {
            DestinationPath = destinationPath ?? throw new ArgumentNullException(nameof(destinationPath));
            Writers = writers ?? throw new ArgumentNullException(nameof(writers));
        }
    }

    public sealed class ConflictWriterRowModel
    {
        [NotNull]
        public string ComponentName { get; }

        public bool IsWinner { get; }

        [NotNull]
        public string DisplayLine => IsWinner
            ? $"{ComponentName} (wins)"
            : ComponentName;

        public ConflictWriterRowModel([NotNull] string componentName, bool isWinner)
        {
            ComponentName = componentName ?? throw new ArgumentNullException(nameof(componentName));
            IsWinner = isWinner;
        }
    }
}
