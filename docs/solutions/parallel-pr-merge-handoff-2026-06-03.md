# Parallel PR merge handoff (#110 + #111)

`[REPO]` Durable steps for landing the two merge-ready feature branches without bundling them. Routing context: [doc-hierarchy.md](../knowledgebase/doc-hierarchy.md#active-open-prs-2026-06-03), test filters in [ci-test-matrix.md](../knowledgebase/ci-test-matrix.md#pr-targeted-local-filters-merge-ready-open-prs).

## Branches

| PR | Branch | Primary paths |
|----|--------|----------------|
| [#110](https://github.com/th3w1zard1/ModSync/pull/110) | `feat/wizard-archive-validation-parity` | `src/KOTORModSync.GUI/` (ValidatePage, validation services), validation KB |
| [#111](https://github.com/th3w1zard1/ModSync/pull/111) | `feat/holocron-erf-nested-open` | `tools/godot-holocron/`, `KotorFormatBridgeCliTests` |

No shared implementation files between the feature arcs — either PR can merge first.

## Pre-merge (both PRs)

1. Confirm CI green on the PR you are merging.
2. Run `./scripts/agents/test_pr110_validation.sh` or `./scripts/agents/test_pr111_holocron_bridge.sh` (or the filters in [ci-test-matrix.md](../knowledgebase/ci-test-matrix.md#pr-targeted-local-filters-merge-ready-open-prs)).
3. Do not squash Holocron commits into #110 or validation commits into #111.

## Merge order

**Recommended:** Merge **#110** first, then **#111** (validation docs are authoritative on the wizard branch; Holocron branch carries a synced copy of `gui-validation-surfaces.md` for agent routing only).

**Alternate:** Merge **#111** first if Holocron is higher priority — no code conflict expected; only `docs/` overlap possible.

## After the first merge

1. Rebase the remaining feature branch onto the updated default branch:

   ```bash
   git fetch origin
   git checkout feat/<remaining-branch>
   git rebase origin/<default-branch>
   ```

2. Resolve **documentation-only** conflicts by keeping the merged default for shared agent docs (`AGENTS.md`, `doc-hierarchy.md`, `docs/knowledgebase/README.md`) and re-applying any track-specific KB if needed.
3. Re-run the remaining PR's test filter; push with lease if history was rewritten.
4. Confirm CI green on the second PR before merging.

## After both merge

- Close plan arc `012`–`056` / `013`–`056` — no further LFG doc slices required for merge readiness.
- Holocron Phase 2: new branch from default per [2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md](../plans/2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md).
- Wizard structural debt: [gui-architecture-deferred.md](../knowledgebase/gui-architecture-deferred.md) (MainWindow split, wizard hosts) — separate future PRs.

## Related

- [agent-guidance-layering.md](agent-guidance-layering.md)
- [gui-validation-surfaces.md](../knowledgebase/gui-validation-surfaces.md)
- [godot-holocron-editor.md](../knowledgebase/godot-holocron-editor.md)
