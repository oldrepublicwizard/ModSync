---
title: "test: GuiPathService recent-path and apply helpers"
status: shipped
pr: pending
---

# test: GuiPathService recent-path and apply helpers

## Problem

`GuiPathService` manages mod/game path application and recent-directory persistence for path suggestion comboboxes but had no dedicated tests.

## Solution

Add `GuiPathServiceTests` covering `AddToRecentDirectories`, recent-directory save/load round-trip, and `TryApplySourcePath` / `TryApplyDestinationPath` with temp directories.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter GuiPathService`
