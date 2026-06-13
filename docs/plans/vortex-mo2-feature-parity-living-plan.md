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
| 4 | Managed deployment | **In PR** (#158 core merged); install wiring + GUI toggle (plan 123) |
| 5 | File conflicts | Core #160 + GUI #165 merged |
| 6 | FOMOD | Parser + installer dialog merged (#166); archive hook deferred |
| 7 | (roadmap tail) | Per slice plans |

## Delta update (2026-06-14)

### Landed

- Managed deployment engine core (#158).
- Managed deployment install wiring (P0+P1): opt-in staging, deploy hooks, settings toggle, wizard/single-mod/CLI parity (plan 123, in PR).
- Nexus update badges + **Check for Nexus Updates** menu action (#167, plan 122).
- FOMOD installer dialog + parser stack (#166).
- Conflicts analysis GUI (#165).
- Nexus update check core (#156).

### Partial

- FOMOD: no automatic archive enumeration hook in download flow.
- Update checking: no endorsement UI; check results not persisted via `DownloadCacheService`.
- Desktop validation skipped for managed deployment + update badges (headless agent).
- Managed deployment: uninstall/purge GUI and dry-run staging parity deferred (P2).

### Next

1. FOMOD archive discovery hook in download/archive enumeration.
2. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable.
3. Managed deployment P2: dry-run/VFS staging parity or document install-only validation.

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
