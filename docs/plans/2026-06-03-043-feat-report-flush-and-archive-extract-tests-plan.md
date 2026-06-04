---
title: "feat: copy-report flush in builder + archive extract/installations tests"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
prerequisite: docs/plans/2026-06-03-042-feat-holocron-write-error-tests-plan.md
---

# feat: copy-report flush in builder + archive extract/installations tests

## Holocron (PR #111)

- [x] `Extract_MissingArchive_ReturnsError`
- [x] `Installations_ReturnsOk`
- [x] README test count 26

## Wizard (PR #110, merged)

- [x] `BuildValidationReportText()` calls `FlushLogQueue()` so queued lines appear in `--- Log ---` even if copy path changes
- [x] `gui-architecture-deferred.md` plan `043` row

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
