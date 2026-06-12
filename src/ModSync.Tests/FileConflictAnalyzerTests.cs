// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ModSync.Core;
using ModSync.Core.Services.Conflicts;

using NUnit.Framework;

namespace ModSync.Tests
{
    [TestFixture]
    public sealed class FileConflictAnalyzerTests
    {
        private string _testDirectory;
        private string _modDirectory;
        private string _kotorDirectory;
        private DirectoryInfo _savedSourcePath;
        private DirectoryInfo _savedDestinationPath;

        [SetUp]
        public void SetUp()
        {
            _savedSourcePath = MainConfig.SourcePath;
            _savedDestinationPath = MainConfig.DestinationPath;

            _testDirectory = Path.Combine(Path.GetTempPath(), "ModSync_ConflictTests_" + Guid.NewGuid());
            _modDirectory = Path.Combine(_testDirectory, "Mods");
            _kotorDirectory = Path.Combine(_testDirectory, "KOTOR");
            Directory.CreateDirectory(_modDirectory);
            Directory.CreateDirectory(_kotorDirectory);
            Directory.CreateDirectory(Path.Combine(_kotorDirectory, "Override"));

            MainConfig.Instance.sourcePath = new DirectoryInfo(_modDirectory);
            MainConfig.Instance.destinationPath = new DirectoryInfo(_kotorDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            MainConfig.Instance.sourcePath = _savedSourcePath;
            MainConfig.Instance.destinationPath = _savedDestinationPath;

            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private static ModComponent CreateCopyToOverrideComponent(string name, Guid guid, string relativeModFile)
        {
            var component = new ModComponent
            {
                Name = name,
                Guid = guid,
                IsSelected = true,
            };

            component.Instructions.Add(new Instruction
            {
                Action = Instruction.ActionType.Copy,
                Source = new List<string> { "<<modDirectory>>/" + relativeModFile },
                Destination = "<<kotorDirectory>>/Override",
            });

            return component;
        }

        [Test]
        public async Task AnalyzeAsync_TwoComponentsSameDestination_ReportsConflictWithSecondWinner()
        {
            Directory.CreateDirectory(Path.Combine(_modDirectory, "mod1"));
            Directory.CreateDirectory(Path.Combine(_modDirectory, "mod2"));
            File.WriteAllText(Path.Combine(_modDirectory, "mod1", "contested.txt"), "first");
            File.WriteAllText(Path.Combine(_modDirectory, "mod2", "contested.txt"), "second");

            Guid firstGuid = Guid.NewGuid();
            Guid secondGuid = Guid.NewGuid();
            var first = CreateCopyToOverrideComponent("First", firstGuid, "mod1/contested.txt");
            var second = CreateCopyToOverrideComponent("Second", secondGuid, "mod2/contested.txt");

            var analyzer = new FileConflictAnalyzer();
            ConflictAnalysisResult result = await analyzer.AnalyzeAsync(new List<ModComponent> { first, second }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result.AnalyzedComponentCount, Is.EqualTo(2));
                Assert.That(result.Conflicts.Count, Is.EqualTo(1));
                FileConflict conflict = result.Conflicts[0];
                Assert.That(conflict.DestinationPath, Is.EqualTo("<<kotorDirectory>>/Override/contested.txt"));
                Assert.That(conflict.Writers.Count, Is.EqualTo(2));
                Assert.That(conflict.Writers[0].ComponentGuid, Is.EqualTo(firstGuid));
                Assert.That(conflict.Writers[1].ComponentGuid, Is.EqualTo(secondGuid));
                Assert.That(conflict.WinnerComponentGuid, Is.EqualTo(secondGuid));
            });
        }

        [Test]
        public async Task AnalyzeAsync_DistinctDestinations_ReportsNoConflicts()
        {
            File.WriteAllText(Path.Combine(_modDirectory, "one.txt"), "one");
            File.WriteAllText(Path.Combine(_modDirectory, "two.txt"), "two");

            var first = CreateCopyToOverrideComponent("First", Guid.NewGuid(), "one.txt");
            var second = CreateCopyToOverrideComponent("Second", Guid.NewGuid(), "two.txt");

            var analyzer = new FileConflictAnalyzer();
            ConflictAnalysisResult result = await analyzer.AnalyzeAsync(new List<ModComponent> { first, second }).ConfigureAwait(false);

            Assert.That(result.Conflicts, Is.Empty);
        }

        [Test]
        public async Task AnalyzeAsync_CaseOnlyPathDifference_Collides()
        {
            Directory.CreateDirectory(Path.Combine(_modDirectory, "modA"));
            Directory.CreateDirectory(Path.Combine(_modDirectory, "modB"));
            File.WriteAllText(Path.Combine(_modDirectory, "modA", "Contested.TXT"), "a");
            File.WriteAllText(Path.Combine(_modDirectory, "modB", "contested.txt"), "b");

            var first = CreateCopyToOverrideComponent("First", Guid.NewGuid(), "modA/Contested.TXT");
            var second = CreateCopyToOverrideComponent("Second", Guid.NewGuid(), "modB/contested.txt");

            var analyzer = new FileConflictAnalyzer();
            ConflictAnalysisResult result = await analyzer.AnalyzeAsync(new List<ModComponent> { first, second }).ConfigureAwait(false);

            Assert.That(result.Conflicts.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task AnalyzeAsync_ThreeWriters_LastComponentWins()
        {
            Directory.CreateDirectory(Path.Combine(_modDirectory, "one"));
            Directory.CreateDirectory(Path.Combine(_modDirectory, "two"));
            Directory.CreateDirectory(Path.Combine(_modDirectory, "three"));
            File.WriteAllText(Path.Combine(_modDirectory, "one", "shared.txt"), "a");
            File.WriteAllText(Path.Combine(_modDirectory, "two", "shared.txt"), "b");
            File.WriteAllText(Path.Combine(_modDirectory, "three", "shared.txt"), "c");

            Guid thirdGuid = Guid.NewGuid();
            var components = new List<ModComponent>
            {
                CreateCopyToOverrideComponent("One", Guid.NewGuid(), "one/shared.txt"),
                CreateCopyToOverrideComponent("Two", Guid.NewGuid(), "two/shared.txt"),
                CreateCopyToOverrideComponent("Three", thirdGuid, "three/shared.txt"),
            };

            var analyzer = new FileConflictAnalyzer();
            ConflictAnalysisResult result = await analyzer.AnalyzeAsync(components).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result.Conflicts.Count, Is.EqualTo(1));
                Assert.That(result.Conflicts[0].Writers.Count, Is.EqualTo(3));
                Assert.That(result.Conflicts[0].WinnerComponentGuid, Is.EqualTo(thirdGuid));
            });
        }

        [Test]
        public async Task AnalyzeAsync_DeselectedComponent_ExcludedFromAnalysis()
        {
            Directory.CreateDirectory(Path.Combine(_modDirectory, "sel"));
            Directory.CreateDirectory(Path.Combine(_modDirectory, "desel"));
            File.WriteAllText(Path.Combine(_modDirectory, "sel", "shared.txt"), "a");
            File.WriteAllText(Path.Combine(_modDirectory, "desel", "shared.txt"), "b");

            var selected = CreateCopyToOverrideComponent("Selected", Guid.NewGuid(), "sel/shared.txt");
            var deselected = CreateCopyToOverrideComponent("Deselected", Guid.NewGuid(), "desel/shared.txt");
            deselected.IsSelected = false;

            var analyzer = new FileConflictAnalyzer();
            ConflictAnalysisResult result = await analyzer.AnalyzeAsync(new List<ModComponent> { selected, deselected }).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(result.Conflicts, Is.Empty);
                Assert.That(result.AnalyzedComponentCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void AnalyzeAsync_CancelledToken_ThrowsOperationCanceledException()
        {
            File.WriteAllText(Path.Combine(_modDirectory, "only.txt"), "a");
            var component = CreateCopyToOverrideComponent("Only", Guid.NewGuid(), "only.txt");
            var analyzer = new FileConflictAnalyzer();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await analyzer.AnalyzeAsync(new List<ModComponent> { component }, cts.Token).ConfigureAwait(false));
        }

    }
}
