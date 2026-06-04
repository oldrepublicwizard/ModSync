---
title: "feat: verify_open_pr_ready.sh (tests + gh pr checks)"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# feat: verify_open_pr_ready.sh (plan 059)

## Problem

Agents run `test_current_open_pr.sh` but still merge with red or pending CI. A single verifier should run local PR tests then surface GitHub check status for #110 / #111.

## Scope

### In scope

- `scripts/agents/verify_open_pr_ready.sh` — `test_current_open_pr.sh` + `gh pr checks` for mapped PR
- Wire into `scripts/agents/README.md`, merge handoff, `.cursorrules`
- Update handoff doc plan arc ceiling to `058` → `059`
- Plan index on both KB pages

### Out of scope

- Merging PRs, CI workflow changes

## Verification

```bash
./scripts/agents/verify_open_pr_ready.sh
```

Requires `gh` authenticated for CI leg; local tests run without `gh`.
