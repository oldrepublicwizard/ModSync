---
title: "feat: validation log flush on focus + Holocron probe CLI tests"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
prerequisite: docs/plans/2026-06-03-040-feat-validatepage-reset-log-scroll-plan.md
---

# feat: validation log flush on focus + Holocron probe CLI tests

## Problem

1. **ValidatePage** — `FocusFirstValidationIssue` scrolls `_logBuilder` immediately, but log lines may still sit in `_logQueue` until the 100ms timer fires. Auto-focus after validation can miss the first ERROR/WARNING line.
2. **Holocron bridge** — PR #111 added `probe.editor_kind` and error paths for archive ops; probe failures (missing path, unknown extension) lack regression tests.

## Scope

| Track | PR | Work |
|-------|-----|------|
| Wizard | #110 | Call `FlushLogQueue()` at start of `FocusFirstValidationIssue` |
| Holocron | #111 | Add `Probe_*` tests; update README test count and deferred KB |

Out of scope: Phase 2 editors, MainWindow split, merging PRs.

## Wizard — requirements

- [x] `FocusFirstValidationIssue` calls `FlushLogQueue()` before `ScrollLogToFirstIssueLine`
- [x] No behavior change when log is empty

**Files:** `src/KOTORModSync.GUI/Dialogs/WizardPages/ValidatePage.axaml.cs`

**Verification:** manual reasoning; existing wizard presenter tests unchanged.

## Holocron — requirements

- [ ] `Probe_SampleTwoDa_ReturnsEditorKind` asserts `editor_kind` is `twoda` (or documented value from bridge)
- [ ] `Probe_MissingPath_ReturnsError` — nonexistent path, `ok: false`, error mentions exist
- [ ] `Probe_UnknownExtension_ReturnsError` — temp file `.notkotor`, `ok: false`, error mentions resource type
- [ ] README: bump test count note (20 → 23)
- [ ] `docs/knowledgebase/gui-architecture-deferred.md`: plan 041 row on wizard track; holocron PR #111 probe tests note

**Files:**

- `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`
- `tools/godot-holocron/README.md`
- `docs/knowledgebase/gui-architecture-deferred.md`

**Verification:**

```bash
dotnet test src/KOTORModSync.Tests/KOTORModSync.Tests.csproj --filter "FullyQualifiedName~KotorFormatBridgeCliTests"
```

## Risks

- Unknown-extension test depends on PyKotor rejecting `.notkotor`; if extension maps unexpectedly, adjust fixture name.

## Done when

Both branches pushed, PR bodies note plan 041, CI green.
