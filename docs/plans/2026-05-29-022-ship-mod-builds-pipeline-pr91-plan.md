---
title: Ship mod-builds pipeline PR #91
type: feat
status: completed
date: 2026-05-29
origin: docs/brainstorms/2026-05-29-mod-builds-pipeline-requirements.md
---

# Ship mod-builds pipeline PR #91

## Summary

Close the mod-builds full pipeline initiative: mark the brainstorm complete, refresh PR #91 residual findings to reflect landed work (#93 closed, dry-run warning implemented), run verification, and leave the branch merge-ready.

## Problem Frame

Plans 017–021 delivered round-trip, merge, dry-run, install wiring, and accuracy closure. PR #91 is open with CI green. Residual review items are stale in the PR body; the brainstorm remains `active`.

## Requirements

- R1. Set brainstorm `Status: completed` in `docs/brainstorms/2026-05-29-mod-builds-pipeline-requirements.md`.
- R2. Update PR #91 body `Residual Review Findings` to mark #93 resolved and dry-run warning resolved; keep `FullBuildInstallLongRunning` deferred.
- R3. Run targeted test filter from plan 021 verification; confirm exit 0.
- R4. Flip this plan `status: completed` when done.

## Scope Boundaries

- **In scope:** Docs, PR body, verification.
- **Out of scope:** New features, merge to master (user/automation decision), LongRunning install.
- **Deferred:** `FullBuildInstallLongRunning` on CI.

## Implementation Units

### U1. Mark brainstorm completed

**Goal:** Reflect pipeline delivery in requirements doc.  
**Files:** `docs/brainstorms/2026-05-29-mod-builds-pipeline-requirements.md`  
**Approach:** Change `Status: active` → `Status: completed`.  
**Test expectation:** none — metadata only.

### U2. Refresh PR #91 residual section

**Goal:** PR body matches current branch state.  
**Files:** PR #91 via `gh pr edit`  
**Approach:** Replace residual bullets: #93 done (b50594c), dry-run warning done (line 170 in agent script); note deferred LongRunning.  
**Test expectation:** none — PR metadata.

### U3. Verification run

**Goal:** Confirm pipeline tests still pass locally.  
**Files:** `src/ModSync.Tests/ModSync.Tests.csproj`  
**Approach:** Run filter from plan 021 verification block.  
**Verification:** Exit code 0.

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "Name~ModBuildConverterCliIntegrationTests|Name~AutoGenerateLocalCliIntegrationTests|Name~FullBuildMergedDryRunTests|Name~FullBuildMarkdownMergeRoundTripTests|Name~FullBuildSerializationRoundTripTests"
```
