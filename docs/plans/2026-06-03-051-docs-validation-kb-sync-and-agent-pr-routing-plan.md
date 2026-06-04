---
title: "docs: sync validation KB + agent parallel-PR routing"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# docs: validation KB sync and parallel-PR agent routing (plan 051)

## Problem

`feat/holocron-erf-nested-open` still carries a pre–PR #110 `gui-validation-surfaces.md` (archive errors described as log-only). Agents on the Holocron branch can mis-route validation work. Both open PRs (#110, #111) need a single AGENTS entry for test filters and merge independence.

## Scope

### In scope

- Replace `docs/knowledgebase/gui-validation-surfaces.md` on Holocron branch with PR #110 content (no code changes).
- Add **Parallel open PRs** section to `AGENTS.md` on both branches.
- Index plan `051` in `godot-holocron-editor.md` (Holocron) and `gui-architecture-deferred.md` (wizard).
- Cherry-pick or add `docs/plans/2026-06-03-050-docs-push-sync-and-merge-checklist-plan.md` on Holocron if missing.

### Out of scope

- Merging PRs, Phase 2 Holocron editors, MainWindow split.

## Implementation units

### Unit 1 — Holocron branch KB sync

- Files: `docs/knowledgebase/gui-validation-surfaces.md`
- Copy authoritative content from `origin/feat/wizard-archive-validation-parity:docs/knowledgebase/gui-validation-surfaces.md`
- One-line banner: validation UX changes belong on PR #110 branch.

### Unit 2 — Agent routing (both branches)

- Files: `AGENTS.md`
- Section: links to PR #110 / #111, branches, targeted `dotnet test` filters, “do not bundle PRs”.

### Unit 3 — Plan index closure

- Holocron: `docs/knowledgebase/godot-holocron-editor.md` → plans through `051`
- Wizard: `docs/knowledgebase/gui-architecture-deferred.md` → plan `051` row
- Add plan `050` file on Holocron if absent

## Verification

```bash
# Holocron
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"

# Wizard (after checkout)
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter|FullyQualifiedName~ValidationPipelineDialogMapper"
```

Docs-only; no GUI/browser run required.

## Requirements traceability

| Requirement | Unit |
|-------------|------|
| Accurate validation KB on Holocron branch | 1 |
| Agent discovers dual PR tracks | 2 |
| Plan arc indexed through 051 | 3 |
