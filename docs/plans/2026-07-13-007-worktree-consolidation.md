# Worktree consolidation — 2026-07-13

Non-interactive cherry-pick consolidation of parallel `ModSync-*` agent worktrees into the main repo.

## Map (source commit → destination branch tip)

| Source worktree / commit | Destination | Tip SHA | Notes |
|---|---|---|---|
| `ModSync-fomod-gate-tests` `18254ddb` | `feat/fomod-configuration-gate` | `6129e297a5b07f8013334e7e42465199cce0d613` | Already present as `16d8904ad6006d6e67dc9d0de02fa64b48614572` (identical patch-id); no new cherry-pick |
| `ModSync-gui-smoke-headless` `9165e691` | `feat/guide-paste-ingestion` | `647205782e6b7a4e0302e43b2975c9951db39ef1` | Merged FOMOD tip into guide-paste, then cherry-picked as `3b470d9b1f171d43bb5ccc2c9aa116bae7256ce2`; conflicts resolved in KB README + DownloadsExplainPage |
| `ModSync-managed-deploy-audit` `c02ee89c` | `fix/validation-progress-count` (from master) | `e51537efac94d6cd745367e50970b1f4b0589ad6` | Master-safe CountStages + progress test + plan 004; FOMOD-only UI tests omitted |
| same `c02ee89c` (full) | `feat/fomod-configuration-gate` | `6129e297a5b07f8013334e7e42465199cce0d613` | Full cherry-pick `c02ee89c658321acebb645cff9c82821df782e82` → `6129e297` on live FOMOD tip |
| same `c02ee89c` (full) | `feat/guide-paste-ingestion` | `647205782e6b7a4e0302e43b2975c9951db39ef1` | Full cherry-pick → `d38352c2` on stacked paste tip |

## Tests run

- `feat/guide-paste-ingestion`: GuiSmoke + FomodGate headless + ProgressTotalSteps / FomodConfigurationFailure — **13 passed**
- `fix/validation-progress-count`: `ProgressTotalSteps` + `ValidationPipelineParityTests` — **passed**
- `feat/fomod-configuration-gate`: `ProgressTotalSteps` + FOMOD gate/failure filters — **passed** (Name~ filter)

## Other `ModSync-*` worktrees (not integrated this pass)

| Worktree | Branch tip | Reason |
|---|---|---|
| `ModSync-guide-quality` | `2dcfbb8c` / `feat/guide-ingestion-coverage` | Separate guide-quality arc; includes `e05b0918` simplify-paste refactor — leave for dedicated stack |
| `ModSync-modsync-protocol-wt` | `b7e2d1d7` `feat/modsync-protocol-phase1` | Protocol Phase 1; keep independent of paste/FOMOD landing |
| `ModSync-paste-sandbox-fix` | `47c1abeb` | Already merged into guide-paste history |
| `ModSync-serialization-fix` | holds `feat/fomod-configuration-gate` | Active FOMOD tip worktree |
| `ModSync-test-suite-triage` | `bd53b5f8` | Triage branch on older tip |
| Prunable legacy (`ModSync-conflicts`, `-deployment`, `-fomod`, `-profiles`, `-updates`) | various | Marked prunable; not touched |

## Stacking notes

- `feat/guide-paste-ingestion` is stacked on FOMOD via merge commit `94f373cf` (plus later FOMOD recovery commits), then GUI smoke + CountStages + string-literal sync commits.
- Prefer landing `feat/fomod-configuration-gate` before `feat/guide-paste-ingestion`.
- `fix/validation-progress-count` can land on master independently (small CountStages bugfix).
