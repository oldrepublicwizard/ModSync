---
title: "feat: Holocron dock probe routing and add-member resref override"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-018-feat-holocron-probe-editor-kind-plan.md
prerequisite: docs/plans/2026-06-03-018-feat-holocron-probe-editor-kind-plan.md
---

# feat: Holocron dock probe routing and add-member resref override

## Summary

Consume bridge `probe.editor_kind` in the Holocron dock (single routing source) and let modders override resref/restype when adding archive members instead of forcing filename-derived names only.

## Problem Frame

Plan 018 added `editor_kind` to probe but the dock still maps extensions locally. Add-member derives names solely from the picked filename, which blocks adding `foo.2da` as resref `bar`.

## Requirements

- R1. `KotorResourceTypes.kind_from_editor_kind` maps bridge keys to `EditorKind`.
- R2. Dock `_open_path` prefers probe `editor_kind`, falls back to `kind_for_extension`.
- R3. After file pick for add, show a small dialog to edit resref/restype before `add_member`.
- R4. Empty resref/restype blocked with status message.
- R5. README notes dock routing and add override.

## Scope Boundaries

**In scope:** `resource_types.gd`, `kotor_holocron_dock.gd`, `container_editor.gd`, README.

**Deferred:** ModSync PR #110; automated Godot UI tests.

## Implementation Units

### U1. kind_from_editor_kind + dock routing

**Files:** `resource_types.gd`, `kotor_holocron_dock.gd`

### U2. Add-member resref/restype dialog

**Files:** `container_editor.gd`

### U3. README

**Files:** `tools/godot-holocron/README.md`

## Success Criteria

- [x] Dock uses probe editor_kind when present
- [x] Add member allows resref/restype override
- [x] README updated
