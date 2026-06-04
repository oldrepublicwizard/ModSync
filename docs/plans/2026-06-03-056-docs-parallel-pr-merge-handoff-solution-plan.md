---
title: "docs: parallel PR merge handoff solution + install-lifecycle pointer"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# docs: parallel PR merge handoff (plan 056)

## Problem

Plans `051`–`055` document routing and pre-merge tests, but there is no durable **post-merge** playbook for landing #110 and #111 in either order without doc drift or a stale long-lived branch.

## Scope

### In scope

- `docs/solutions/parallel-pr-merge-handoff-2026-06-03.md` — merge order, rebase, verification
- `docs/knowledgebase/README.md` — link solution from Active open PRs
- `docs/knowledgebase/install-lifecycle.md` — open PR scope pointer
- Merge checklist step on wizard + Holocron KB pages
- Plan `056` index

### Out of scope

- Executing merges, Phase 2 Holocron code

## Verification

Docs-only. Optional smoke:

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
