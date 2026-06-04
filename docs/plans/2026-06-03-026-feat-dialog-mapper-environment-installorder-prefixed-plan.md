---
title: "feat: dialog mapper Environment and Install Order prefixed rows"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-025-feat-dialog-mapper-stage-aggregate-issues-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: dialog mapper Environment and Install Order prefixed rows

## Summary

Parse `ERROR:`/`WARNING:` stage messages for Environment and Install Order in `ValidationPipelineDialogMapper`, matching wizard and Conflicts/archive behavior. Avoid duplicate Environment rows when the pipeline already emits `ERROR: {summary}`.

## Problem Frame

Install Order failures add `ERROR:` lines to `Messages` but the dialog mapper only showed the aggregate row. Environment failures emit the same prefixed line the wizard parses, yet the dialog always showed a second identical `Environment` row.

## Requirements

- R1. Install Order: per-mod prefixed rows plus existing aggregate fail/warn rows.
- R2. Environment: per-mod prefixed rows on failure; aggregate `Environment` row only when no prefixed row was added.
- R3. Mapper tests for both stages.
- R4. KB note.

## Success Criteria

- [x] Dialog and wizard parity for Environment/Install Order prefixed lines
- [x] Tests pass
