---
title: "refactor: extract InitializeTopMenu to MenuBuilderService"
status: shipped
pr: pending
stacks_on: "#130"
---

# refactor: extract InitializeTopMenu to MenuBuilderService

## Problem

`MainWindow.InitializeTopMenu` (~270 lines) builds File/Tools/Help/About/More menus inline. Plan 072 wired mod context menus and global flyout to `MenuBuilderService`; top-level menus remained deferred per `gui-architecture-deferred.md`.

## Solution

Move top menu construction and `UpdateMenuVisibility` editor-mode toggles into `MenuBuilderService.BuildTopMenu` / `UpdateTopMenuVisibility`. `MainWindow` passes action callbacks via `TopMenuCallbacks` and stores `TopMenuBuildResult` handles.

## Requirements

| R1 | Top menu headers and tool items match pre-extraction behavior | Headless tests |
| R2 | Editor-only visibility (Close/Save/Editor Mode toggle) preserved | Headless test |
| R3 | `MenuBuilderService` created before `InitializeTopMenu` in ctor | Code review |

## Out of scope

- Download orchestration / editor hosting extraction
- Changes to help/about URL targets or menu structure

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter MenuBuilderService`
