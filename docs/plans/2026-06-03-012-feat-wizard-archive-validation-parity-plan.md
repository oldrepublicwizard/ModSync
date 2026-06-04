---
title: "feat: wizard archive validation parity on ValidatePage"
type: feat
status: completed
date: 2026-06-03
origin: docs/knowledgebase/gui-architecture-deferred.md
---

# feat: wizard archive validation parity on ValidatePage

## Summary

Close the last documented ValidatePage validation UX gap: ComponentValidation archive `ERROR:` lines must produce wizard result cards (and warnings/summary) using the same parsing rules as `ValidationPipelineDialogMapper`, so full-build agents see missing-archive failures in the summary panel—not only in the expanded log.

## Problem Frame

PRs #103–#107 unified pipeline parsing for conflicts and dry-run summaries. `gui-architecture-deferred.md` still listed archive `ERROR:` as log-only on the wizard while the legacy dialog mapper already surfaced them. Users running **Run Validation** on `ValidatePage` had to expand logs to spot per-mod archive failures.

## Requirements

- R1. `WizardValidationStagePresenter` adds result cards for ComponentValidation `ERROR:` messages via `TryParsePrefixedStageMessage`.
- R2. Wizard also surfaces ComponentValidation `WARNING:` messages as `⚠️` cards (parity with conflicts stage).
- R3. When the archive stage fails (`Passed == false`), add an aggregate `❌ Archive Validation` card using `stage.Summary`.
- R4. `ValidationPipelineDialogMapper` maps ComponentValidation `WARNING:` to dialog issues (wizard + Getting Started dialog stay aligned).
- R5. Regression tests in `WizardValidationStagePresenterTests` and `ValidationPipelineDialogMapperTests`.
- R6. Update `gui-validation-surfaces.md` and `gui-architecture-deferred.md`; do not commit unrelated `vendor/bin` binary churn.

## Scope Boundaries

**In scope:** Presenter, mapper, tests, KB.

**Deferred:** Per-issue dry-run rows on ValidatePage; MainWindow decomposition.

## Assumptions

- Partial work for R1 may already exist on the working tree from a prior session; `ce-work` completes R2–R6 and excludes accidental vendor binary changes from the PR.

## Key Technical Decisions

- Reuse `ValidationPipelineDialogMapper.TryParsePrefixedStageMessage` in the wizard presenter—no duplicate parsers.
- Mirror conflicts-stage card titles: `❌ {modName}` / `⚠️ {modName}` with `detail` as message body.
- Add stage summary card only when `!stage.Passed` to avoid noise when all mods pass.

## Implementation Units

### U1. Wizard archive ERROR and WARNING cards

**Goal:** Per-mod archive failures and warnings appear in ValidatePage result cards.

**Requirements:** R1, R2

**Files:**
- Modify: `src/KOTORModSync.GUI/Services/WizardValidationStagePresenter.cs`

**Test scenarios:**
- `ERROR: Test Mod: missing archive` → card title `❌ Test Mod`, message contains detail.
- `WARNING: Test Mod: checksum mismatch` → card title `⚠️ Test Mod`.

**Verification:** `dotnet test` filter `WizardValidationStagePresenterTests`.

### U2. Wizard archive stage summary card

**Goal:** Failed archive stage shows aggregate summary like Environment/Install Order.

**Requirements:** R3

**Files:**
- Modify: `src/KOTORModSync.GUI/Services/WizardValidationStagePresenter.cs`

**Test scenarios:**
- Stage `Passed = false`, `Summary = "2 component error(s)"` → one card `❌ Archive Validation` with that summary (in addition to per-mod cards from U1).

**Verification:** New test in `WizardValidationStagePresenterTests`.

### U3. Dialog mapper archive WARNING parity

**Goal:** Getting Started / `ValidationDialog` lists archive warnings, not only errors.

**Requirements:** R4

**Files:**
- Modify: `src/KOTORModSync.GUI/Services/ValidationPipelineDialogMapper.cs`
- Modify: `src/KOTORModSync.Tests/ValidationPipelineDialogMapperTests.cs`

**Test scenarios:**
- `WARNING: Mod B: stale archive` → issue with `⚠`, `ArchiveValidation`, mod name `Mod B`.

**Verification:** `dotnet test` filter `ValidationPipelineDialogMapperTests`.

### U4. Knowledgebase closure

**Goal:** Docs reflect shipped parity; deferred table no longer lists archive wizard gap.

**Requirements:** R6

**Files:**
- Modify: `docs/knowledgebase/gui-validation-surfaces.md`
- Modify: `docs/knowledgebase/gui-architecture-deferred.md`

**Verification:** Read-only review.

## Success Criteria

- [x] ValidatePage shows per-mod archive ERROR/WARNING cards and aggregate failure summary.
- [x] Dialog mapper includes archive WARNING rows.
- [x] Presenter and mapper tests pass.
- [x] Plan `status: completed` after merge.
