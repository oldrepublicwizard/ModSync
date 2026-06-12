---
title: "refactor: wire MainWindow to MenuBuilderService"
type: refactor
status: completed
date: 2026-06-04
origin: docs/knowledgebase/gui-architecture-deferred.md
branch: refactor/mainwindow-menu-builder-wiring
---

# refactor: wire MainWindow to MenuBuilderService

## Summary

`MenuBuilderService` exists but is discarded at construction; `MainWindow` duplicates its context-menu and global-actions flyout logic inline. Wire the service as the single builder, aligning it to current MainWindow behavior before delegation.

## Problem Frame

`MainWindow.axaml.cs` instantiates `MenuBuilderService` with `_ = new MenuBuilderService(...)` and never stores it. `BuildContextMenuForComponent` (~140 lines) and `BuildMenuFlyoutItems` (~100 lines) duplicate `src/ModSync.GUI/Services/MenuBuilderService.cs`. This violates the deferred GUI extraction guidance in `docs/knowledgebase/gui-architecture-deferred.md` and leaves dead service code that can drift.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `MainWindow` stores and uses a `MenuBuilderService` instance | Code review; no discarded `_ = new MenuBuilderService` |
| R2 | `BuildContextMenuForComponent` delegates to the service with equivalent behavior | Headless test asserts menu item headers; manual parity on selection/move/delete |
| R3 | Global actions flyout delegates to the service with equivalent behavior | Headless test asserts flyout headers in editor and non-editor modes |
| R4 | Move-up/down still triggers post-move list refresh (`ProcessComponentsAsync`) | Test or callback wiring inspection |
| R5 | Selection toggle still runs `UpdateModCounts` and checkbox dependency handlers | Callback wiring inspection |
| R6 | KB deferred-work note updated for menus extraction progress | `gui-architecture-deferred.md` |

## Key Technical Decisions

- **KTD1: MainWindow behavior is source of truth** — Before delegation, extend `MenuBuilderService` to match production menus. Do not ship new flyout items (e.g., sort-by-name) that MainWindow does not expose today.
- **KTD2: Post-move refresh via callback** — `MenuBuilderService` calls `MoveModRelative` directly today; MainWindow wraps moves with `ProcessComponentsAsync`. Pass an `Action<ModComponent, int>` move callback from `MainWindow` instead of calling `MoveModRelative` inline.
- **KTD3: Selection side-effects via callback** — Pass a lambda that invokes `UpdateModCounts` plus `ComponentCheckboxChecked` / `ComponentCheckboxUnchecked` so dependency propagation is unchanged.
- **KTD4: Flyout bulk actions as injected delegates** — Add optional `Func<Task>` parameters for Generate Instructions, Lock Install Order, and Remove All Dependencies so the service stays UI-framework-agnostic at the orchestration boundary.
- **KTD5: Scope stops at context menu + global flyout** — `InitializeTopMenu` File/Tools menus remain in `MainWindow`; `BuildGlobalActionsContextMenu` is unused and not wired in this slice.

## Scope Boundaries

### In scope

- `MenuBuilderService` alignment + `MainWindow` delegation for mod context menu and global actions flyout
- Headless tests for menu item presence
- KB progress note

### Deferred to Follow-Up Work

- `InitializeTopMenu` extraction
- `BuildGlobalActionsContextMenu` wiring (no call sites today)
- File drag-drop overlay extraction from `MainWindow`
- Sort-by-name/category/tier flyout items (present only in unused service code)

## Implementation Units

### U1. Align MenuBuilderService to MainWindow behavior

**Goal:** Service builds menus matching current production behavior.

**Requirements:** R2, R3, R4, R5

**Files:**
- `src/ModSync.GUI/Services/MenuBuilderService.cs`

**Approach:**
- Replace direct `MoveModRelative` calls with `Action<ModComponent, int> onMoveRelative` parameter on `BuildContextMenuForComponent`.
- Add flyout parameters: `Func<Task> onGenerateInstructions`, `onLockInstallOrder`, `onRemoveAllDependencies`.
- Insert the three bulk-action items between Validate All and the editor separator (matching MainWindow order).
- Remove sort-by flyout items from `AddEditorModeFlyoutItems` (never shipped in MainWindow).
- Match delete-confirmation dialog tooltips to MainWindow.
- Use `Environment.NewLine` in validation error messages to match MainWindow.

**Test scenarios:**
- Happy path (editor mode): context menu includes Select, Move Up/Down, Delete, Duplicate, Edit, Validate headers.
- Happy path (non-editor): context menu has only Select item.
- Happy path (editor flyout): includes Generate Instructions, Lock Install Order, Remove All Dependencies, Add New Mod, Save Config.
- Edge case: null component returns empty context menu.

**Verification:** Service compiles; no behavioral additions beyond MainWindow parity.

### U2. Wire MainWindow to MenuBuilderService

**Goal:** Remove duplicated menu-building code from `MainWindow`.

**Requirements:** R1, R2, R3, R4, R5

**Dependencies:** U1

**Files:**
- `src/ModSync.GUI/MainWindow.axaml.cs`

**Approach:**
- Add `private MenuBuilderService _menuBuilderService;` field.
- Replace discarded construction with field assignment.
- Replace `BuildContextMenuForComponent` body with delegation + callbacks (`MoveComponentListItem`, selection handlers, `RemoveComponentButton_Click`, etc.).
- Replace `BuildMenuFlyoutItems` body with `_menuBuilderService.BuildGlobalActionsFlyout(...)` and delete the private method if fully delegated.
- Keep `BuildGlobalActionsMenu` as thin dispatcher to the service.

**Test scenarios:**
- Integration: `ModListItem` context menu still obtained via `mainWindow.BuildContextMenuForComponent` (public API unchanged).

**Verification:** `dotnet build ModSync.sln -c Debug` succeeds; duplicated inline menu blocks removed.

### U3. Headless menu builder tests

**Goal:** Guard against menu regression without full UI automation.

**Requirements:** R2, R3

**Dependencies:** U1, U2

**Files:**
- `src/ModSync.Tests/MenuBuilderServiceTests.cs`

**Approach:** Follow `ControlsHeadlessTests` / `HeadlessTestApp` pattern. Instantiate service with stub `ModManagementService` and headless window; build menus with no-op callbacks; assert `MenuItem` header strings.

**Test scenarios:**
- Editor context menu item count and key headers for a sample `ModComponent`.
- Non-editor context menu has single select item.
- Editor flyout includes bulk-action headers in correct relative order.

**Verification:** `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter MenuBuilderService`

### U4. KB closure

**Goal:** Record menus extraction progress.

**Requirements:** R6

**Dependencies:** U2

**Files:**
- `docs/knowledgebase/gui-architecture-deferred.md`

**Approach:** Add a completed row under MainWindow god object for context menu + global flyout delegation; note top menu still deferred.

**Test expectation:** none — documentation only.

**Verification:** KB reflects shipped state.
