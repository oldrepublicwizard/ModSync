---
title: "docs: KB closure for June 2026 rebrand and MainWindow extraction arcs"
type: docs
status: completed
date: 2026-06-04
origin: docs/knowledgebase/README.md
branch: docs/june-2026-arc-kb-closure
---

# docs: KB closure for June 2026 rebrand and MainWindow extraction arcs

## Summary

Plans 067–075 and PRs #120–#123 extend the rebrand closure and MainWindow service-extraction work, but `master` KB pages do not yet route agents to merged vs in-flight slices. Add a durable index without duplicating plan bodies.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | KB README lists merged June 2026 slices (067–071) and in-flight PRs (120–123) | Read README §June 2026 arcs |
| R2 | `rebrand-legacy-strings.md` links telemetry doc plans 068/073 and test plans 070–071 | Related plans + telemetry docs table |
| R3 | `gui-architecture-deferred.md` notes in-flight MainWindow extraction PRs without marking them merged | Deferred table shows Pending until on master |

## Scope boundaries

### In scope

- `docs/knowledgebase/README.md`
- `docs/knowledgebase/rebrand-legacy-strings.md`
- `docs/knowledgebase/gui-architecture-deferred.md`

### Out of scope

- Merging open PRs
- Editing plan files on feature branches

## Implementation units

### U1. KB README arc index

**Files:** `docs/knowledgebase/README.md`

**Approach:** Add `### June 2026 arcs (rebrand closure + MainWindow extraction)` under Plans with merged vs open-PR tables and links to KB pages.

**Test expectation:** none — documentation only.

### U2. Rebrand KB cross-links

**Files:** `docs/knowledgebase/rebrand-legacy-strings.md`

**Approach:** Add telemetry setup docs table (068, 073) and extend related plans through 071.

**Test expectation:** none — documentation only.

### U3. GUI deferred in-flight PRs

**Files:** `docs/knowledgebase/gui-architecture-deferred.md`

**Approach:** Add pending rows for plans 072–075 / PRs #120–#123 under MainWindow god object.

**Test expectation:** none — documentation only.
