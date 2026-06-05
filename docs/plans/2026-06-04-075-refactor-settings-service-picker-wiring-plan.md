---
title: "refactor: wire MainWindow directory pickers to SettingsService"
type: refactor
status: completed
date: 2026-06-04
origin: docs/knowledgebase/gui-architecture-deferred.md
branch: refactor/settings-service-picker-wiring
---

# refactor: wire MainWindow directory pickers to SettingsService

## Summary

`SettingsService` is constructed but discarded in `MainWindow`; directory picker sync logic is duplicated inline. Store the service and delegate picker initialization, settings refresh, and cross-picker sync to `SettingsService` static/instance APIs.

## Problem Frame

After plans 072/074 address `MenuBuilderService` and other dead constructions, `SettingsService` remains the next discarded GUI service. `UpdateDirectoryPickersFromSettings`, `InitializeDirectoryPickers`, and `SyncDirectoryPickers` duplicate `SettingsService` helpers with equivalent `SetCurrentPathFromSettings` calls.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `MainWindow` stores `_settingsService` from construction | No `_ = new SettingsService` |
| R2 | `UpdateDirectoryPickersFromSettings` delegates to `SettingsService.UpdateDirectoryPickersFromSettings` | Code review |
| R3 | `InitializeDirectoryPickers` delegates to `SettingsService.InitializeDirectoryPickers` | Code review |
| R4 | `SyncDirectoryPickers` delegates to `SettingsService.SyncDirectoryPickers` and still calls `UpdateStepProgress` | Code review |
| R5 | Step1 pickers resolve via `GettingStartedTabControl` through a shared `FindDirectoryPickerControl` helper | Build + behavior parity |
| R6 | KB notes SettingsService picker wiring progress | `gui-architecture-deferred.md` |

## Key Technical Decisions

- **KTD1:** Use a `FindDirectoryPickerControl(string name)` helper — route `Step1*` names to `GettingStartedTabControl`, others to `MainWindow.FindControl`.
- **KTD2:** Keep `LoadSettings` / `SaveSettings` inline for this slice — they contain theme/HoloPatcher UI orchestration beyond `SettingsService.LoadSettings` / `SaveSettings`.
- **KTD3:** Preserve `SyncDirectoryPickers` as public on `MainWindow` (called from `SettingsDialog`) — delegate internally only.

## Scope boundaries

### In scope

- `SettingsService` field + directory picker delegation
- Remove duplicate `UpdateDirectoryPickerWithPath` if unused after delegation
- KB progress note

### Deferred

- Full `LoadSettings` / `SaveSettings` delegation
- `InitializeTopMenu` extraction

## Implementation units

### U1. Wire SettingsService picker APIs

**Files:**
- `src/ModSync.GUI/MainWindow.axaml.cs`

**Approach:** Add `_settingsService` field, `FindDirectoryPickerControl`, delegate three picker methods, delete redundant private helpers.

**Test expectation:** none — refactor with identical underlying picker API calls.

**Verification:** `dotnet build ModSync.sln -c Debug`

### U2. KB closure

**Files:** `docs/knowledgebase/gui-architecture-deferred.md`

**Verification:** Deferred-work table mentions SettingsService picker wiring.
