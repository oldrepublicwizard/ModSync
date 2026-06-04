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

- [ ] `Probe_ExtractedTwoDa_ReturnsTwodaEditorKind` (extract from sample.mod then probe)
- [ ] README test count 29 → 30

## Wizard (PR #110)

- [ ] `validation-pipeline.md`: ValidatePage presentation section → `gui-validation-surfaces.md`
- [ ] `gui-architecture-deferred.md` plan `046` row

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~Probe_ExtractedTwoDa"
```
