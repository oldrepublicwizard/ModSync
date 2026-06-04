# Parallel PR merge handoff (#110 + #111)

**Status (2026-06-04):** PR [#110](https://github.com/th3w1zard1/ModSync/pull/110) **merged**. PR [#111](https://github.com/th3w1zard1/ModSync/pull/111): branch merged `origin/master` locally (plan `062`); push + `gh pr merge 111` pending CI on updated branch.

`[REPO]` Durable steps for landing the two merge-ready feature branches without bundling them. Routing context: [doc-hierarchy.md](../knowledgebase/doc-hierarchy.md#active-open-prs-2026-06-03), test filters in [ci-test-matrix.md](../knowledgebase/ci-test-matrix.md#pr-targeted-local-filters-merge-ready-open-prs).

## Branches

| PR | Branch | Primary paths |
|----|--------|----------------|
| [#110](https://github.com/th3w1zard1/ModSync/pull/110) | `feat/wizard-archive-validation-parity` | `src/KOTORModSync.GUI/` (ValidatePage, validation services), validation KB |
| [#111](https://github.com/th3w1zard1/ModSync/pull/111) | `feat/holocron-erf-nested-open` | `tools/godot-holocron/`, `KotorFormatBridgeCliTests` |

No shared implementation files between the feature arcs — either PR can merge first.

## Pre-merge (both PRs)

1. On the feature branch, run `./scripts/agents/verify_open_pr_ready.sh` (local tests + `gh pr checks`), or separately confirm CI green and run `test_current_open_pr.sh`.
2. Do not squash Holocron commits into #110 or validation commits into #111.

## Merge order

**Recommended:** Merge **#110** first, then **#111** (validation docs are authoritative on the wizard branch; Holocron branch carries a synced copy of `gui-validation-surfaces.md` for agent routing only).

**Alternate:** Merge **#111** first if Holocron is higher priority — no code conflict expected; only `docs/` overlap possible.

### GitHub merge (maintainer)

Use merge commits (not a single combined PR):

```bash
./scripts/agents/merge_open_prs.sh          # dry-run sequence
./scripts/agents/merge_open_prs.sh --execute   # merge #110 after verify (wizard branch)
# then rebase feat/holocron-erf-nested-open onto origin/master, verify, push, and:
gh pr merge 111 --merge
```

## After the first merge

1. Rebase the remaining feature branch onto the updated default branch:

   ```bash
   git fetch origin
   git checkout feat/<remaining-branch>
   git rebase origin/master
   ```

2. Resolve **documentation-only** conflicts by keeping the merged default for shared agent docs (`AGENTS.md`, `doc-hierarchy.md`, `docs/knowledgebase/README.md`) and re-applying any track-specific KB if needed.
3. Re-run the remaining PR's test filter; push with lease if history was rewritten.
4. Confirm CI green on the second PR before merging.

## After both merge

- Close plan arc `012`–`060` / `013`–`060` — agent tooling frozen; no further LFG doc-only slices required for merge readiness.
- Holocron Phase 2: new branch from default per [2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md](../plans/2026-06-03-047-feat-holocron-phase2-deferred-editors-plan.md).
- Wizard structural debt: [gui-architecture-deferred.md](../knowledgebase/gui-architecture-deferred.md) (MainWindow split, wizard hosts) — separate future PRs.

## Related

- [agent-guidance-layering.md](agent-guidance-layering.md)
- [gui-validation-surfaces.md](../knowledgebase/gui-validation-surfaces.md)
- [godot-holocron-editor.md](../knowledgebase/godot-holocron-editor.md)
