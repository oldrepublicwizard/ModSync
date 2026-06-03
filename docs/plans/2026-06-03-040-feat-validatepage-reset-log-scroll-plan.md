---
title: "feat: ValidatePage reset log scroll on new validation"
type: feat
status: completed
date: 2026-06-03
branch: feat/wizard-archive-validation-parity
prerequisite: docs/plans/2026-06-03-039-feat-validatepage-reset-results-scroll-plan.md
---

# feat: ValidatePage reset log scroll on new validation

## Summary

Reset the validation log `ScrollViewer` to the top when a new run starts (pairs with plan 039 results scroll reset).

## Success Criteria

- [x] `RunValidation` sets log scroll offset to zero before appending new log lines
