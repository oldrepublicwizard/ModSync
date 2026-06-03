---
title: "feat: Holocron archive inject and save-back from nested editors"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-013-feat-holocron-erf-extract-nested-open-plan.md
prerequisite: docs/plans/2026-06-03-013-feat-holocron-erf-extract-nested-open-plan.md
---

# feat: Holocron archive inject and save-back from nested editors

## Summary

Complete the archive editing loop started in plan 013: replace a member inside ERF/RIM/MOD/SAV from disk via bridge `inject`, and when a nested member editor saves, write changes back into the parent archive automatically.

## Problem Frame

Extract + open (PR #111) lets modders edit nested files in cache, but saves only touch the temp file — not the archive. Holocron parity expects in-place member updates.

## Requirements

- R1. Bridge `inject <archive> --resref X --restype Y --source <file>` replaces member bytes and rewrites the archive.
- R2. `FormatBridge.inject_member` GDScript wrapper.
- R3. `member_open_requested` carries `{archive, resref, restype}` context; dock stores it while nested editor is open.
- R4. On nested editor `saved` signal, call inject when context is set; show dock status success/failure.
- R5. Tests use a **temp copy** of `sample.mod` (never mutate committed fixture).
- R6. `Inject_RoundTripsAfterExtract` CLI test when PyKotor present.
- R7. README: inject + save-back documented; Phase 2 inject row updated.

## Scope Boundaries

**In scope:** inject command, save-back wiring, tests, README.

**Deferred:** Add new members to archives; delete members; UI to return to container without re-open.

## Key Technical Decisions

- Use PyKotor `set_data` + `write_erf` / rim writer on same path (in-place archive update).
- Inject only when open path originated from container (context dictionary non-empty).
- Tests copy `sample.mod` to temp before inject mutation.

## Implementation Units

### U1. Bridge inject command

**Files:** `tools/godot-holocron/bridge/kotor_format_bridge.py`

**Test scenarios:** Copy fixture to temp; inject `sample.2da` bytes for `test2da`; re-extract and row count matches.

### U2. Godot save-back

**Files:** `format_bridge.gd`, `container_editor.gd`, `kotor_holocron_dock.gd`

### U3. Tests and README

**Files:** `KotorFormatBridgeCliTests.cs`, `tools/godot-holocron/README.md`

## Success Criteria

- [x] `inject` CLI works on temp archive copy
- [x] Nested editor Save updates parent archive when opened from container
- [x] Tests pass; README updated
