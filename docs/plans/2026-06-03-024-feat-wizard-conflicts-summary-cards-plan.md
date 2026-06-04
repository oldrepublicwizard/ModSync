---
title: "feat: Wizard Conflicts aggregate summary cards"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-023-refactor-wizard-prefixed-stage-message-helper-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: Wizard Conflicts aggregate summary cards

## Summary

Add ValidatePage aggregate result cards for the Conflicts pipeline stage (pass / warning / fail) and set `Summary` on `RunConflictStage` so wizard and dialog surfaces share counts.

## Problem Frame

Install Order and Archive Validation already emit per-mod prefixed cards plus a stage summary card. Conflicts only showed per-mod cards or a log line when clean—no `✅`/`⚠️`/`❌ Conflicts` summary.

## Requirements

- R1. `RunConflictStage` sets `Summary` from error/warning counts (or clean message).
- R2. `ApplyConflictsStage` adds summary cards matching Install Order patterns.
- R3. Presenter tests cover pass, warning-only, and restriction failure.
- R4. Brief KB note in `gui-validation-surfaces.md`.

## Scope Boundaries

**In scope:** `InstallationValidationPipeline.cs`, `WizardValidationStagePresenter.cs`, tests, KB.

**Deferred:** Holocron PR #111; MainWindow decomposition.

## Success Criteria

- [x] Conflicts stage summary on ValidatePage for pass/warn/fail
- [x] Tests pass
