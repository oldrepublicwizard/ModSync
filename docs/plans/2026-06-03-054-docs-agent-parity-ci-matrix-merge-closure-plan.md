---
title: "docs: agent-parity + CI matrix PR filters + routing closure"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# docs: agent-parity and CI matrix merge closure (plan 054)

## Problem

Parallel PR routing exists in `AGENTS.md`, `doc-hierarchy.md`, and the KB index, but `agent-action-parity.md`, `ci-test-matrix.md`, `product-overview.md`, and `.cursorrules` still omit #110 / #111 handoff. Reviewers validating a PR may run the wrong test filter.

## Scope

### In scope

- `agent-action-parity.md` — Holocron track + validation UX test filters in headless table
- `ci-test-matrix.md` — PR-targeted local `dotnet test` filters
- `product-overview.md` — active open PRs note
- `.cursorrules` — one-line parallel PR pointer
- Plan `054` index on wizard + Holocron KB pages

### Out of scope

- Merging PRs, new features, browser tests

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
