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
| 4 | Managed deployment | **Merged** — engine (#158) + install wiring (#176) + CLI overrides (#177) + uninstall/purge GUI (#179) + patcher live-file provenance (#181); classic DryRun caveat documented (#180) |
| 5 | File conflicts | Core #160 + GUI #165 merged |
| 6 | FOMOD | Parser + installer dialog (#166); GUI + CLI post-download (#169); **fail-closed gate merged** (#170); living-plan sync (this update / U7) |
| 7 | (roadmap tail) | Per slice plans |

## Delta update (2026-07-17 / U7)

### Landed

- Managed deployment engine core (#158).
- Nexus update badges + **Check for Nexus Updates** menu action (#167, plan 122).
- FOMOD installer dialog + parser stack (#166).
- Conflicts analysis GUI (#165).
- Nexus update check core (#156).
- Three-project ports + managed install wiring (#176).
- Guide paste / draft instructions (#171); guide ports on master (U9).
- CLI `--managed` / `--no-managed` / `--profile` process-local overrides (#177 / U3).
- Per-mod **Uninstall Managed Deployment** + **Deployed** badge (#179 / U5).
- Managed validation decision B — classic DryRun does not model staging (#180 / U4).
- Patcher live game-file hash snapshot → manifests (#181 / U6; CAS restore for in-place overwrites deferred).
- FOMOD fail-closed `FomodConfigurationGate` (#170 / U7 code); KB `fomod-support.md` already documents gate behavior.
- `modsync://` Phase 2 consume + OS registration builders (#176; Settings checkbox deferred) (U8).

### Partial

- U6 residual: CAS pre-image restore when patchers overwrite existing game files in place.
- U8 residual: Settings UI toggle for `modsync://` OS registration + conflict probe polish.
- Update checking: no endorsement UI; check results not persisted via `DownloadCacheService`.
- Desktop validation skipped for FOMOD prompts and update badges (headless agent).

### Next

1. Residual gaps only — no open parity phase gates.
2. Pick one residual unit: Settings `modsync://` toggle, U6 CAS restore, or a high-signal agent-native gap from the latest audit.
3. Migrate `NexusModsDownloadHandler` to `NexusApiClient` when download handler branch is stable (U10).

## Superseded

- PR #162 closed; superseded by #166 (FOMOD dialog includes parser stack).
- PR #168 closed; superseded by #176 managed wiring.
