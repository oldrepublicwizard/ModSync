---
title: "feat: Holocron probe returns editor_kind for routing parity"
type: feat
status: completed
date: 2026-06-03
origin: tools/godot-holocron/addons/kotor_holocron/resource_types.gd
prerequisite: docs/plans/2026-06-03-017-feat-holocron-safe-archive-member-ops-plan.md
---

# feat: Holocron probe returns editor_kind for routing parity

## Summary

Extend bridge `probe` JSON with `editor_kind` aligned to Godot `KotorResourceTypes` so agents, tests, and future UI can pick editors without duplicating extension tables in callers.

## Problem Frame

Dock infers editor via `kind_for_extension(probe.extension)` locally. External tools invoking the bridge only see extension/category — not the Holocron editor routing key.

## Requirements

- R1. `probe` emits `editor_kind` (`twoda`, `gff`, `tlk`, `ssf`, `erf`, `text`, `ncs`, `binary`, `unsupported`).
- R2. Mapping mirrors `EXTENSION_TO_KIND` in `resource_types.gd`.
- R3. Tests assert `editor_kind` for `sample.2da` and `sample.mod`.
- R4. README documents `editor_kind` on probe.

## Scope Boundaries

**In scope:** Bridge helper, probe payload, tests, README.

**Deferred:** Dock refactor to consume `editor_kind` from probe; ModSync PR #110.

## Implementation Units

### U1. Bridge `_editor_kind_for_extension` + probe field

**Files:** `tools/godot-holocron/bridge/kotor_format_bridge.py`

### U2. Tests and README

**Files:** `KotorFormatBridgeCliTests.cs`, `tools/godot-holocron/README.md`

## Success Criteria

- [x] Probe returns editor_kind for 2da and mod fixtures
- [x] 17+ bridge tests pass
