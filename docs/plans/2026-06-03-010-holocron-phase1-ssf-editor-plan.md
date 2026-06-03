---
title: Holocron Phase 1b — SSF sound-set editor
status: shipped
shipped_pr: 109
date: 2026-06-03
origin: docs/plans/2026-06-03-009-holocron-phase1-tlk-container-editors-plan.md
prerequisite: feat/holocron-phase1-tlk-container (PR #109)
---

# Holocron Phase 1b — SSF sound-set editor

## Problem

PR #109 adds TLK and archive browsers; SSF still routes to the generic JSON editor despite bridge read/write support.

## Scope

**In scope**

- `ssf_editor` Godot scene: table of `id`, `label`, `strref` with save via existing bridge `format: ssf`.
- `EditorRegistry` maps `EditorKind.SSF` to `ssf_editor.tscn`.
- `sample.ssf` fixture + CLI read/round-trip test in `KotorFormatBridgeCliTests`.
- README parity line for SSF.

**Out of scope**

- ERF extract/nested open (Phase 1+).

## Success criteria

- [x] `.ssf` uses dedicated editor in registry (not `gff_editor`).
- [x] Bridge round-trip test passes when PyKotor is available.
