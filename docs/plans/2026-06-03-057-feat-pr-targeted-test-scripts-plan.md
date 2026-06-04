---
title: "feat: PR-targeted agent test scripts for #110 and #111"
type: feat
status: completed
date: 2026-06-03
branches:
  - feat/holocron-erf-nested-open
  - feat/wizard-archive-validation-parity
---

# feat: PR-targeted test scripts (plan 057)

## Problem

Docs reference `dotnet test` filters for #110 and #111, but agents still copy long commands. Small wrappers reduce merge-review friction.

## Scope

### In scope

- `scripts/agents/test_pr110_validation.sh` — presenter + mapper filters
- `scripts/agents/test_pr111_holocron_bridge.sh` — `KotorFormatBridgeCliTests`
- `scripts/agents/README.md` catalog + handoff/ci-test-matrix links
- Plan `057` index on both KB pages

### Out of scope

- Merging PRs, CI workflow changes

## Verification

```bash
./scripts/agents/test_pr110_validation.sh
./scripts/agents/test_pr111_holocron_bridge.sh
```
