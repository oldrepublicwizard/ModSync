---
title: "feat: merge_open_prs.sh (dry-run default, --execute to merge)"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# feat: merge_open_prs.sh (plan 061)

## Problem

Handoff doc lists `gh pr merge` commands but no scripted sequence. After plan `060` tooling freeze, the next actionable slice is a **safe merge helper** (dry-run by default).

## Scope

### In scope

- `scripts/agents/merge_open_prs.sh` — print sequence; `--execute` merges #110; `--execute-all` completes #110 + rebase/push/merge #111
- Fix handoff `origin/master` branch name
- `scripts/agents/README.md` catalog
- Plan `061` index (Holocron KB; wizard deferred table)

### Out of scope

- Auto-merge #111 without rebase (maintainer step)
- Further doc-only LFG slices

## Verification

```bash
./scripts/agents/merge_open_prs.sh
./scripts/agents/verify_open_pr_ready.sh
```
