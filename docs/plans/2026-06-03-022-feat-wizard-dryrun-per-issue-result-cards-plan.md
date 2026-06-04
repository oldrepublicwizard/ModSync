---
title: "feat: Wizard dry-run per-issue result cards on ValidatePage"
type: feat
status: completed
date: 2026-06-03
origin: docs/knowledgebase/gui-validation-surfaces.md
prerequisite: docs/plans/2026-06-03-021-feat-wizard-install-order-stage-message-cards-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: Wizard dry-run per-issue result cards on ValidatePage

## Summary

Show individual ValidatePage result cards for the top dry-run errors and warnings (with solution hints from `ValidationPipelineDialogMapper`), plus the existing aggregate Instruction Execution summary.

## Problem Frame

`gui-validation-surfaces.md` documents that dry-run issues appear only as a single summary card with top-N snippets in the message. The dialog mapper already surfaces every issue with solutions; the wizard should expose the same detail as cards.

## Requirements

- R1. Up to 5 error issues become `❌ {Mod} ({Category})` cards with message + solution hint.
- R2. When only warnings, up to 5 warning cards plus summary.
- R3. Aggregate `Instruction Execution` summary card retained; note overflow count when more than 5 issues.
- R4. Regression tests for error and warning dry-run cards.

## Scope Boundaries

**In scope:** `WizardValidationStagePresenter`, tests, KB update.

**Deferred:** Holocron PR #111; clickable issue detail dialog from cards.

## Success Criteria

- [x] Per-issue dry-run cards on ValidatePage
- [x] Tests pass
- [x] KB updated
