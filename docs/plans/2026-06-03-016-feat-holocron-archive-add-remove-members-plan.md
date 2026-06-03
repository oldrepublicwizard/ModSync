---
title: "feat: Holocron add and remove archive members"
type: feat
status: completed
date: 2026-06-03
origin: tools/godot-holocron/README.md
prerequisite: docs/plans/2026-06-03-015-feat-holocron-archive-refresh-after-saveback-plan.md
---

# feat: Holocron add and remove archive members

## Summary

Close Phase 2 archive membership gaps: bridge `remove` command, reuse `inject` for adding members from disk, container editor toolbar actions, and CLI tests on temp archive copies.

## Problem Frame

Extract/inject/save-back (plans 013–015) supports editing existing members. Modders still cannot add a new file into an archive or delete a stale member without external tools.

## Requirements

- R1. Bridge `remove <archive> --resref X --restype EXT` deletes member and rewrites archive.
- R2. Adding a member uses existing `inject` semantics (`set_data`); expose `FormatBridge.add_member` as alias for discoverability.
- R3. Container editor toolbar: **Add member** (file picker → resref/restype from filename) and **Remove** (selected row).
- R4. After add/remove, refresh listing in-place via `read` without leaving the editor.
- R5. Tests mutate **temp copies** of `sample.mod` only.
- R6. README Phase 2 row updated to Done for add/remove.

## Scope Boundaries

**In scope:** remove CLI, GDScript wrappers, container UI, tests, README.

**Deferred:** ResRef validation dialog, bulk import, RIM-specific UX polish, ModSync GUI (#110).

## Key Technical Decisions

- PyKotor `ERF.remove(resref, restype)` + `_write_container` mirrors inject path.
- Add member derives resref/restype from chosen filename (basename + extension).
- No confirmation dialog for remove in this slice (archive path is explicit; tests cover remove).

## Implementation Units

### U1. Bridge remove command

**Files:** `tools/godot-holocron/bridge/kotor_format_bridge.py`

**Test scenarios:** Temp copy of `sample.mod`; remove `test2da`; `read` returns zero resources.

### U2. Godot bridge + container toolbar

**Files:** `format_bridge.gd`, `container_editor.gd`, `container_editor.tscn`

**Test scenarios:** Manual add/remove on `sample.mod` copy in Godot.

### U3. CLI tests and README

**Files:** `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`, `tools/godot-holocron/README.md`

**Test scenarios:** `Remove_DeletesMemberFromArchiveCopy`; `Add_AppendsMemberViaInject` (new resref, count increases).

## Success Criteria

- [x] `remove` CLI works on temp archive
- [x] Container add/remove refreshes tree
- [x] Bridge tests pass (14)
- [x] README documents add/remove
