---
title: "feat: sample.mod fixture assertion + KB/CLI docs index"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# feat: sample.mod fixture assertion + KB/CLI docs index

## Holocron (PR #111)

- [x] `Read_SampleMod_ResourcesIncludeTest2da` test
- [x] `tools/godot-holocron/README.md` archive CLI examples (extract/inject/remove)
- [x] `docs/knowledgebase/README.md` → `godot-holocron-editor.md`
- [x] `godot-holocron-editor.md` plan range through `045`

## Wizard (PR #110)

- [ ] `AGENTS.md` validation surfaces routing line
- [ ] `docs/knowledgebase/README.md` holocron + validation cross-links
- [ ] `gui-architecture-deferred.md` plan `045` closure

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests|FullyQualifiedName~WizardValidationStagePresenter"
```
