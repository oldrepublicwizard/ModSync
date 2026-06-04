---
title: "feat: probe extracted member + validation pipeline ValidatePage KB"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# feat: probe extracted member + validation pipeline ValidatePage KB

## Holocron (PR #111)

- [x] `Probe_ExtractedTwoDa_ReturnsTwodaEditorKind` (extract from sample.mod then probe)
- [x] README test count 29 → 30

## Wizard (PR #110)

- [x] `validation-pipeline.md`: ValidatePage presentation section → `gui-validation-surfaces.md` (plan `047` on wizard branch)
- [x] `gui-architecture-deferred.md` plan `046` row (plan `047` on wizard branch)

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~Probe_ExtractedTwoDa"
```
