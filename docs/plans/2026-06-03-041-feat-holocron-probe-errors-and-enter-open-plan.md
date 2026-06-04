---
title: "feat: Holocron probe/read error tests + Enter opens member"
type: feat
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
prerequisite: docs/plans/2026-06-03-040-feat-holocron-inject-missing-source-test-plan.md
---

# feat: Holocron probe/read error tests + Enter opens member

## Summary

Regression tests for missing-path `probe`/`read` failures and keyboard Enter in the container tree to open the selected archive member (plan 041, PR #111).

## Success Criteria

- [x] `Probe_MissingPath_ReturnsError` and `Read_MissingPath_ReturnsError` in `KotorFormatBridgeCliTests`
- [x] Container editor `KEY_ENTER` / `KEY_KP_ENTER` triggers `_on_item_activated`
- [x] README test count updated to 22
