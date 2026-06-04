---
title: "feat: ValidatePage reset results scroll on new validation"
type: feat
status: completed
date: 2026-06-03
branch: feat/wizard-archive-validation-parity
---

# feat: ValidatePage reset results scroll on new validation

## Summary

When **Run Validation** starts, reset the results `ScrollViewer` offset so a prior run's scroll position does not hide new cards.

## Success Criteria

- [x] `RunValidation` clears results scroll to top before rebuilding cards
