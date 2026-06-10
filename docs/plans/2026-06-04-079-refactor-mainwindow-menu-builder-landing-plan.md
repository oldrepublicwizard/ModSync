---
title: "refactor: land MainWindow MenuBuilderService wiring"
type: refactor
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-072-refactor-mainwindow-menu-builder-wiring-plan.md
branch: refactor/mainwindow-menu-builder-landing
---

# refactor: land MainWindow MenuBuilderService wiring

## Summary

Plan `072` / PR #120 wires `MainWindow` mod context menus and global actions flyout to `MenuBuilderService`. Master still discards the service (`_ = new MenuBuilderService(...)`) and duplicates ~240 lines of menu logic. Land the wiring on current master (post #123 SettingsService merge).

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `MainWindow` stores and uses `_menuBuilderService` | No discarded construction |
| R2 | Context menu delegates to service with parity callbacks | `MenuBuilderService` filter tests |
| R3 | Global flyout delegates to service | Flyout header tests |
| R4 | KB marks menu extraction done; `InitializeTopMenu` still deferred | `gui-architecture-deferred.md` |

## Scope boundaries

### In scope

- Cherry-pick / re-apply PR #120 changes on current `origin/master`
- Resolve merge conflicts if any
- `MenuBuilderServiceTests.cs`

### Out of scope

- `InitializeTopMenu` extraction
- New flyout features not in MainWindow today

## Implementation units

### U1. Apply plan 072 implementation on master

**Files:** `MenuBuilderService.cs`, `MainWindow.axaml.cs`, `MenuBuilderServiceTests.cs`, plan `072`, KB

**Verification:** `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter MenuBuilderService`
