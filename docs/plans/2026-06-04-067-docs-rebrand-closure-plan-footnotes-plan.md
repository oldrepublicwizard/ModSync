---
title: "docs: rebrand closure — plan footnotes and legacy string inventory"
type: docs
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-066-refactor-holopatcher-namespace-rebrand-plan.md
branch: docs/rebrand-closure-footnotes
---

# docs: rebrand closure — plan footnotes and legacy string inventory

## Summary

Close the ModSync + HoloPatcher rebrand documentation arc. Plan 066 shipped HoloPatcher namespace renames but deferred plan 065 footnote updates. Add a durable KB inventory of intentional `KOTORModSync` survivors in source (migration paths, JSON keys) so `rg KOTORModSync` outside `docs/plans/*` is explainable.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | Plan 065 gap-fill no longer lists HoloPatcher namespaces as unchanged | Read 065 §Gap-fill |
| R2 | Plan 065 references plan 066 completion for HoloPatcher namespaces | Cross-link present |
| R3 | KB documents intentional `KOTORModSync` strings in `src/ModSync.*` | `docs/knowledgebase/rebrand-legacy-strings.md` |
| R4 | `rg KOTORModSync` outside plans hits only documented intentional code | Manual rg + KB table match |

## Scope boundaries

### In scope

- `docs/plans/2026-06-03-065-refactor-rebrand-kotormodsync-to-modsync-plan.md` footnote updates
- `docs/knowledgebase/rebrand-legacy-strings.md` (new)
- `docs/knowledgebase/README.md` link to legacy strings page

### Deferred to follow-up work

- Mass-editing historical `docs/plans/*` migration narratives (065/066 keep `KOTORModSync` where documenting before/after)
- Renaming `telemetry.kotormodsync.com`, `KOTORMODSYNC_SIGNING_SECRET`, DeadlyStream slug

### Out of scope

- Source code changes to migration paths
- Telemetry-auth Docker image renames

## Implementation units

### U1. Update plan 065 gap-fill footnote

**Goal:** Reflect HoloPatcher namespace rebrand completion.

**Files:** `docs/plans/2026-06-03-065-refactor-rebrand-kotormodsync-to-modsync-plan.md`

**Approach:** Move HoloPatcher from "Intentionally unchanged" to "Completed in plan 066". Keep DNS/secret/slug unchanged list.

**Test expectation:** none — documentation only.

**Verification:** 065 accurately describes current repo state post-#114.

### U2. Add rebrand legacy strings KB page

**Goal:** Single source of truth for intentional `KOTORModSync` in non-plan tracked files.

**Files:** `docs/knowledgebase/rebrand-legacy-strings.md`, `docs/knowledgebase/README.md`

**Approach:** Table listing file, string, reason (settings migration, JSON root key read compat, telemetry path fallback).

**Test expectation:** none — documentation only.

**Verification:** Table rows match `rg KOTORModSync --glob '!docs/plans/**'` output.

### U3. Mark plan 067 completed and verify rg gate

**Goal:** Confirm rebrand grep posture documented.

**Files:** this plan file (status flip at end of ce-work)

**Verification:**

```bash
rg -n 'KOTORModSync' --glob '!docs/plans/*'
```

Expect only the four documented migration/compat lines in Core/GUI.
