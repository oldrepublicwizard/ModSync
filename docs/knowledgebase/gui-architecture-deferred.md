# GUI architecture — deferred work

`[SYNTH]` Larger Avalonia refactors called out in plans #025, #030, #002, #003. None of these block CLI/GUI **validation pipeline parity** (landed in PRs #94, #103–#104). Use this page to avoid mixing small fixes with multi-week structural work.

## Completed validation UI consolidation

| Item | Status | Reference |
|------|--------|-----------|
| Unified `InstallationValidationPipeline` (GUI + CLI) | Done | [validation-pipeline.md](validation-pipeline.md), PR #94 |
| `ValidationPipelineDialogMapper` (dialog issue rows) | Done | [gui-validation-surfaces.md](gui-validation-surfaces.md), PR #103 |
| Shared `ERROR:`/`WARNING:` parsing on `ValidatePage` conflicts | Done | PR #104 |

## Deferred — high impact

### MainWindow god object

`[REPO]` `MainWindow.axaml.cs` still coordinates editor, downloads, validation, drag-drop, menus, and Getting Started. Extraction targets (from audits/plans): download orchestration, validation entry, mod list/editor hosting.

**Agent guidance:** Do not add new feature logic inline without a plan; prefer existing services under `src/KOTORModSync.GUI/Services/`.

### Duplicate wizard hosts

`[REPO]` Two entry paths:

- `InstallWizardDialog` — modal wizard (`src/KOTORModSync.GUI/Dialogs/InstallWizardDialog.axaml.cs`)
- `WizardHostControl` — embedded host (`src/KOTORModSync.GUI/Controls/WizardHostControl.axaml.cs`)

Both register similar page sequences. Consolidation requires UX decisions (modal vs embedded) and regression testing across editor + install flows.

**Agent guidance:** Full-build validation uses `InstallWizardDialog` / `ValidatePage` per `AGENTS.md`; do not assume `WizardHostControl` is identical without reading both initializers.

### ValidatePage remaining UI duplication

`[REPO]` Environment, install-order, component archive, and dry-run **wizard cards** still duplicate messaging logic that exists in spirit in the mapper/dialog path. Further dedupe needs a wizard-specific presenter (log lines + `AddResult`), not only `ValidationIssue` mapping.

See [gui-validation-surfaces.md](gui-validation-surfaces.md).

## Deferred — medium impact

| Topic | Notes |
|-------|--------|
| Widescreen install batch API | Dynamic pages after base install; GUI-only progress surfaces `[UI]` |
| Download status depth | Some status UI is GUI-only; headless tests use cache/orchestration APIs — [download-system.md](download-system.md) |
| `EmbeddedLogPanel` | Line-count fix landed (#99); further output UX is cosmetic |

## Deferred — separate product track

| Topic | Notes |
|-------|--------|
| Godot Holocron editor plugin | Open PR #92 (`feat/godot-holocron-editor-plugin`); not part of KOTORModSync install wizard work |

## Suggested PR sizing

| Size | Examples |
|------|----------|
| Small | Mapper/parser tweaks, `ValidatePage` log copy, KB updates |
| Medium | Extract one `MainWindow` concern into a service |
| Large | Single wizard host, or MainWindow split into multiple PRs with feature flags |

## Related plans

- `docs/plans/2026-06-03-002-refactor-validation-pipeline-dialog-mapper-plan.md` (completed)
- `docs/plans/2026-06-03-003-refactor-validatepage-stage-message-parser-plan.md` (completed)
- `docs/plans/2026-05-30-003-feat-tier2-knowledgebase-pages-plan.md` (Tier 3 items)
