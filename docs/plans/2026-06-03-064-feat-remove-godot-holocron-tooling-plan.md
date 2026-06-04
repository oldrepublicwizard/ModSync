---
title: "feat: remove Godot Holocron tooling and documentation"
type: feat
status: completed
date: 2026-06-03
branch: feat/remove-godot-holocron-tooling
---

# feat: remove Godot Holocron tooling and documentation

## Summary

Fully remove the in-repo Godot Holocron experiment (`tools/`), bridge CLI tests, agent scripts tied to PR #111, and all knowledgebase/plan/ideation references to Godot or the Holocron editor plugin. External HolocronToolset links in the Avalonia app menu remain (upstream PyQt product, not this repo's Godot plugin).

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `tools/` directory absent from repo | `test ! -d tools` |
| R2 | No `godot-holocron` or `tools/godot` paths in `docs/knowledgebase/` | ripgrep clean |
| R3 | `KotorFormatBridgeCliTests` removed | Project builds; tests run |
| R4 | Agent routing docs describe wizard track only | `AGENTS.md`, `doc-hierarchy.md` |
| R5 | Holocron-specific plans and ideation removed | Files deleted |

## Implementation Units

### U1. Delete tooling and tests

**Files:** `tools/` (entire tree), `src/ModSync.Tests/KotorFormatBridgeCliTests.cs`, `src/ModSync.Tests/Fixtures/kotor/sample.mod` (if only used by bridge tests)

### U2. Delete Holocron KB and plans

**Delete:** `docs/knowledgebase/godot-holocron-editor.md`, `docs/ideation/2026-05-29-godot-holocron-editor-plugin.md`, all `docs/plans/*godot*`, all `docs/plans/*holocron*`, plan `063`

### U3. Scrub remaining docs and agent scripts

**Edit:** `AGENTS.md`, `docs/knowledgebase/README.md`, `doc-hierarchy.md`, `gui-validation-surfaces.md`, `gui-architecture-deferred.md`, `agent-action-parity.md`, `ci-test-matrix.md`, `install-lifecycle.md`, `product-overview.md`, `scripts/agents/*`, `docs/solutions/parallel-pr-merge-handoff-2026-06-03.md`, `.github/copilot-instructions.md`, `.cursorrules` if applicable

**Delete scripts:** `test_pr111_holocron_bridge.sh`, `merge_open_prs.sh`, simplify `test_current_open_pr.sh` and `verify_open_pr_ready.sh`

## Out of scope

- HolocronToolset menu entries in `MainWindow.axaml.cs` (external product URLs)
- `HoloPatcher` C# project (unrelated naming)
