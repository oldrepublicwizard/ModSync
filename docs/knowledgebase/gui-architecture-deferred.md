# GUI architecture — deferred work

`[SYNTH]` Larger Avalonia refactors called out in plans #025, #030, and early Tier 2 KB. **Validation UI consolidation (questionnaire arc) is complete** as of PRs #103–#107. Use this page to avoid mixing small validation fixes with multi-week structural work.

## Completed validation UI consolidation (2026-06-03 arc)

| Item | Status | Reference |
|------|--------|-----------|
| Unified `InstallationValidationPipeline` (GUI + CLI) | Done | [validation-pipeline.md](validation-pipeline.md), PR #94 |
| `ValidationPipelineDialogMapper` (dialog issue rows) | Done | [gui-validation-surfaces.md](gui-validation-surfaces.md), PR #103 |
| Shared `ERROR:`/`WARNING:` parsing on `ValidatePage` conflicts | Done | PR #104 |
| KB: validation surfaces + deferred scope | Done | PR #105 |
| `LegacyValidationRunner` (Getting Started validate pipeline) | Done | PR #106 |
| `WizardValidationStagePresenter` (`ValidatePage` stage UI) | Done | PR #107 |

Plans: `docs/plans/2026-06-03-002` through `007` (002–006 implementation, 007 KB closure).

## Deferred — high impact

### MainWindow god object

`[REPO]` `MainWindow.axaml.cs` still coordinates editor, downloads, validation orchestration UI, drag-drop, menus, and Getting Started. Further extraction targets: download orchestration, editor hosting, menus — validation **pipeline** work now lives in `LegacyValidationRunner` + `ValidationPipelineDialogMapper`.

**Agent guidance:** Do not add new feature logic inline without a plan; prefer `src/KOTORModSync.GUI/Services/`.

### Duplicate wizard hosts

`[REPO]` Two entry paths:

- `InstallWizardDialog` — modal wizard (`src/KOTORModSync.GUI/Dialogs/InstallWizardDialog.axaml.cs`)
- `WizardHostControl` — embedded host (`src/KOTORModSync.GUI/Controls/WizardHostControl.axaml.cs`)

Both register similar page sequences. Consolidation requires UX decisions (modal vs embedded) and regression testing across editor + install flows.

**Agent guidance:** Full-build validation uses `InstallWizardDialog` / `ValidatePage` per `AGENTS.md`; do not assume `WizardHostControl` is identical without reading both initializers.

## Deferred — medium impact

| Topic | Notes |
|-------|--------|
| Widescreen install batch API | Dynamic pages after base install; GUI-only progress surfaces `[UI]` |
| Download status depth | Some status UI is GUI-only; headless tests use cache/orchestration APIs — [download-system.md](download-system.md) |
| `EmbeddedLogPanel` | Line-count fix landed (#99); further output UX is cosmetic |
| Archive `ERROR:` wizard result cards | Dialog mapper lists archive errors; wizard logs only — optional UX parity |

## Deferred — separate product track

| Topic | Notes |
|-------|--------|
| Godot Holocron editor plugin | Open PR #92 (`feat/godot-holocron-editor-plugin`); not part of KOTORModSync install wizard work |

## Suggested PR sizing

| Size | Examples |
|------|----------|
| Small | KB updates, mapper/parser tweaks |
| Medium | Extract one non-validation `MainWindow` concern into a service |
| Large | Single wizard host, or MainWindow split into multiple PRs |

## Related plans

- `docs/plans/2026-06-03-002-refactor-validation-pipeline-dialog-mapper-plan.md` (completed)
- `docs/plans/2026-06-03-003-refactor-validatepage-stage-message-parser-plan.md` (completed)
- `docs/plans/2026-06-03-005-refactor-legacy-validation-runner-plan.md` (completed)
- `docs/plans/2026-06-03-006-refactor-wizard-validation-stage-presenter-plan.md` (completed)
- `docs/plans/2026-05-30-003-feat-tier2-knowledgebase-pages-plan.md` (Tier 3 structural items)
