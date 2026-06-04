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
- **PR:** #111 — Phase 1 archive browser, nested open/save-back, bridge tests (plans `013`–`058`)
- **Not in:** Install wizard / `ValidatePage` work (PR #110)
- **Status:** Merge-ready for Phase 1; Phase 2 editors (TPC/WAV/MDL/DLG) deferred per `docs/plans/2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md`

### Merge checklist — PR #111

1. Confirm CI green on [PR #111](https://github.com/th3w1zard1/ModSync/pull/111).
2. Merge **after or independently of** PR #110 — no shared validation wizard files.
3. Post-merge Phase 2: start from `docs/plans/2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md`.
4. Pre-merge: `./scripts/agents/test_current_open_pr.sh` (or [ci-test-matrix.md](ci-test-matrix.md#pr-targeted-local-filters-merge-ready-open-prs)).
5. Validation wizard UX lives on [PR #110](https://github.com/th3w1zard1/ModSync/pull/110) — see [gui-validation-surfaces.md](gui-validation-surfaces.md) (synced on this branch in plan `051`).
6. After first merge: [parallel-pr-merge-handoff-2026-06-03.md](../solutions/parallel-pr-merge-handoff-2026-06-03.md) (rebase remaining branch).

## Bridge tests

```bash
./scripts/agents/test_current_open_pr.sh
```

Skips when PyKotor is not importable. Fixture: `src/KOTORModSync.Tests/Fixtures/kotor/sample.mod`.

**Read payload shapes:** Flat files (e.g. `.2da`) return `format: twoda` with row data and no `resources` array. Archives (`.mod`) return `format: erf` with a `resources` listing.

## Container editor UX

- **F5** refresh listing
- **Esc** clears member filter (when filter field focused)
- **Enter** or double-click opens member (extract → nested editor → inject on save)
- **Back to archive** returns without saving nested member

## Phase 2+ (deferred)

MDL, TPC, WAV, DLG graph editors — see `docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md` and `docs/plans/2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md`.

## Related

- [gui-validation-surfaces.md](gui-validation-surfaces.md) — ModSync Avalonia validation only
- [validation-pipeline.md](validation-pipeline.md) — Core install validation pipeline
