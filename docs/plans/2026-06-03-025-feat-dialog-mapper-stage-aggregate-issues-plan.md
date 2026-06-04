---
title: "feat: ValidationDialog aggregate stage issues for Conflicts and archives"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-024-feat-wizard-conflicts-summary-cards-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: ValidationDialog aggregate stage issues for Conflicts and archives

## Summary

Extend `ValidationPipelineDialogMapper` so Getting Started / `ValidationDialog` shows aggregate stage rows for Conflicts and ComponentValidation failures and warnings—matching Install Order and wizard summary cards.

## Problem Frame

Slice 024 added wizard `✅`/`⚠️`/`❌ Conflicts` cards and pipeline summaries. Legacy validate still only listed per-mod conflict/archive rows in `ValidationDialog`, with no top-level stage summary row like Install Order.

## Requirements

- R1. Conflicts: after per-mod rows, add aggregate issue when `!Passed` or `HasWarnings` using `stage.Summary`.
- R2. ComponentValidation: same aggregate pattern for archive stage fail/warn.
- R3. Mapper tests for conflict failure, conflict warning (+ aggregate), archive failure aggregate.
- R4. KB table/doc note for dialog aggregate parity.

## Scope Boundaries

**In scope:** `ValidationPipelineDialogMapper.cs`, tests, KB.

**Deferred:** Holocron #111; clean-pass aggregate rows (none for Install Order today).

## Success Criteria

- [x] Dialog mapper emits stage-level Conflicts and Archive Validation rows
- [x] Tests pass
