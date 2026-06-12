---
title: "test: FileTreeNode checkbox hierarchy"
status: shipped
pr: pending
---

# test: FileTreeNode checkbox hierarchy

## Problem

`FileTreeNode` drives tri-state selection in the mod files browser but had no dedicated tests for parent/child checkbox propagation.

## Solution

Add `FileTreeNodeTests` for parent check, partial selection indeterminate state, and uncheck propagation.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter FileTreeNode
