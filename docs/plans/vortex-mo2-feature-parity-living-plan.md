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

Architecture blueprint (Vortex + MO2 + HoloPatcher, ports/adapters, U1–U10):
[2026-07-16-001-three-project-parity-architecture-plan.md](2026-07-16-001-three-project-parity-architecture-plan.md).

## Phase status

| Phase | Area | Status |
|-------|------|--------|
| 1 | nxm protocol handler | Merged (#155–#164) |
| 2 | Nexus update checks | **Merged** (#156 core, #167 GUI badges) |
| 3 | Profiles | Merged (#157) |
| 4 | Managed deployment | **Merged** (#158 core); install wiring deferred |
| 5 | File conflicts | Core #160 + GUI #165 merged |
| 6 | FOMOD | Parser + installer dialog merged (#166); GUI + CLI post-download **merged** (#169); fail-closed gate in #170 |
| 7 | (roadmap tail) | Per slice plans |

## Delta update (2026-06-14)

### Landed

- Managed deployment engine core (#158).
- Nexus update badges + **Check for Nexus Updates** menu action (#167, plan 122).
- FOMOD installer dialog + parser stack (#166).
- Conflicts analysis GUI (#165).
- Nexus update check core (#156).

### Partial

- Deployment: `DeploymentService` not wired into install execution; no GUI toggle (see PR #168).
- FOMOD post-download: GUI dialog + CLI orchestrator (`--fomod-choices`, settings `fomodPostDownloadMode`) **merged** (#169); validate/install gate still open (#170).
- Update checking: no endorsement UI; check results not persisted via `DownloadCacheService`.
- Desktop validation skipped for FOMOD prompts and update badges (headless agent).

### Next

1. Merge managed deployment install wiring (#168) when ready.
2. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable.
3. Managed deployment P2: dry-run/VFS staging parity or document install-only validation.

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
