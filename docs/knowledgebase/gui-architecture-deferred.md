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

### PR #110 extension (wizard + dialog parity, same branch)

| Item | Status | Reference |
|------|--------|-----------|
| Archive / Install Order / Dry-run wizard result cards | Done | plans `021`–`022` |
| Shared `ApplyPrefixedStageMessageCards` | Done | plan `023` |
| Conflicts wizard summaries + pipeline counts | Done | plan `024` |
| Dialog mapper stage aggregates (Conflicts, archives) | Done | plan `025` |
| Environment / Install Order prefixed dialog rows | Done | plan `026` |
| ValidatePage **Copy report** | Done | plan `027` |
| Auto-expand validation log on errors/warnings | Done | plan `029` |
| Scroll + highlight first issue result card | Done | plans `033`, `035` |
| Scroll validation log to first issue line | Done | plan `036` |
| Go to first issue button + expand log on focus | Done | plans `037`, `038` |
| Reset results/log scroll on re-validation | Done | plans `039`, `040` |
| Flush log queue before focus/scroll to first issue | Done | plan `041` |

Plans: `docs/plans/2026-06-03-012`, `021`–`029`, `033`–`041`. Surface reference: [gui-validation-surfaces.md](gui-validation-surfaces.md). **PR #110** is merge-ready for the validation parity arc; Holocron archive track is **PR #111** (not the install wizard).

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
| Archive `ERROR:` wizard result cards | Done (PR #110) — wizard + `ValidationPipelineDialogMapper` parity |
| ValidatePage copy report / log UX | Done (PR #110) — see [gui-validation-surfaces.md](gui-validation-surfaces.md) |

## Deferred — separate product track

| Topic | Notes |
|-------|--------|
| Godot Holocron editor plugin | Open PR #111 (`feat/holocron-erf-nested-open`); separate from KOTORModSync install wizard work |

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
