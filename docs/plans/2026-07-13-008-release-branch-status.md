---
title: "RELEASE_BRANCH_STATUS — tip inventory and post-#170 order"
type: docs
status: active
date: 2026-07-13
---

# Release branch tip inventory (2026-07-13)

Snapshot of local feature tips vs `origin` (no force-push). Open PRs at snapshot: **#170** (`feat/fomod-configuration-gate`), **#168** (`feat/managed-deployment-wiring`).

## Recommended next PR order after #170

1. **Land #170** — push local tip `4bb9d2ca` (8 commits ahead of `origin` `b09a4e7e`) and merge when CI green.
2. **`fix/validation-progress-count`** (`e51537ef`) — small master-safe CountStages fix; independent of paste.
3. **`feat/guide-paste-ingestion`** (`10534571`) — stacked on FOMOD tip via merge `94f373cf` + guide-quality/sandbox/smoke; open PR targeting master after #170 (rebase/merge master as needed).
4. **`feat/modsync-protocol-phase1`** (`283db006`) — master-based `modsync://` Phase 1 (not mixed into #170); open independent PR.
5. **`docs/release-gap-inventory`** (`31cb0df1`) — gap inventory + consolidation + this status doc.
6. **`test/2026-07-13-suite-triage`** — master test harness hardening; can land anytime (does not block product PRs).
7. **#168** managed-deployment wiring — keep after FOMOD/paste unless urgently needed; still open.

Reference-only / do not open as release blockers: `feat/guide-ingestion-coverage` / `feat/guide-ingestion-quality` (folded into paste), `backup/modsync-protocol-phase1-fomod-base` (local backup of old protocol+FOMOD tip), prunable conflict/deploy/fomod/profiles/updates worktrees.

## Tip map

| Branch | Tip | Push status | What's in it |
|---|---|---|---|
| `master` | `3b0792b1` | ahead=1 behind=1 (origin `bd53b5f8`) | Local/master diverge — reconcile carefully before release cut |
| `feat/fomod-configuration-gate` | `4bb9d2ca` | ahead=8 (origin `b09a4e7e`) | PR **#170** — FOMOD configure gate + serialization/recovery/tests |
| `feat/guide-paste-ingestion` | `10534571` | NOT_PUSHED | Paste ingest + sandbox + FOMOD stack + guide-quality `2dcfbb8c` (same patch-id as `10534571`) + headless smoke |
| `feat/modsync-protocol-phase1` | `283db006` | NOT_PUSHED | `modsync://` Phase 1 on **master** base (cherry-pick of `b7e2d1d7`) |
| `docs/release-gap-inventory` | `31cb0df1` | NOT_PUSHED | Release gap inventory, readiness checklist, worktree consolidation |
| `fix/validation-progress-count` | `e51537ef` | NOT_PUSHED | CountStages / progress total alignment |
| `test/2026-07-13-suite-triage` | `36839166`+ | NOT_PUSHED | MainConfig races, FileWatcher skips, Inter fonts, SettingsService ctor |
| `feat/managed-deployment-wiring` | `09eb6160` | synced | PR **#168** |
| `feat/file-conflict-analyzer` | `ba999e64` | synced | File-level conflict analyzer |
| `feat/install-profiles` | `ec760acb` | behind 2 | MO2-style profiles |
| `feat/nexus-update-checks` | `d587f707` | synced | Nexus update checks |
| `feat/managed-deployment-engine` | `2ed79f7e` | synced | Hardlink deployment engine |
| `feat/fomod-installer` | `d1e816b4` | synced | ModuleConfig parser / Option mapper |
| `feat/guide-ingestion-coverage` | `2dcfbb8c` | NOT_PUSHED | Source tip; content on paste |
| `feat/guide-ingestion-quality` | `e05b0918` | NOT_PUSHED | Simplify + docs lineage folded into paste |
| `fix/paste-draft-path-sandbox` | `47c1abeb` | NOT_PUSHED | Sandbox harden; in paste history |
| `test/expand-avalonia-gui-smoke-headless` | `9165e691` | NOT_PUSHED | GuiSmoke expansion; in paste history |
| `audit/managed-deployment-validation-2026-07-13` | `c02ee89c` | NOT_PUSHED | CountStages + managed-deploy plan; split to fix + FOMOD |
| `backup/modsync-protocol-phase1-fomod-base` | `b7e2d1d7` | NOT_PUSHED | **Do not push** — old protocol tip on FOMOD/docs base |
| `feat/nxm-*` / Holocron / June landing / settings tests | various | mostly synced | Prior arcs; not on critical 2026-07-13 release path |

## Push candidates (safe, no force)

| Branch | Why |
|---|---|
| `feat/fomod-configuration-gate` | Updates open #170 |
| `feat/guide-paste-ingestion` | New PR after #170 |
| `feat/modsync-protocol-phase1` | Independent Phase 1 PR |
| `docs/release-gap-inventory` | Docs PR |
| `fix/validation-progress-count` | Small fix PR |
| `test/2026-07-13-suite-triage` | Optional test harness PR |

Do **not** force-push. Do **not** push `backup/modsync-protocol-phase1-fomod-base`.
