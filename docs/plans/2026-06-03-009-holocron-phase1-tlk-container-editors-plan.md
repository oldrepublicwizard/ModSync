---
title: Holocron Phase 1 — TLK editor and archive browser
status: shipped
shipped_pr: 109
date: 2026-06-03
origin: docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md
---

# Holocron Phase 1 — TLK editor and archive browser

## Problem

Phase 0 merged in #92: bridge + 2DA/GFF/text editors. TLK, SSF, and ERF/RIM still open in the generic GFF JSON editor, which is poor UX and hides archive contents.

## Scope

**In scope**

- Dedicated `tlk_editor` (string table: index, text, sound resref) with save via bridge.
- Dedicated `container_editor` for ERF/RIM/MOD/SAV (resource list; read-only in this slice).
- Bridge `write` support for `format: tlk` and `format: ssf` (PyKotor `TLK.from_json` / `SSF.from_json`).
- Wire `EditorRegistry` to new scenes; dock opens `dialog.tlk` when an installation row is activated (if present).
- CLI tests: synthetic TLK round-trip; `supported-types` includes `tlk`.
- Update `tools/godot-holocron/README.md` parity table.

**Out of scope**

- ERF extract/inject, nested open-from-archive.
- SSF dedicated Godot UI (still JSON via gff_editor until a follow-up).
- DLG graph (Phase 2).

## Implementation units

### Unit 1 — Bridge write (TLK/SSF)

**Files:** `tools/godot-holocron/bridge/kotor_format_bridge.py`

- Import `TLK`, `write_tlk`, `SSF`, `write_ssf`.
- `cmd_write`: handle `fmt == "tlk"` and `fmt == "ssf"` with `data` dict.

**Tests:** `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`

- `Write_RoundTripsSyntheticTlk` — create minimal TLK via read after write from JSON payload built in test (or committed `Fixtures/kotor/sample.tlk`).

### Unit 2 — Godot editors

**Files:**

- `tools/godot-holocron/addons/kotor_holocron/editors/tlk_editor.gd`
- `tools/godot-holocron/addons/kotor_holocron/editors/tlk_editor.tscn`
- `tools/godot-holocron/addons/kotor_holocron/editors/container_editor.gd`
- `tools/godot-holocron/addons/kotor_holocron/editors/container_editor.tscn`
- `tools/godot-holocron/addons/kotor_holocron/editor_registry.gd`

Pattern: follow `twoda_editor.gd` / `KotorResourceEditorBase`.

### Unit 3 — Dock installation shortcut

**Files:** `tools/godot-holocron/addons/kotor_holocron/ui/kotor_holocron_dock.gd`, `kotor_holocron_dock.tscn`

- `item_activated` on `InstallList` → prefer `dialog.tlk` under install path.

### Unit 4 — Docs

**Files:** `tools/godot-holocron/README.md`

## Test scenarios

| Scenario | Expected |
|----------|----------|
| Bridge write TLK JSON | `ok: true`, re-read same string count |
| Probe/read committed `sample.tlk` | format `tlk`, strings array non-empty |
| Registry TLK kind | Instantiates `tlk_editor`, not gff_editor |
| Registry ERF kind | Instantiates `container_editor` |

## Risks

- PyKotor absent in CI → tests skip (existing pattern).
- Godot scenes not CI-tested → bridge + registry paths covered in .NET tests only.

## Success criteria

- [x] TLK opens in table editor and saves round-trip via bridge (automated when PyKotor present).
- [x] ERF/RIM show resource list in container editor.
- [x] README reflects Phase 1 partial completion.
