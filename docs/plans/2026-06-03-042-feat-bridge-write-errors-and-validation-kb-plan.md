---
title: "feat: bridge write error tests + validation surface KB"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
prerequisite: docs/plans/2026-06-03-041-feat-holocron-probe-errors-and-enter-open-plan.md
---

# feat: bridge write error tests + validation surface KB

## Problem

Holocron bridge `write` failure paths (bad JSON, unsupported format) lack regression tests. ValidatePage focus/scroll/copy UX from plans 033–041 is undocumented in the validation KB.

## Holocron (PR #111)

- [x] `Write_InvalidJsonPayload_ReturnsError`
- [x] `Write_UnimplementedFormat_ReturnsError`
- [x] README: container keyboard shortcuts (Enter open, Esc filter, F5 refresh)
- [x] Test count note 22 → 24

**Files:** `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`, `tools/godot-holocron/README.md`

## Wizard (PR #110)

- [ ] `gui-validation-surfaces.md`: ValidatePage log UX (auto-expand, go to first issue, scroll/highlight, flush-before-focus, copy report sections)
- [ ] `gui-architecture-deferred.md`: plan `042` closure row

**Files:** `docs/knowledgebase/gui-validation-surfaces.md`, `docs/knowledgebase/gui-architecture-deferred.md`

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
