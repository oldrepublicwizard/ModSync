---
title: "fix: Holocron nested archive member open path"
type: fix
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# fix: Holocron nested archive member open path

## Summary

`ContainerEditor._on_item_activated` emitted `member_open_requested` with an undefined `extracted` identifier; use the cache `output` path from `_extract_member_to_path`.

## Success Criteria

- [x] Signal passes the extracted member file path
- [x] Godot script parses without unresolved identifier
