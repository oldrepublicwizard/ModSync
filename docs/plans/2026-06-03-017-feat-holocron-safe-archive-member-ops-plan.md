---
title: "feat: Holocron safe archive member remove and add feedback"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-016-feat-holocron-archive-add-remove-members-plan.md
prerequisite: docs/plans/2026-06-03-016-feat-holocron-archive-add-remove-members-plan.md
---

# feat: Holocron safe archive member remove and add feedback

## Summary

Harden Phase 2 archive membership UX: confirm before destructive remove, surface replace-vs-add intent when adding an existing resref, and test bridge error when removing a missing member.

## Problem Frame

Add/remove landed in plan 016 without guardrails. One-click remove can delete the wrong row; adding `foo.2da` when `foo` already exists silently replaces via inject.

## Requirements

- R1. Container **Remove** shows a confirmation dialog naming `resref.restype` before calling `remove_member`.
- R2. **Add member** checks listing; status text notes when the operation will **replace** an existing member vs add new.
- R3. CLI test: `remove` on missing member returns `ok: false` with error (temp copy).
- R4. README one-line note on confirmed remove.

## Scope Boundaries

**In scope:** Godot confirmation, add feedback, one negative CLI test, README.

**Deferred:** Custom resref entry dialog, ModSync ValidatePage (#110), bulk operations.

## Implementation Units

### U1. Container remove confirmation and add replace hint

**Files:** `container_editor.gd`

### U2. Bridge negative test

**Files:** `KotorFormatBridgeCliTests.cs`

### U3. README

**Files:** `tools/godot-holocron/README.md`

## Success Criteria

- [x] Remove requires confirmation
- [x] Add shows replace hint when applicable
- [x] Missing-member remove test passes
- [x] README updated
