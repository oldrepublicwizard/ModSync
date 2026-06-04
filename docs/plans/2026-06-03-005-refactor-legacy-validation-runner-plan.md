---
title: "refactor: extract legacy validation pipeline runner from MainWindow"
type: refactor
status: completed
date: 2026-06-03
origin: docs/knowledgebase/gui-architecture-deferred.md
---

# refactor: extract legacy validation pipeline runner from MainWindow

## Summary

Move pipeline execution and `ValidationPipelineDialogMapper` aggregation out of `MainWindow.RunValidationAsync` into a GUI service so the legacy Getting Started validate path is testable without Avalonia progress/dialog UI.

## Problem Frame

`RunValidationAsync` (~180 lines) mixes Core pipeline work with progress dialog, logger forwarding, and `ValidationDialog`. KB lists MainWindow decomposition as deferred; this is a minimal first extraction without changing UX.

## Requirements

- R1. New `LegacyValidationRunner` runs `InstallationValidationPipeline` with `WizardFull` and returns mod issues + summary fields.
- R2. `MainWindow.RunValidationAsync` calls the runner; keeps all Avalonia UI (progress, dialog, Step4 checkbox).
- R3. Unit test covers runner mapping with synthetic pipeline result (no Avalonia).
- R4. Behavior unchanged for validate button and `GettingStartedValidateButton_Click` (still delegates to same click path).

## Scope Boundaries

**In scope:** `LegacyValidationRunner`, `MainWindow` wiring, one test class.

**Deferred:** Full `RunValidationAsync` move to service; wizard host dedupe; ValidatePage presenter.

## Implementation Units

### U1. LegacyValidationRunner + MainWindow wiring

**Files:**
- New: `src/ModSync.GUI/Services/LegacyValidationRunner.cs`
- Modify: `src/ModSync.GUI/MainWindow.axaml.cs`

### U2. Runner tests

**Files:** `src/ModSync.Tests/LegacyValidationRunnerTests.cs`

**Verification:** `dotnet test --filter LegacyValidationRunnerTests`
