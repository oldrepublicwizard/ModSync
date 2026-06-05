---
title: "test: headless SettingsService directory picker tests"
type: feat
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-075-refactor-settings-service-picker-wiring-plan.md
branch: test/settings-service-picker-headless
---

# test: headless SettingsService directory picker tests

## Summary

Plan 075 wires `MainWindow` directory pickers to `SettingsService` (PR #123). Add headless Avalonia tests for the static picker APIs so regressions are caught without full GUI automation.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `UpdateDirectoryPickersFromSettings` applies source and destination paths to registered pickers | NUnit test |
| R2 | `SyncDirectoryPickers` updates main and Step1 pickers for mod directory type | NUnit test |
| R3 | `InitializeDirectoryPickers` seeds pickers from `MainConfig` paths | NUnit test |
| R4 | KB lists test filter command | `gui-architecture-deferred.md` |

## Scope boundaries

### In scope

- `src/ModSync.Tests/SettingsServiceTests.cs`
- KB test pointer

### Out of scope

- `MainWindow` changes (PR #123)
- `LoadSettings` / `SaveSettings` instance methods

## Implementation units

### U1. Headless picker tests

**Files:** `src/ModSync.Tests/SettingsServiceTests.cs`

**Approach:** Follow `ControlsHeadlessTests` / `HeadlessTestApp` pattern. Host `DirectoryPickerControl` instances in a `Window`, register via name→control map, use temp directories as paths.

**Test scenarios:**
- Editor settings with both paths updates four pickers when all registered.
- `SyncDirectoryPickers(ModDirectory, path)` sets main + Step1 mod pickers.
- `InitializeDirectoryPickers` reads `MainConfig.sourcePath` / `destinationPath`.

**Verification:** `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter SettingsService`

### U2. KB test command

**Files:** `docs/knowledgebase/gui-architecture-deferred.md`

**Test expectation:** none beyond listing filter in KB.
