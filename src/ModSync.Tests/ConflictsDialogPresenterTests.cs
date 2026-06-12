// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

using ModSync.Core.Services.Conflicts;
using ModSync.Services;
using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class ConflictsDialogPresenterTests
    {
        [Test]
        public void BuildSummary_NoConflicts_ReturnsPositiveSummary()
        {
            var result = new ConflictAnalysisResult(
                Array.Empty<FileConflict>(),
                new Dictionary<Guid, int>(),
                analyzedComponentCount: 3);

            ConflictsDialogSummary summary = ConflictsDialogPresenter.BuildSummary(result);

            Assert.That(summary.HasConflicts, Is.False);
            Assert.That(summary.SummaryLine, Does.Contain("No file conflicts"));
            Assert.That(summary.ConflictRows, Is.Empty);
        }

        [Test]
        public void BuildSummary_WithConflict_MarksWinnerAndCounts()
        {
            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();
            var writers = new List<FileConflictWriter>
            {
                new FileConflictWriter(firstGuid, "First Mod", 0),
                new FileConflictWriter(secondGuid, "Second Mod", 2),
            };
            var conflict = new FileConflict("<<kotorDirectory>>/Override/foo.tpc", writers, secondGuid);
            var counts = new Dictionary<Guid, int>
            {
                [firstGuid] = 1,
                [secondGuid] = 1,
            };
            var result = new ConflictAnalysisResult(
                new List<FileConflict> { conflict },
                counts,
                analyzedComponentCount: 2);

            ConflictsDialogSummary summary = ConflictsDialogPresenter.BuildSummary(result);

            Assert.That(summary.HasConflicts, Is.True);
            Assert.That(summary.SummaryLine, Does.Contain("1 conflicting"));
            Assert.That(summary.ConflictRows.Count, Is.EqualTo(1));
            Assert.That(summary.ConflictRows[0].DestinationPath, Does.Contain("foo.tpc"));
            Assert.That(summary.ConflictRows[0].Writers[1].DisplayLine, Does.Contain("(wins)"));
        }

        [Test]
        public void BuildSummary_UsesComponentNameLookupForCounts()
        {
            Guid modGuid = Guid.NewGuid();
            var writers = new List<FileConflictWriter>
            {
                new FileConflictWriter(modGuid, "Alpha", 0),
                new FileConflictWriter(Guid.NewGuid(), "Beta", 1),
            };
            var conflict = new FileConflict("<<kotorDirectory>>/Override/a.2da", writers, writers[1].ComponentGuid);
            var counts = new Dictionary<Guid, int> { [modGuid] = 1 };
            var result = new ConflictAnalysisResult(
                new List<FileConflict> { conflict },
                counts,
                analyzedComponentCount: 2);

            var names = new Dictionary<Guid, string> { [modGuid] = "Alpha Mod" };
            ConflictsDialogSummary summary = ConflictsDialogPresenter.BuildSummary(result, names);

            Assert.That(summary.ComponentCountLines[0], Does.Contain("Alpha Mod"));
        }
    }
}
