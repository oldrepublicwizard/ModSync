---
title: "feat: Holocron ERF extract and nested resource open"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md
---

# feat: Holocron ERF extract and nested resource open

## Summary

Add Phase 1+ support to open resources inside ERF/RIM/MOD/SAV archives: a bridge `extract` command, Godot `FormatBridge.extract_member`, double-click in `container_editor`, and automated CLI tests with a committed `sample.mod` fixture.

## Problem Frame

Phase 1 (#109) lists archive contents read-only. Modders must extract files manually before editing. Holocron parity expects open-from-archive for nested UTC/2DA/etc.

## Requirements

- R1. Bridge `extract <archive> --resref X --restype Y --output <path>` writes member bytes (ERF and RIM).
- R2. `FormatBridge.extract_member` wraps the CLI from GDScript.
- R3. `container_editor` double-click (or activate) extracts to cache and requests the dock open the temp file in the correct editor.
- R4. `kotor_holocron_dock` wires container `member_open_requested` to existing `_open_path`.
- R5. Fixture `src/KOTORModSync.Tests/Fixtures/kotor/sample.mod` with embedded `test2da` (from `sample.2da`).
- R6. `KotorFormatBridgeCliTests`: extract succeeds; extracted file round-trips via `read`.
- R7. Update `tools/godot-holocron/README.md` parity table (nested open = Phase 1+ partial).

## Scope Boundaries

**In scope:** Extract-to-temp + open; tests; README.

**Deferred:** Inject/write back into archive; MDL viewport; DLG graph.

## Assumptions

- PyKotor `ERF.get_data(resref, ResourceType)` works for MOD/ERF/SAV and `read_rim` containers behave the same.
- PR #110 (wizard archive validation) remains a separate PR; this slice branches from `master`.

## Key Technical Decisions

- Extract to `OS.get_cache_dir()` with deterministic names (`{archive}_{resref}.{ext}`) — no in-archive editing.
- Reuse dock `_open_path` for nested opens (probe/read/editor registry unchanged).
- Signal `member_open_requested(path)` from container editor to avoid tight coupling.

## Implementation Units

### U1. Bridge extract command

**Files:** `tools/godot-holocron/bridge/kotor_format_bridge.py`

**Test scenarios:**
- `extract` on `sample.mod` for `test2da`/`2da` writes file with non-zero size.
- `read` on extracted path returns `format: twoda` with same row count as direct `sample.2da` read.

### U2. Fixture sample.mod

**Files:** `src/KOTORModSync.Tests/Fixtures/kotor/sample.mod` (binary, generated once from `sample.2da`)

### U3. Godot extract + nested open UX

**Files:**
- `tools/godot-holocron/addons/kotor_holocron/format_bridge.gd`
- `tools/godot-holocron/addons/kotor_holocron/editors/container_editor.gd`
- `tools/godot-holocron/addons/kotor_holocron/editors/container_editor.tscn` (connect `item_activated`)
- `tools/godot-holocron/addons/kotor_holocron/ui/kotor_holocron_dock.gd`

**Verification:** Manual in Godot — open `.mod`, double-click `test2da` row, TwoDA editor loads.

### U4. CLI tests and README

**Files:**
- `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`
- `tools/godot-holocron/README.md`

## Success Criteria

- [x] Bridge extract works for committed `sample.mod`
- [x] Godot container double-click opens nested editor via dock
- [x] Automated extract test passes when PyKotor present
- [x] README documents nested open
