---
title: "test: ComponentEditorService unsaved-change detection"
status: shipped
pr: pending
---

# test: ComponentEditorService unsaved-change detection

## Problem

`ComponentEditorService.HasUnsavedChanges` guards raw-editor save prompts but had no dedicated tests.

## Solution

Add `ComponentEditorServiceTests` for null/blank input handling and TOML/JSON serialization parity checks.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ComponentEditorService`
