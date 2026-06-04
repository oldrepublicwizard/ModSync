---
title: "test: Holocron bridge inject missing source file"
type: test
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# test: Holocron bridge inject missing source file

## Summary

Add `Inject_MissingSource_ReturnsError` when `--source` path does not exist.

## Success Criteria

- [x] Inject with missing source returns `ok: false` and path error
