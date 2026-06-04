# GUI validation surfaces

`[REPO]` How the Avalonia app presents `InstallationValidationPipeline` results after PRs #103–#104. Core pipeline behavior is in [validation-pipeline.md](validation-pipeline.md).

## Shared mapper

**Type:** `KOTORModSync.Services.ValidationPipelineDialogMapper`  
**Source:** `src/KOTORModSync.GUI/Services/ValidationPipelineDialogMapper.cs`  
**Tests:** `ValidationPipelineDialogMapperTests` in `KOTORModSync.Tests`

| API | Purpose |
|-----|---------|
| `AddPipelineStageIssues` | Environment; per-mod + aggregate Conflicts / Install Order / Archive Validation rows → `Dialogs.ValidationIssue` list |
| `AddDryRunIssues` | All VFS dry-run severities (✗/⚠/ℹ) with category-based solutions |
| `GetSolutionForIssue` | User-facing fix hints from `Core.Services.FileSystem.ValidationIssue` category/message |
| `TryParsePrefixedStageMessage` | Parse `ERROR:` / `WARNING:` stage lines into mod name + detail (shared with wizard) |
| `ParseModNameAndDescription` | Colon-split after prefix strip (`Mod Name: description`) |

## Surfaces (who calls the pipeline)

| Surface | Entry | Pipeline preset | Result UI |
|---------|--------|-----------------|-----------|
| Install wizard **ValidatePage** | `ValidateAsync` / **Run Validation** | `WizardFull` | In-page log, progress, summary badges; `ApplyPipelineResultToWizardUi` |
| Legacy **Getting Started → Validate** | `MainWindow.ValidateButton_Click` | `WizardFull` | Progress dialog log + `ValidationDialog` mod issue list |
| **ValidationService.AnalyzeValidationFailures** | Async helper (pre-check / failure analysis) | `WizardFull` | Populates `Dialogs.ValidationIssue` + optional `systemIssues` strings |
| Core CLI **validate** / **install** pre-check | `ModBuildConverter` | `WizardFull` (install) | stdout / exit code — no GUI mapper |

`[REPO]` All GUI paths above that run the full pipeline should use the same Core stages; presentation differs per surface.

## ValidatePage vs dialog mapping

`ValidatePage` (`src/KOTORModSync.GUI/Dialogs/WizardPages/ValidatePage.axaml.cs`) does **not** build `ValidationDialog` rows. It:

- Copies aggregate counts from `ValidationPipelineResult` (`ErrorCount`, `WarningCount`, …)
- Logs each stage step-by-step
- Adds wizard result cards via `AddResult(title, message)`

**Unified (PR #104+):** Conflicts, Install Order, ComponentValidation, and Environment failure (`ERROR:` from `RunEnvironmentStageAsync`) use `ApplyPrefixedStageMessageCards` → `TryParsePrefixedStageMessage` so wizard cards match the dialog mapper. Environment keeps the `❌ Environment Error` summary card only when no prefixed message lines were added.

**Wizard stage UI:** `WizardValidationStagePresenter.ApplyStages` (`src/KOTORModSync.GUI/Services/WizardValidationStagePresenter.cs`) — log lines + `AddResult` cards for each pipeline stage. `ValidatePage` passes `AppendLog` / `AddResult` delegates. Headless coverage: `WizardValidationStagePresenterTests` in `KOTORModSync.Tests`.

**Unified (archive parity):** ComponentValidation `ERROR:` and `WARNING:` lines produce wizard result cards via the same `TryParsePrefixedStageMessage` rules as `ValidationDialog` archive rows. Failed or warning archive stages also add aggregate `Archive Validation` summary cards.

**Conflicts on ValidatePage:** Per-mod `ERROR:`/`WARNING:` cards plus aggregate `✅`/`⚠️`/`❌ Conflicts` summary (counts from `RunConflictStage.Summary`).

**Dry-run on ValidatePage:** `ApplyDryRunStage` adds up to five per-issue result cards (mod name, category, message, solution hint from `GetSolutionForIssue`) plus an aggregate `Instruction Execution` summary card.

`[UI]` Full-build agents can use **Copy report** on `ValidatePage` (after Run Validation) to copy summary counts, result cards, and the full validation log to the clipboard. **LogExpander** auto-expands when the run reports errors or warnings; clean passes leave it collapsed.

**Results panel** scrolls to the first `❌` result card after validation (or first `⚠️` when there are no errors) and applies a red or amber border on that card so it stands out in long lists.

**Validation log** scrolls to the first `ERROR:` / `❌` log line (or first `WARNING:` / `⚠` when there are no errors) when the log expander is open.

**Go to first issue** (log header) expands the log, then re-runs log scroll + result-card highlight after a failed or warned run. Both auto-focus after validation and this button call `FlushLogQueue()` first so batched log lines are visible before scroll/highlight (plan 041).

A new **Run Validation** resets scroll position on both the results panel and the log (plans 039–040).

**Copy report** flushes the log queue before building clipboard text (`--- Results ---` cards plus `--- Log ---` body). `BuildValidationReportText()` also calls `FlushLogQueue()` so the `--- Log ---` section stays complete if the report is built outside the button handler (plan 043).

## Legacy MainWindow validate

`MainWindow.ValidateButton_Click` → `RunValidationAsync` runs **`LegacyValidationRunner.RunAsync`** (`WizardFull` + mapper), then shows progress log and `ValidationDialog` with the returned issue list.

**Dialog aggregates:** Besides per-mod prefixed rows, `AddPipelineStageIssues` adds stage-level rows for failed/warned Conflicts, Install Order, and Archive Validation (mod name `Conflicts`, `Install Order`, or `Archive Validation` with `stage.Summary`)—aligned with wizard summary cards.

**Dialog prefixed lines:** Environment and Install Order parse `ERROR:`/`WARNING:` messages like other stages; Environment skips the duplicate aggregate row when a prefixed `ERROR:` line was already mapped (pipeline emits `ERROR: {summary}` on failure).

Prefer the **install wizard** for documented full-build flows ([install-lifecycle.md](install-lifecycle.md), `AGENTS.md`).

## Automated tests

```bash
./scripts/agents/verify_open_pr_ready.sh
# or: ./scripts/agents/test_pr110_validation.sh
```

## Debugging order

1. Confirm which surface the user used (wizard vs Getting Started).
2. Read Core pipeline output in logs — stage order is fixed in [validation-pipeline.md](validation-pipeline.md).
3. For missing or inconsistent **dialog** rows, inspect mapper callers (`MainWindow`, `ValidationService`).
4. For wizard **summary cards** only, inspect `ApplyPipelineResultToWizardUi` (not the mapper).

## Related

- [validation-pipeline.md](validation-pipeline.md) — stages and CLI flags
- [gui-architecture-deferred.md](gui-architecture-deferred.md) — larger GUI refactors not done here
- [agent-action-parity.md](agent-action-parity.md) — CLI vs GUI capabilities
- [godot-holocron-editor.md](godot-holocron-editor.md) — separate Godot track ([PR #111](https://github.com/th3w1zard1/ModSync/pull/111))
