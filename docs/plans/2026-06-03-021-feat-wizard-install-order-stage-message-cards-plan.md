---
title: "feat: Wizard Install Order stage prefixed message result cards"
type: feat
status: completed
date: 2026-06-03
origin: docs/knowledgebase/gui-validation-surfaces.md
prerequisite: docs/plans/2026-06-03-012-feat-wizard-archive-validation-parity-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: Wizard Install Order stage prefixed message result cards

## Summary

Extend `WizardValidationStagePresenter` Install Order handling so `ERROR:`/`WARNING:` stage lines (e.g. circular dependency) get the same result cards as Conflicts and ComponentValidation, matching dialog mapper parity.

## Problem Frame

Install Order can emit `ERROR: {message}` in `stage.Messages` but the wizard only showed a single summary card on failure, hiding prefixed detail lines.

## Requirements

- R1. `ApplyInstallOrderStage` logs each message and parses `ERROR:`/`WARNING:` into result cards.
- R2. Preserve existing summary cards for pass/warn/fail aggregate outcomes.
- R3. Regression test for circular-dependency style `ERROR:` message line.
- R4. Update `gui-validation-surfaces.md` Install Order note.

## Scope Boundaries

**In scope:** `WizardValidationStagePresenter.cs`, tests, KB.

**Deferred:** Per-issue dry-run cards; Holocron PR #111.

## Success Criteria

- [x] Install Order ERROR line produces a result card
- [x] Tests pass
- [x] KB updated
