---
title: "feat: ValidatePage scroll log to first issue line"
type: feat
status: completed
date: 2026-06-03
branch: feat/wizard-archive-validation-parity
prerequisite: docs/plans/2026-06-03-033-feat-validatepage-scroll-to-first-issue-plan.md
---

# feat: ValidatePage scroll log to first issue line

## Summary

When validation reports errors or warnings, scroll the validation log to the first `ERROR:` / `❌` line (or first `WARNING:` / `⚠` when no errors), complementing scroll+highlight on result cards.

## Success Criteria

- [x] Log scroll runs after failed/warned validation completes
- [x] KB and deferred doc list plans 033–036 on PR #110 arc
