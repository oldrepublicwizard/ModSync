---
title: "feat: ValidatePage auto-expand log when issues found"
type: feat
status: completed
date: 2026-06-03
origin: docs/plans/2026-06-03-027-feat-validatepage-copy-validation-report-plan.md
branch: feat/wizard-archive-validation-parity
---

# feat: ValidatePage auto-expand log when issues found

## Summary

Expand the validation log automatically when a run reports errors or warnings so users see step-by-step detail without an extra click.

## Success Criteria

- [x] Log expander opens on `_hasCriticalErrors` or `_warningCount > 0`
- [x] Clean pass leaves log collapsed (default)
