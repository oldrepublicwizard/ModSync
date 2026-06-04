---
title: "docs: agent tooling freeze + merge commands in handoff"
type: docs
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# docs: agent tooling freeze and merge commands (plan 060)

## Problem

Plan `059` completed verifier scripts, but the KB index **Quick commands** omit `verify_open_pr_ready.sh`, and the merge handoff lacks explicit `gh pr merge` examples for maintainers.

## Scope

### In scope

- `docs/knowledgebase/README.md` — pre-merge verify in Quick commands + Active PRs
- `parallel-pr-merge-handoff-2026-06-03.md` — `gh pr merge` examples, tooling freeze note
- `.github/copilot-instructions.md` — prefer `verify_open_pr_ready.sh`
- Plan `060` index on wizard + Holocron KB

### Out of scope

- Executing merges (maintainer action)
- New product code

## Verification

```bash
./scripts/agents/verify_open_pr_ready.sh
```
