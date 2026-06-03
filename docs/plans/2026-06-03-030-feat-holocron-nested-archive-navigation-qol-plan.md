---
title: "feat: Holocron nested archive navigation QoL"
type: feat
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# feat: Holocron nested archive navigation QoL

## Summary

When editing an extracted archive member, add **Back to archive** in the dock and **Copy listing** on the container toolbar for agent-friendly handoff.

## Requirements

- R1. Dock shows Back to archive while nested inject context is active; returns to parent listing without save.
- R2. Container editor copies resref/type/size TSV to clipboard.
- R3. Holocron README note.

## Success Criteria

- [x] Nested edit can return to parent archive in one click
- [x] Listing copy works from container toolbar
