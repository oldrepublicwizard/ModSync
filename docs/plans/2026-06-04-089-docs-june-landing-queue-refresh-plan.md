---
title: "docs: refresh June 2026 landing queue (#134–#136)"
status: shipped
pr: "#134"
stacks_on: "#134"
---

# docs: refresh June 2026 landing queue (#134–#136)

## Problem

`AGENTS.md`, copilot instructions, and KB README still referenced "KB/routing docs PR" generically and omitted open PRs #135 (`InitializeTopMenu`) and #136 (`DownloadOrchestrationService` tests).

## Solution

Update landing queue text and KB arc table with explicit PR links and merge order: #133 → #130 → #134 → #135; #136 independent on `master`.

## Verification

Docs-only; review links resolve to open PRs.
