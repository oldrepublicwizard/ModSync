---
title: "feat: ValidatePage flush log queue in report builder"
type: feat
status: completed
date: 2026-06-03
branch: feat/wizard-archive-validation-parity
---

# feat: ValidatePage flush log queue in report builder (plan 043)

`BuildValidationReportText()` calls `FlushLogQueue()` so batched lines are included in `--- Log ---` regardless of caller.
