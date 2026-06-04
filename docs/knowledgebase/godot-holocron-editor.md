# Godot Holocron editor plugin

`[REPO]` KOTOR 1/2 resource editing inside Godot 4 via **PyKotor** bridge — separate from the ModSync install wizard ([gui-architecture-deferred.md](gui-architecture-deferred.md)).

## Location

| Path | Role |
|------|------|
| `tools/godot-holocron/project.godot` | Open in Godot 4.3+ |
| `tools/godot-holocron/bridge/kotor_format_bridge.py` | CLI: probe, read, write, extract, inject, remove, installations |
| `tools/godot-holocron/addons/kotor_holocron/` | EditorPlugin, dock, per-format editors |
| `tools/godot-holocron/README.md` | Quick start, parity table |

## Active delivery

- **Branch:** `feat/holocron-erf-nested-open`
- **PR:** #111 — Phase 1 archive browser, nested open/save-back, bridge tests (plans `013`–`047`)
- **Not in:** Install wizard / `ValidatePage` work (PR #110)

## Bridge tests

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```

Skips when PyKotor is not importable. Fixture: `src/KOTORModSync.Tests/Fixtures/kotor/sample.mod`.

## Container editor UX

- **F5** refresh listing
- **Esc** clears member filter (when filter field focused)
- **Enter** or double-click opens member (extract → nested editor → inject on save)
- **Back to archive** returns without saving nested member

## Phase 2+ (deferred)

MDL, TPC, WAV, DLG graph editors — see `docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md`.

## Related

- [gui-validation-surfaces.md](gui-validation-surfaces.md) — ModSync Avalonia validation only
- [validation-pipeline.md](validation-pipeline.md) — Core install validation pipeline
