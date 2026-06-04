---
title: "docs: doc-hierarchy active PR routing + validation-pipeline sync"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# docs: doc-hierarchy active PR routing (plan 052)

## Problem

Plan `051` added parallel-PR routing to `AGENTS.md`, but `doc-hierarchy.md` and `.github/copilot-instructions.md` do not mention the open #110 / #111 tracks. The Holocron branch is missing the `ValidatePage presentation (PR #110)` cross-link in `validation-pipeline.md`.

## Scope

### In scope

- `docs/knowledgebase/doc-hierarchy.md` — active open PR table (both branches)
- `.github/copilot-instructions.md` — one-line pointer to `AGENTS.md` parallel PR section (both branches)
- `validation-pipeline.md` — restore ValidatePage UX cross-link on Holocron branch
- Index plan `052` in `gui-architecture-deferred.md` (wizard) and `godot-holocron-editor.md` (Holocron)

### Out of scope

- Code changes, merges, Phase 2 Holocron editors

## Implementation units

| Unit | Branch | Files |
|------|--------|-------|
| 1 | Both | `doc-hierarchy.md`, `copilot-instructions.md` |
| 2 | Holocron | `validation-pipeline.md` |
| 3 | Both | deferred KB plan index + plan file status |

## Verification

Docs-only; re-run existing filters if touching test docs (not required).

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
