---
title: "refactor: WizardValidationStagePresenter for ValidatePage"
type: refactor
status: completed
date: 2026-06-03
origin: docs/knowledgebase/gui-architecture-deferred.md
---

# refactor: WizardValidationStagePresenter for ValidatePage

## Summary

Extract `ValidatePage.ApplyPipelineResultToWizardUi` stage switch into `WizardValidationStagePresenter` so wizard log/result cards share one presenter and stay aligned with `ValidationPipelineDialogMapper` parsing rules.

## Requirements

- R1. New `WizardValidationStagePresenter.ApplyStages` takes pipeline result, selected-mod count, and `appendLog` / `addResult` callbacks.
- R2. `ValidatePage` delegates to presenter; behavior unchanged.
- R3. Unit tests cover environment failure and conflict `ERROR:` result card via callbacks.
- R4. No change to `InstallationValidationPipeline` or dialog mapper APIs.

## Scope Boundaries

**In scope:** Presenter class, ValidatePage wiring, tests.

**Deferred:** Wizard host consolidation; moving `ValidateAsync` pipeline invocation into `LegacyValidationRunner`.

## Implementation Units

### U1. Presenter + ValidatePage + tests

**Files:**
- New: `src/ModSync.GUI/Services/WizardValidationStagePresenter.cs`
- Modify: `src/ModSync.GUI/Dialogs/WizardPages/ValidatePage.axaml.cs`
- New: `src/ModSync.Tests/WizardValidationStagePresenterTests.cs`

**Verification:** `dotnet test --filter WizardValidationStagePresenterTests`
