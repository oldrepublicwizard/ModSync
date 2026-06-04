---
title: "docs: install-lifecycle ValidatePage UX link + push plan 048"
type: docs
status: active
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# docs: install-lifecycle link + Holocron read-shape KB (plan 049)

## Wizard (PR #110)

- [ ] Push pending commit `bd3f86f` (plan 048 KB) to `origin`
- [ ] `install-lifecycle.md` — link ValidatePage row to `gui-validation-surfaces.md` for UX/detail
- [ ] `gui-architecture-deferred.md` plan `049` index
- [ ] Update PR #110 body through plan `049` when `gh` is available

## Holocron (PR #111)

- [ ] `godot-holocron-editor.md` — document flat vs archive `read` payload shapes (tests 048+)
- [ ] Update PR #111 body through plan `049` when `gh` is available

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~WizardValidationStagePresenter"
```
