---
title: "Holocron Phase 2 — deferred specialized editors"
type: feat
status: deferred
date: 2026-06-03
branch: feat/holocron-erf-nested-open
prerequisite: docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md
---

# Holocron Phase 2 — deferred specialized editors

Phase 1 (PR #111) delivers archive browse, nested open/save-back, and bridge coverage through plan `047`.

## Phase 2 targets (not started here)

| Editor | Extensions | Notes |
|--------|------------|--------|
| TPC | `tpc` | Texture preview; may extend binary read-only path first |
| WAV | `wav` | Audio preview / metadata |
| MDL | `mdl`, `mdx` | 3D viewport or external tool handoff |
| DLG | `dlg` | Node graph editor (largest lift) |

## Recommended first slice after merge

1. **TPC read-only preview** in Godot using bridge `read` binary/base64 payload.
2. Bridge tests for round-trip only when PyKotor write support exists.

Track in `docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md` and [godot-holocron-editor.md](../knowledgebase/godot-holocron-editor.md).
