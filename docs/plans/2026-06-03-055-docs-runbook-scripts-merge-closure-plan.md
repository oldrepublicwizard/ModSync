---
title: "docs: runbook and scripts README merge-closure routing"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# docs: runbook and scripts README merge closure (plan 055)

## Wizard (PR #110)

- [x] `local_desktop_agent_runbook.md`, `scripts/agents/README.md`
- [x] `gui-architecture-deferred.md` — plan `055`

## Holocron (PR #111)

- [x] Same runbook + scripts (committed on holocron branch)
- [x] `godot-holocron-editor.md` — plan index `055`

## Problem

Plans `051`–`054` wired parallel PR routing through KB, AGENTS, and copilot-instructions, but procedural entry points (`docs/local_desktop_agent_runbook.md`, `scripts/agents/README.md`) still lack #110 / #111 scope and test filters.

## Scope

### In scope

- `docs/local_desktop_agent_runbook.md` — open PR scope + link to ci-test-matrix filters
- `scripts/agents/README.md` — PR-targeted `dotnet test` examples
- Plan `055` index on Holocron + wizard KB pages

### Out of scope

- Merging PRs, code changes, browser tests

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
```
