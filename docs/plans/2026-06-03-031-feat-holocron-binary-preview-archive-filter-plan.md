---
title: "feat: Holocron binary preview and archive filter"
type: feat
status: completed
date: 2026-06-03
branch: feat/holocron-erf-nested-open
---

# feat: Holocron binary preview and archive filter

## Summary

Improve nested archive workflows: filter members in the container tree, and show a read-only hex preview for binary resources (MDL/TPC/WAV, etc.) instead of raw base64 JSON.

## Requirements

- R1. `text_editor` renders `format: binary` as hex preview; save disabled.
- R2. Container toolbar filter narrows listing by resref/restype substring.
- R3. Bridge CLI test for binary read payload; README updated.

## Success Criteria

- [x] Binary members open with readable preview
- [x] Filter works on large archives
- [x] Tests pass
