---
title: "test: Holocron bridge extract missing archive member"
type: test
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# test: Holocron bridge extract missing archive member

## Summary

Add `Extract_MissingMember_ReturnsError` CLI regression test mirroring `Remove_MissingMember_ReturnsError`.

## Success Criteria

- [x] Extract of nonexistent resref returns `ok: false` with not-found error
