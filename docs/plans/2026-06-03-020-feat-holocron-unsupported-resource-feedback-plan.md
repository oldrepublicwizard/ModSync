---
title: "feat: Holocron clear feedback for unsupported resource types"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-019-feat-holocron-dock-probe-routing-add-resref-plan.md
prerequisite: docs/plans/2026-06-03-019-feat-holocron-dock-probe-routing-add-resref-plan.md
---

# feat: Holocron clear feedback for unsupported resource types

## Summary

When `probe.editor_kind` is `unsupported`, stop before bridge `read` and show an explicit dock status message. Block nested archive open for unsupported member types. Add CLI test for `.res` probe parity.

## Problem Frame

Probe can succeed with `editor_kind: unsupported` (e.g. `.res`), but the dock still called `read` and fell through to a generic text editor fallback, which is confusing and may fail opaquely.

## Requirements

- R1. Dock returns early when resolved kind is `UNSUPPORTED`, citing extension and category from probe.
- R2. Container double-click checks member restype; skips extract when unsupported.
- R3. Test `Probe_UnsupportedResource_ReturnsUnsupportedEditorKind` on temp `.res` file.
- R4. README notes unsupported-type behavior.

## Scope Boundaries

**In scope:** dock, container_editor, one test, README.

**Deferred:** Hex/binary viewer; ModSync PR #110.

## Success Criteria

- [x] Dock does not call read for unsupported kinds
- [x] Container blocks unsupported nested open
- [x] Test passes
- [x] README updated
