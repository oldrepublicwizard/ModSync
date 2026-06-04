---
title: "feat: ValidatePage copy validation report"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-026-feat-dialog-mapper-environment-installorder-prefixed-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: ValidatePage copy validation report

## Summary

Finish the in-progress **Copy report** control on the install wizard ValidatePage so users can copy summary counts, result cards, and the validation log for bug reports and agent handoff.

## Requirements

- R1. Button visible after a validation run completes.
- R2. Clipboard text includes summary, per-result title/message, and full log.
- R3. Re-run clears prior results list; copy uses latest run only.
- R4. Brief KB note; no `.cursor/hooks` in commit.

## Success Criteria

- [x] Copy report works via TopLevel clipboard API
- [x] Builds on existing XAML `CopyReportButton`
