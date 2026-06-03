---
title: Ship PR #92 Godot Holocron Phase 0
type: feat
status: active
date: 2026-06-03
origin: docs/knowledgebase/gui-architecture-deferred.md
---

# Ship PR #92 Godot Holocron Phase 0

## Summary

Rebase `feat/godot-holocron-editor-plugin` onto current `master`, drop duplicate mod-builds commits already merged via PRs #91/#94, push to trigger GitHub Actions, fix any CI failures, and refresh PR #92 metadata.

## Problem Frame

PR #92 is open with **no CI checks reported**. The branch diverged from an old merge-base and still contains mod-builds pipeline commits that are already on `master`. Only `dc057e5` (Holocron Phase 0) is unique product work.

## Requirements

- R1. Branch tip is `master` + single Holocron commit (or equivalent clean history).
- R2. `git diff master...HEAD` contains Holocron feature files only (plus this ship plan).
- R3. Push branch; confirm PR #92 shows workflow runs.
- R4. Fix CI failures without weakening tests (up to 3 iterations in LFG step 8).
- R5. Run `KotorFormatBridgeCliTests` locally.

## Scope Boundaries

**In scope:** Rebase/cherry-pick, CI fixes, PR body update.

**Out of scope:** Holocron Phase 1+; merging without green CI; ModSync validation arc.

## Implementation Units

### U1. Rebase branch onto master

Cherry-pick `dc057e5` onto `master`; force-push with lease.

### U2. CI verification and fixes

Standard PR workflows must pass.

### U3. PR metadata

Update PR #92 body with rebase note and test plan.

## Success Criteria

- PR #92 mergeable with green required checks.
- Diff vs `master` is Holocron-scoped.
