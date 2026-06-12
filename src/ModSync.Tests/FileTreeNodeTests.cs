// Copyright 2021-2025 ModSync
// Licensed under the Business Source License 1.1 (BSL 1.1).
// See LICENSE.txt file in the project root for full license information.

using ModSync.Models;
using Xunit;

namespace ModSync.Tests
{
    public sealed class FileTreeNodeTests
    {
        [Fact(DisplayName = "Checking parent checks all children")]
        public void IsChecked_ParentChecked_UpdatesChildren()
        {
            FileTreeNode parent = CreateNode("root");
            FileTreeNode child = CreateNode("child", parent);
            parent.Children.Add(child);

            parent.IsChecked = true;

            Assert.True(child.IsChecked);
        }

        [Fact(DisplayName = "Partial child selection marks parent indeterminate")]
        public void IsChecked_PartialChildren_SetsParentIndeterminate()
        {
            FileTreeNode parent = CreateNode("root");
            FileTreeNode firstChild = CreateNode("first", parent);
            FileTreeNode secondChild = CreateNode("second", parent);
            parent.Children.Add(firstChild);
            parent.Children.Add(secondChild);

            firstChild.IsChecked = true;

            Assert.False(parent.IsChecked);
            Assert.True(parent.IsIndeterminate);
        }

        [Fact(DisplayName = "All children checked marks parent checked")]
        public void IsChecked_AllChildrenChecked_SetsParentChecked()
        {
            FileTreeNode parent = CreateNode("root");
            FileTreeNode firstChild = CreateNode("first", parent);
            FileTreeNode secondChild = CreateNode("second", parent);
            parent.Children.Add(firstChild);
            parent.Children.Add(secondChild);

            firstChild.IsChecked = true;
            secondChild.IsChecked = true;

            Assert.True(parent.IsChecked);
            Assert.False(parent.IsIndeterminate);
        }

        [Fact(DisplayName = "Unchecking parent unchecks children")]
        public void IsChecked_ParentUnchecked_ClearsChildren()
        {
            FileTreeNode parent = CreateNode("root");
            FileTreeNode child = CreateNode("child", parent);
            parent.Children.Add(child);

            parent.IsChecked = true;
            parent.IsChecked = false;

            Assert.False(child.IsChecked);
        }

        private static FileTreeNode CreateNode(string name, FileTreeNode parent = null)
        {
            return new FileTreeNode
            {
                Name = name,
                Parent = parent,
            };
        }
    }
}
