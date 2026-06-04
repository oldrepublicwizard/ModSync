---
title: "docs: KB index + Holocron README merge-closure pointers"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# docs: KB README and Holocron README merge closure (plan 053)

## Problem

Plans `051`–`052` added parallel-PR routing to `AGENTS.md` and `doc-hierarchy.md`, but the knowledgebase index and `tools/godot-holocron/README.md` still reference stale PR numbers (#92/#109) and omit merge-ready handoff for reviewers.

## Scope

### In scope

- `docs/knowledgebase/README.md` — active open PRs subsection (both branches)
- `tools/godot-holocron/README.md` — PR #111, KB link, test filter, merge note (Holocron branch)
- Plan index `053` in `godot-holocron-editor.md` and `gui-architecture-deferred.md`

### Out of scope

- Merging PRs, new bridge features, Phase 2 editors

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
```

Docs-only; browser tests skipped.
