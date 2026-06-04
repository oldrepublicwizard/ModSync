---
title: "feat: branch-aware open PR test script + AGENTS wiring"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/wizard-archive-validation-parity
  - feat/holocron-erf-nested-open
---

# feat: branch-aware PR test script (plan 058)

## Problem

Plan `057` added `test_pr110_validation.sh` and `test_pr111_holocron_bridge.sh`, but `AGENTS.md` still lists raw `dotnet test` filters. Agents on the wrong branch may run the wrong script manually.

## Scope

### In scope

- `scripts/agents/test_current_open_pr.sh` — detect branch, invoke correct PR script
- `AGENTS.md`, `copilot-instructions.md` — script column / pointer
- Plan `058` index on wizard + Holocron KB

### Out of scope

- Merging PRs, new product features

## Verification

```bash
./scripts/agents/test_current_open_pr.sh   # on each feature branch
```
