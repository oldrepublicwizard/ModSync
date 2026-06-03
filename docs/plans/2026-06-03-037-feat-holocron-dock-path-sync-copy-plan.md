---
title: "feat: Holocron dock sync path field and copy path"
type: feat
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# feat: Holocron dock sync path field and copy path

## Summary

Keep the dock path field aligned with the open resource (including nested members) and add **Copy path** for agent/handoff workflows.

## Success Criteria

- [x] `_open_path` sets `PathEdit` on successful open
- [x] Nested member open updates `PathEdit` to the cache member path
- [x] **Copy path** toolbar copies the current path field
