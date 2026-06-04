---
title: "refactor: remove dead MainWindow service instantiations"
type: refactor
status: completed
date: 2026-06-04
origin: docs/knowledgebase/gui-architecture-deferred.md
branch: refactor/remove-dead-service-instantiations
---

# refactor: remove dead MainWindow service instantiations

## Summary

`MainWindow` constructs `InstallationService` and `InstructionManagementService` with discarded `_ = new ...()` expressions. Both types are used only via static APIs; the instantiations are no-ops. Remove them and mark `InstructionManagementService` as a static class to match usage.

## Problem Frame

Plan 072 (PR #120) wires `MenuBuilderService`, which was another discarded construction. `InstallationService` and `InstructionManagementService` have the same smell: instance construction with no stored reference and no instance methods invoked.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | No discarded `new InstallationService()` in `MainWindow` | grep / code review |
| R2 | No discarded `new InstructionManagementService()` in `MainWindow` | grep / code review |
| R3 | `InstructionManagementService` declared `static` (all members already static) | Build succeeds |
| R4 | Existing static call sites unchanged | `dotnet build ModSync.sln` |

## Scope boundaries

### In scope

- `MainWindow.axaml.cs` constructor cleanup (two lines)
- `InstructionManagementService.cs` static class declaration

### Deferred to follow-up work

- `SettingsService` wiring (discarded construction; settings logic still inline in `MainWindow.LoadSettings`)
- `MenuBuilderService` wiring (plan 072 / PR #120)

## Implementation units

### U1. Static class + remove dead constructions

**Goal:** Eliminate misleading instance constructions without behavior change.

**Files:**
- `src/ModSync.GUI/Services/InstructionManagementService.cs`
- `src/ModSync.GUI/MainWindow.axaml.cs`

**Approach:** Change `public class InstructionManagementService` to `public static class InstructionManagementService`. Delete the two `_ = new ...()` lines from `MainWindow` constructor.

**Test scenarios:**
- Happy path: solution builds; existing tests compile.

**Test expectation:** none — no behavioral change; build is verification.

**Verification:** `dotnet build ModSync.sln -c Debug`
