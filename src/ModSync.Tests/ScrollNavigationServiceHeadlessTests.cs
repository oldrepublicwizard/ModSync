// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using ModSync.Services;
using Xunit;

namespace ModSync.Tests
{
    [Collection(HeadlessTestApp.CollectionName)]
    public sealed class ScrollNavigationServiceHeadlessTests
    {
        [AvaloniaFact(DisplayName = "FindControlRecursive returns null for null parent")]
        public void FindControlRecursive_NullParent_ReturnsNull()
        {
            Button result = ScrollNavigationService.FindControlRecursive<Button>(null, _ => true);
            Assert.Null(result);
        }

        [AvaloniaFact(DisplayName = "FindControlRecursive finds nested control matching predicate")]
        public void FindControlRecursive_NestedControl_FindsMatch()
        {
            var root = new StackPanel();
            var nested = new Border();
            var target = new Button { Name = "JumpTarget" };
            nested.Child = target;
            root.Children.Add(nested);

            Button found = ScrollNavigationService.FindControlRecursive<Button>(
                root,
                button => button.Name == "JumpTarget");

            Assert.Same(target, found);
        }

        [AvaloniaFact(DisplayName = "FindControlRecursive returns null when predicate does not match")]
        public void FindControlRecursive_NoMatch_ReturnsNull()
        {
            var root = new StackPanel();
            root.Children.Add(new Button { Name = "Other" });

            Button found = ScrollNavigationService.FindControlRecursive<Button>(
                root,
                button => button.Name == "Missing");

            Assert.Null(found);
        }

        [AvaloniaFact(DisplayName = "FindScrollViewer returns nested scroll viewer")]
        public void FindScrollViewer_NestedViewer_ReturnsViewer()
        {
            var root = new StackPanel();
            var scrollViewer = new ScrollViewer();
            root.Children.Add(scrollViewer);

            ScrollViewer found = ScrollNavigationService.FindScrollViewer(root);

            Assert.Same(scrollViewer, found);
        }
    }
}
