# Worktree consolidation — 2026-07-13

Non-interactive cherry-pick consolidation of parallel `ModSync-*` agent worktrees into the main repo.

## Map (source commit → destination branch tip)

| Source worktree / commit | Destination | Tip SHA | Notes |
|---|---|---|---|
| `ModSync-fomod-gate-tests` `18254ddb` | `feat/fomod-configuration-gate` | `4bb9d2ca1f2e293103fba967ee7e67f786c9d792` | Already present as `16d8904a` (identical patch-id); tip advanced by concurrent FOMOD/serialization agents |
| `ModSync-gui-smoke-headless` `9165e691` | `feat/guide-paste-ingestion` | `105345712f402a098977e658f54917ab11f6a3f0` | Cherry-picked earlier as `3b470d9b`; paste tip later advanced by guide-quality integrate |
| `ModSync-managed-deploy-audit` `c02ee89c` | `fix/validation-progress-count` (from master) | `e51537efac94d6cd745367e50970b1f4b0589ad6` | Master-safe CountStages + progress test + plan 004; FOMOD-only UI tests omitted |
| same `c02ee89c` (full) | `feat/fomod-configuration-gate` | `4bb9d2ca1f2e293103fba967ee7e67f786c9d792` | Full cherry-pick → `6129e297`, then concurrent FOMOD advances |
| same `c02ee89c` (full) | `feat/guide-paste-ingestion` | `105345712f402a098977e658f54917ab11f6a3f0` | Full cherry-pick → `d38352c2` on stacked paste tip |
| `ModSync-guide-quality` `2dcfbb8c` (+ quality docs/simplify) | `feat/guide-paste-ingestion` | `105345712f402a098977e658f54917ab11f6a3f0` | Integrated second pass (see below) |
| `ModSync-modsync-protocol-wt` `b7e2d1d7` | `feat/modsync-protocol-phase1` (from **master**) | `283db0061c1fc3ff3b9c53cf283cb1afead91da6` | Rebased off FOMOD/paste stack onto master so #170 stays clean |

## Second pass (deferred items) — 2026-07-13 later

### Guide quality → `feat/guide-paste-ingestion`

Cherry-picked in order from `feat/guide-ingestion-quality` / `feat/guide-ingestion-coverage`:

| Source | New tip commit | Notes |
|---|---|---|
| `633178fa` vision docs | `55d16a4a` | Took shipped paste/modsync vision wording |
| `3818cdf9` brainstorms | `36531e43` | README July arcs section kept |
| `eeeb85e7` FOMOD shipped note | `98087ac5` | Clean |
| `75e23601` docs/agent parity + skip warnings | `8be0be42` | Kept richer GuiSmoke filter + added ingest rows |
| `ae1a1753` KB shipped markers | `a21003fe` | Added product-overview snapshot |
| `e05b0918` simplify paste paths | `5e9b7272` | **Conflict with sandbox `47c1abeb`:** kept `ApplyReviewFlag` + `IsSandboxedPath` harden; took `RequiresDestination` + shared NL parser reuse |
| `2dcfbb8c` draft quality / mod-builds NL tests | `10534571` | Clean auto-merge onto sandbox + richer tests |

Sandbox harden `47c1abeb` already in paste history; not re-applied.

### Protocol Phase 1 → master-based `feat/modsync-protocol-phase1`

- Detached protocol worktree, force-moved branch to `master` (`3b0792b1`), cherry-picked `b7e2d1d7` → `283db006`.
- Dropped `STRATEGY.md` / `docs/knowledgebase/product-vision.md` from the cherry-pick (modify/delete vs master — those files belong to the paste/docs arc).
- Kept protocol code, KB handler doc, plan 006, and tests.
- Old FOMOD-based tip preserved as `backup/modsync-protocol-phase1-fomod-base` → `b7e2d1d7` (local only; no push).

## Tests run

- `feat/guide-paste-ingestion` (first pass): GuiSmoke + FomodGate headless + ProgressTotalSteps / FomodConfigurationFailure — **13 passed**
- `feat/guide-paste-ingestion` (second pass): `GuideIngestionTests` — **27 passed** (sandbox + mod-builds NL coverage)
- `feat/modsync-protocol-phase1` (master-based tip): `ModSyncUrlTests` — **22 passed**
- `fix/validation-progress-count`: `ProgressTotalSteps` + `ValidationPipelineParityTests` — **passed**
- `feat/fomod-configuration-gate`: `ProgressTotalSteps` + FOMOD gate/failure filters — **passed** (Name~ filter)

## Remaining / reference worktrees

| Worktree | Branch tip | Notes |
|---|---|---|
| `ModSync-guide-quality` | `2dcfbb8c` / `feat/guide-ingestion-coverage` | Content integrated onto paste; worktree may lag |
| `ModSync-modsync-protocol-wt` | `283db006` / `feat/modsync-protocol-phase1` | Now tracks master-based tip |
| `ModSync-paste-sandbox-fix` | `47c1abeb` | Already in guide-paste history |
| `ModSync-serialization-fix` | holds `feat/fomod-configuration-gate` | Active FOMOD tip worktree (`4bb9d2ca`) |
| `ModSync-test-suite-triage` | `bd53b5f8` | Triage branch on older tip |
| Prunable legacy (`ModSync-conflicts`, `-deployment`, `-fomod`, `-profiles`, `-updates`) | various | Marked prunable; not touched |

## Stacking notes

- `feat/guide-paste-ingestion` remains stacked on FOMOD via merge `94f373cf` (+ recovery + smoke + CountStages + guide-quality commits). Prefer landing `feat/fomod-configuration-gate` (#170) before paste.
- `feat/modsync-protocol-phase1` is **not** stacked on FOMOD/paste — base is current `master` so protocol PRs do not pull #170.
- `fix/validation-progress-count` can land on master independently.
