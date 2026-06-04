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

## Wizard (PR #110)

- [x] `AGENTS.md` — parallel PR table (#110 / #111) and test filters
- [x] `gui-architecture-deferred.md` — plan `051` index row
- [x] `docs/knowledgebase/README.md` — PR links on validation + Holocron entries
- [x] `gui-validation-surfaces.md` — Holocron cross-link in Related

## Holocron (PR #111)

- [x] Full `gui-validation-surfaces.md` sync from wizard branch + sync banner
- [x] `godot-holocron-editor.md` — plans through `051`, PR #110 pointer
- [x] Same `AGENTS.md` / README updates (committed on `feat/holocron-erf-nested-open`)

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter|FullyQualifiedName~ValidationPipelineDialogMapper"
```
