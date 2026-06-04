---
title: "feat: archive inject/remove error tests + Holocron KB + merge notes"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
prerequisite: docs/plans/2026-06-03-043-feat-holocron-extract-archive-installations-tests-plan.md
---

# feat: archive inject/remove error tests + Holocron KB + merge notes

## Holocron (PR #111)

- [x] `Inject_MissingArchive_ReturnsError`, `Remove_MissingArchive_ReturnsError`
- [x] Container status hints: Enter or double-click to open
- [x] `docs/knowledgebase/godot-holocron-editor.md` (layout, bridge, tests, Phase 2 pointer)
- [x] `docs/plans/2026-05-29-godot-holocron-editor-plugin-plan.md` status → Phase 1 archive track (PR #111)
- [x] README test count 26 → 28

## Wizard (PR #110)

- [ ] `GoToFirstIssueButton` ToolTip in ValidatePage.axaml
- [ ] `gui-validation-surfaces.md`: report builder calls `FlushLogQueue` (plan 043)
- [ ] `gui-architecture-deferred.md`: merge-ready note + plan `044`
- [ ] `docs/plans/2026-06-03-044-docs-wizard-pr110-merge-ready-plan.md`

## Verification

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```
