---
title: "refactor: ValidatePage reuses pipeline stage message parser"
type: refactor
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-002-refactor-validation-pipeline-dialog-mapper-plan.md
---

# refactor: ValidatePage reuses pipeline stage message parser

## Summary

Remove duplicated `ERROR:` / `WARNING:` colon-split parsing in `ValidatePage.ApplyPipelineResultToWizardUi` by calling the shared helpers on `ValidationPipelineDialogMapper` introduced in PR #103.

## Problem Frame

Plan 002 centralized dialog-row mapping but left wizard log/result UI with a third copy of conflict message parsing (lines ~405–418). Divergence risk remains for mod name extraction from `ERROR: Mod: detail` messages.

## Requirements

- R1. `ValidatePage` conflict stage uses `ValidationPipelineDialogMapper.TryParsePrefixedStageMessage` for `ERROR:` and `WARNING:` messages.
- R2. Wizard `AddResult` titles and bodies match current behavior (`⚠️/❌ {modName}`, full detail string).
- R3. Expose parser API as `public` on the mapper (was `internal`).
- R4. Existing `ValidationPipelineDialogMapperTests` remain green; no new GUI automation required.

## Scope Boundaries

**In scope:** Conflicts stage loop in `ValidatePage.axaml.cs`, mapper visibility.

**Deferred:** Full wizard UI refactor of environment/install-order/dry-run panels; MainWindow decomposition; wizard host dedupe.

## Implementation Units

### U1. Public parser + ValidatePage wiring

**Files:**
- Modify: `src/ModSync.GUI/Services/ValidationPipelineDialogMapper.cs`
- Modify: `src/ModSync.GUI/Dialogs/WizardPages/ValidatePage.axaml.cs`

**Verification:** `dotnet test --filter ValidationPipelineDialogMapperTests`
