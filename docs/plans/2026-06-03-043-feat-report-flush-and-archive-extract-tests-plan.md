---
title: "feat: copy-report flush in builder + archive extract/installations tests"
type: feat
status: active
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
prerequisite: docs/plans/2026-06-03-042-docs-validation-surfaces-kb-plan.md
---

# feat: copy-report flush in builder + archive extract/installations tests

## Holocron (PR #111)

- [ ] `Extract_MissingArchive_ReturnsError`
- [ ] `Installations_ReturnsOk` (JSON `ok: true`, `installations` array present)
- [ ] README test count 24 → 26

## Wizard (PR #110)

- [x] `BuildValidationReportText()` calls `FlushLogQueue()` so queued lines appear in `--- Log ---` even if copy path changes
- [x] `gui-architecture-deferred.md` plan `043` row

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
