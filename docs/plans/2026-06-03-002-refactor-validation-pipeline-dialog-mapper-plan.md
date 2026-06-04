---
title: "refactor: shared validation pipeline dialog mapper"
type: refactor
status: completed
date: 2026-06-03
origin: GUI architecture questionnaire (dedupe MainWindow vs ValidationService validation UI mapping)
---

# refactor: shared validation pipeline dialog mapper

## Summary

Extract duplicated `InstallationValidationPipeline` → `ValidationIssue` mapping from `MainWindow` and `ValidationService` into one GUI service so legacy Validate button and failure-analysis paths show the same stage and dry-run issues.

## Problem Frame

`MainWindow.ValidateButton_Click` maps pipeline stages and dry-run VFS issues into `Dialogs.ValidationIssue` with full severity icons (✗/⚠/ℹ) and category-based solutions. `ValidationService.AnalyzeValidationFailures` reimplements a subset (errors/critical only, ❌ icon, generic solutions) and only surfaces environment as plain `systemIssues` text. Plan 025 deferred this refactor; questionnaire selected it as next work.

## Requirements

- R1. Single mapper owns stage mapping (Environment, Conflicts ERROR/WARNING, InstallOrder, ComponentValidation ERROR) and dry-run → `ValidationIssue` conversion.
- R2. `MainWindow` and `ValidationService` call the mapper; remove private duplicates (`AddPipelineStageIssuesToDialog`, `MapDryRunIssuesToDialogIssues`, `GetSolutionForIssue` from MainWindow).
- R3. `ValidationService` dry-run mapping matches MainWindow behavior (all severities, ✗/⚠/ℹ icons, `GetSolutionForIssue` text).
- R4. Headless unit tests cover prefixed message parsing and representative stage/dry-run mapping without GUI automation.
- R5. `ValidatePage.ApplyPipelineResultToWizardUi` unchanged in this slice (wizard log UI differs; optional follow-up).

## Scope Boundaries

**In scope:** New `ValidationPipelineDialogMapper`, MainWindow + ValidationService wiring, tests.

**Deferred:** ValidatePage log/result UI refactor, MainWindow decomposition, wizard host dedupe.

## Implementation Units

### U1. Add ValidationPipelineDialogMapper

**Files:** `src/ModSync.GUI/Services/ValidationPipelineDialogMapper.cs` (new)

**Approach:** Move logic from `MainWindow.AddPipelineStageIssuesToDialog`, dry-run loop, and `GetSolutionForIssue`. Expose `AddPipelineStageIssues`, `AddDryRunIssues`, `GetSolutionForIssue`, and internal `ParseModNameAndDescription` for tests.

### U2. Wire consumers

**Files:**
- Modify: `src/ModSync.GUI/MainWindow.axaml.cs`
- Modify: `src/ModSync.GUI/Services/ValidationService.cs`

**Approach:** Replace inline mapping with mapper calls; `AnalyzeValidationFailures` uses mapper for stages + dry-run (drop incomplete `MapDryRunIssuesToDialogIssues`).

### U3. Mapper unit tests

**Files:** `src/ModSync.Tests/ValidationPipelineDialogMapperTests.cs` (new)

**Scenarios:** ERROR:/WARNING: colon split; environment failure → one ✗ issue; dry-run warning → ⚠ issue with solution; archive category solution string.

## Verification

```bash
dotnet build ModSync.sln
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~ValidationPipelineDialogMapperTests"
```
