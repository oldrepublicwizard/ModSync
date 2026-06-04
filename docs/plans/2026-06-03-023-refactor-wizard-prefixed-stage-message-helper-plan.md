---
title: "refactor: Wizard shared prefixed stage message cards"
type: refactor
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-022-feat-wizard-dryrun-per-issue-result-cards-plan.md
branch: feat/wizard-archive-validation-parity
---

# refactor: Wizard shared prefixed stage message cards

## Summary

Extract duplicated `ERROR:`/`WARNING:` parsing in `WizardValidationStagePresenter` into one helper and wire Environment stage to emit prefixed message cards when the pipeline adds `ERROR:` lines.

## Problem Frame

Conflicts, Install Order, and ComponentValidation repeat identical parsing loops. Environment failures add `ERROR: {summary}` to `Messages` but the presenter ignored those lines and only showed a generic summary card.

## Requirements

- R1. `ApplyPrefixedStageMessageCards` returns count of cards added.
- R2. Conflicts, Install Order, and ComponentValidation use the helper (preserve OK: handling in archives).
- R3. Environment failure: prefixed cards from messages; summary card only when no prefixed card was added.
- R4. Existing presenter tests updated/extended; all pass.

## Scope Boundaries

**In scope:** `WizardValidationStagePresenter.cs`, tests, brief KB note.

**Deferred:** Holocron PR #111.

## Success Criteria

- [x] No duplicated parse loops in three stage handlers
- [x] Environment ERROR line surfaces as a result card
- [x] 9+ presenter tests pass
