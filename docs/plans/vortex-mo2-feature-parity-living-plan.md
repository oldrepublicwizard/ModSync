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
| 2 | Nexus update checks | **Merged** (#156 core, #167 GUI badges) |
| 3 | Profiles | Merged (#157) |
| 4 | Managed deployment | **Merged** (#158 core); install wiring deferred |
| 5 | File conflicts | Core #160 + GUI #165 merged |
| 6 | FOMOD | Parser + installer dialog merged (#166); archive hook deferred |
| 7 | (roadmap tail) | Per slice plans |

## Delta update (2026-06-14)

### Landed

- Managed deployment engine core (#158).
- Nexus update badges + **Check for Nexus Updates** menu action (#167, plan 122).
- FOMOD installer dialog + parser stack (#166).
- Conflicts analysis GUI (#165).
- Nexus update check core (#156).

### Partial

- Deployment: `DeploymentService` not wired into install execution; no GUI toggle.
- FOMOD: no automatic archive enumeration hook in download flow.
- Update checking: no endorsement UI; check results not persisted via `DownloadCacheService`.
- Desktop validation skipped for update badges (headless agent).

### Next

1. Wire `DeploymentService` into install execution + optional GUI toggle (plan 116 slice 2).
2. FOMOD archive discovery hook in download/archive enumeration.
3. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable.

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
