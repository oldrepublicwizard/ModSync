---
title: "docs: GUI validation surfaces and deferred architecture KB"
type: docs
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-003-refactor-validatepage-stage-message-parser-plan.md
---

# docs: GUI validation surfaces and deferred architecture KB

## Summary

After PRs #103–#104 unified pipeline → dialog mapping and shared `ERROR:`/`WARNING:` parsing for wizard conflicts, agents still lack a single KB page for GUI validation surfaces and larger deferred refactors (MainWindow, wizard hosts).

## Requirements

- R1. New KB page lists validation UI surfaces, `ValidationPipelineDialogMapper` responsibilities, and what remains wizard-specific in `ValidatePage`.
- R2. New KB page documents deferred architecture items with file pointers and safe scope guidance for PRs.
- R3. `validation-pipeline.md` and `README.md` link to the new pages.
- R4. No application code changes.

## Scope Boundaries

**In scope:** `docs/knowledgebase/*.md`, plan status.

**Deferred:** Code refactors listed as deferred only.

## Implementation Units

### U1. Author KB pages and index links

**Files:**
- New: `docs/knowledgebase/gui-validation-surfaces.md`
- New: `docs/knowledgebase/gui-architecture-deferred.md`
- Modify: `docs/knowledgebase/validation-pipeline.md`
- Modify: `docs/knowledgebase/README.md`

**Verification:** Markdown only; no `dotnet test` required.
