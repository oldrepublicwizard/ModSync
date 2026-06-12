---
title: "Vortex/MO2 feature parity — living plan"
type: living-plan
status: active
date: 2026-06-14
origin: Vortex/MO2 feature-parity roadmap (phases 1–7)
---

# Vortex/MO2 feature parity — living plan

Single authoritative tracker for parity work. Individual slice plans under
`docs/plans/` remain the implementation record; this file holds status and next steps only.

## Phase status

| Phase | Area | Status |
|-------|------|--------|
| 1 | nxm protocol handler | Merged (#155–#164) |
| 2 | Nexus update checks | Core merged (#156); **GUI badges merged** (plan 122) |
| 3 | Profiles | Merged (#157) |
| 4 | Managed deployment | PR #158 rebased; **merge when CI green**; install wiring deferred |
| 5 | File conflicts | Core #160 + GUI #165 merged |
| 6 | FOMOD | Parser + installer dialog merged (#166); archive hook deferred |
| 7 | (roadmap tail) | Per slice plans |

## Delta update (2026-06-14)

### Landed

- FOMOD installer dialog + parser stack (#166).
- Conflicts analysis GUI (#165).
- Nexus update check core (#156).
- **Update badges + "Check for Nexus Updates" menu action** (plan 122, branch `feat/update-badges-gui`).

### Partial

- Managed deployment engine (#158): rebased on master, CI pending → merge when green; no install-path wiring yet.
- FOMOD: no automatic archive enumeration hook in download flow.
- Update checking: no endorsement UI, no cache persistence of check results.

### Next

1. Merge PR #158 when CI is green.
2. Wire `DeploymentService` into install execution + optional GUI toggle (plan 116 slice 2).
3. FOMOD archive discovery hook in download/archive enumeration.
4. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable.

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
