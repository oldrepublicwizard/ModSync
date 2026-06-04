---
title: "feat: Holocron return to archive and refresh listing after save-back"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-014-feat-holocron-archive-inject-saveback-plan.md
prerequisite: docs/plans/2026-06-03-014-feat-holocron-archive-inject-saveback-plan.md
---

# feat: Holocron return to archive and refresh listing after save-back

## Summary

After a nested member save injects into the parent archive, reopen the parent archive in the dock so the container resource table shows updated byte sizes and the user can open another member without manually navigating back.

## Problem Frame

Plan 014 completed inject-on-save but left the user on the nested editor with a stale mental model. Container `size` columns come from `read` payload; they only refresh when the archive is re-read.

## Requirements

- R1. On successful `inject_member` from dock save-back, call `_open_path(archive)` for the parent archive path from inject context.
- R2. Failed inject does not navigate away from the nested editor (status shows error; context cleared per current behavior).
- R3. CLI test asserts `read` listing `size` for a member matches injected source file length on a temp archive copy.
- R4. README notes post-save return-to-archive behavior.

## Scope Boundaries

**In scope:** Dock navigation after successful inject, one CLI test, README line.

**Deferred:** Add/remove archive members; keyboard shortcut “back to archive”; ModSync GUI work (PR #110).

## Key Technical Decisions

- Reuse existing `_open_path` reload path (probe + read + new `ContainerEditor`) rather than partial tree refresh — matches how opening archives already works.
- Clear inject context via `_clear_editor` when returning to archive (existing `_open_path` flow).

## Implementation Units

### U1. Dock post-inject navigation

**Goal:** Return user to refreshed container after successful save-back.

**Files:** `tools/godot-holocron/addons/kotor_holocron/ui/kotor_holocron_dock.gd`

**Approach:** After `inject_member` succeeds, call `_open_path(archive)` before clearing context (or rely on `_clear_editor` inside `_open_path`).

**Test scenarios:** Manual — open `sample.mod`, edit nested member, Save, verify container tree visible with updated status text.

**Verification:** Godot dock shows container editor and status mentions archive file after nested save.

### U2. Listing size regression test

**Goal:** Prove inject updates sizes exposed by `read` for containers.

**Files:** `src/KOTORModSync.Tests/KotorFormatBridgeCliTests.cs`

**Test scenarios:** Copy `sample.mod` to temp; `read` finds `test2da` entry; `inject` with `sample.2da` source; `read` again — `test2da` `size` equals `sample.2da` file length.

**Verification:** Test passes when PyKotor bridge available.

### U3. README

**Files:** `tools/godot-holocron/README.md`

**Verification:** Parity table mentions return-to-archive after nested save.

## Success Criteria

- [x] Successful nested save returns to parent archive editor
- [x] `Inject_UpdatesResourceListingSize` passes in CI when bridge runs
- [x] README updated
