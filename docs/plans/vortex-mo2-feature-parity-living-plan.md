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
| 4 | Managed deployment | **Engine** (#158) + **install wiring** on `feat/three-project-parity-foundation` (via `IInstallBackend`; see #168) |
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

- Deployment: install wiring landed on parity-foundation branch (`IInstallBackend` /
  `ManagedInstallSession`); uninstall/purge GUI and managed VFS dry-run still open.
- FOMOD post-download: GUI dialog + CLI orchestrator (`--fomod-choices`, settings `fomodPostDownloadMode`) **merged** (#169); validate/install gate still open (#170).
- Update checking: no endorsement UI; check results not persisted via `DownloadCacheService`.
- Desktop validation skipped for FOMOD prompts and update badges (headless agent).

### Next

1. Use Core ports from new GUI/CLI work (`docs/plans/2026-07-16-001-three-project-parity-architecture-plan.md`).
2. Land FOMOD fail-closed gate (#170 / U7) without bypassing managed install path.
3. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable.
4. Managed deployment P2: dry-run/VFS staging parity or document install-only validation (U4).
5. `modsync://` Settings preference + conflict probe (consume + OS registration builders landed on `feat/three-project-parity-foundation`).
6. Per-component uninstall from mod list UI; patcher provenance (U6); CLI `--profile` parity (U3).

### Partial (this branch)

- **U8 consume:** `ModSyncHandoffQueue` / CLI `--modsync` / MainWindow fetch+load; `ModSyncProtocolRegistrationService` builders + Register/Unregister.
- **U5 purge:** Tools + context menu call `ManagedDeploymentLifecycle` / `DeploymentService.PurgeAsync`; status indicator dialog.

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
