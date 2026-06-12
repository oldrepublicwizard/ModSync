---
title: "test: harden SettingsService picker headless tests"
type: feat
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-077-test-settings-service-picker-headless-plan.md
branch: test/settings-service-picker-hardening
---

# test: harden SettingsService picker headless tests

## Summary

Plan 077 landed headless `SettingsService` picker tests (PR #125). Design review found harness gaps: default `PickerType` on all pickers, no `KotorDirectory` sync coverage, and assertions only on `GetCurrentPath()` rather than templated `PathInput`. Harden tests without changing production code.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | Harness pickers use production `PickerType` values (mod vs kotor) | Code review + tests pass |
| R2 | `SyncDirectoryPickers(KotorDirectory, path)` updates main + Step1 kotor pickers | New test |
| R3 | Path assertions include `PathInput.Text` after template apply | Updated assertions helper |
| R4 | KB marks plan `075` as PR #123 (merged) | `gui-architecture-deferred.md` |

## Scope boundaries

### In scope

- `src/ModSync.Tests/SettingsServiceTests.cs`
- `docs/knowledgebase/gui-architecture-deferred.md` (status line only)

### Out of scope

- `MainWindow` / `GettingStartedTab` visual-tree integration tests
- `SettingsDialog` / wizard picker surfaces

## Implementation units

### U1. Harness realism

**Files:** `src/ModSync.Tests/SettingsServiceTests.cs`

Set `PickerType` on harness controls to match XAML: mod pickers `ModDirectory`, kotor pickers `KotorDirectory`.

Add `AssertPickerShowsPath(DirectoryPickerControl picker, string expectedPath)` that checks `GetCurrentPath()` and `FindControl<TextBox>("PathInput").Text`.

### U2. Kotor sync test

**Files:** `src/ModSync.Tests/SettingsServiceTests.cs`

**Scenario:** `SyncDirectoryPickers(DirectoryPickerType.KotorDirectory, path)` sets `KotorDirectoryPicker` and `Step1KotorDirectoryPicker`.

**Verification:** `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter SettingsService`

### U3. KB status fix

**Files:** `docs/knowledgebase/gui-architecture-deferred.md`

Change plan `075` reference from `PR pending` to `PR #123`.
