---
title: "modsync:// Install with ModSync deep links"
status: ready-for-plan
date: 2026-07-13
strategy_track: "Install with ModSync" entry points
related: docs/knowledgebase/modsync-protocol-handler.md
plan: docs/plans/2026-07-13-002-feat-modsync-protocol-os-registration-plan.md
---

# modsync:// Install with ModSync deep links

## Summary

Finish the `"Install with ModSync"` track: OS registration for `modsync://` plus MainWindow consumption of buffered handoff URLs (fetch instruction file over http(s) and load into the normal instruction pipeline).

## Problem Frame

Phase 1 is shipped: URL parse (`ModSyncUrl`), CLI `--modsync=` / bare positional, single-instance pipe forward, and `ModSyncHandoffQueue` buffering ([modsync-protocol-handler.md](../knowledgebase/modsync-protocol-handler.md)). Browsers and the GUI still cannot complete a one-click open: there is no OS scheme registration and nothing drains the queue to fetch/load the instruction file.

## Requirements

**Already satisfied (Phase 1) — do not re-litigate**

- R0a: `modsync://install|open` with `url`/`instruction` http(s) targets; optional `game=kotor|kotor2`.
- R0b: Reject `file://` and bare local paths.
- R0c: CLI + single-instance enqueue into `ModSyncHandoffQueue`.

**Phase 2 — open**

- R1: Register/unregister `modsync://` on Windows/Linux (settings toggle); macOS via `CFBundleURLTypes` in `ModSync.app` (mirror nxm).
- R2: MainWindow (or `ModSyncHandoffService`) drains `ModSyncHandoffQueue`, parses via `ModSyncUrl.TryParse`, fetches the instruction URL over http(s), and loads through the existing instruction-file / paste cascade.
- R3: Failures (bad URL, network, parse) surface a clear dialog/log and do not crash startup.
- R4: KB + `product-vision` row 2 updated when Phase 2 ships; agent path remains `convert`/`install -i` with a downloaded file.

## Success Criteria

- Clicking a `modsync://install?url=https://…/build.toml` link (after OS registration) opens ModSync and loads that instruction set.
- `--modsync=` / secondary-instance forward already works; Phase 2 makes the primary instance *consume* the queue.
- Invalid local paths never fetch or write outside the intended download cache.

## Scope Boundaries

**In scope:** OS registration, settings toggle, queue drain/fetch/load, tests, KB.

**Out of scope:** Publish/share hosting ([plan stub 003](../plans/2026-07-13-003-feat-multi-author-publish-share-plan.md)); changing Phase 1 URL grammar; Nexus downloads inside `modsync://` (use `nxm://`).

## Key Decisions

- Reuse nxm registration/handoff patterns (`NxmProtocolRegistrationService`, `NxmHandoffService`).
- http and https both allowed for instruction URLs (matches current `ModSyncUrl` tests).
- Prefer sequencing publish/share *after* Phase 2 so share links have a working open path.

## Dependencies / Assumptions

- Phase 1 code under `src/ModSync.Core/Services/Protocol/` and GUI handoff types remain stable.
- Guide paste / draft-instruction path applies when fetched content is markdown.

## Outstanding Questions

- Whether v1 auto-advances the install wizard after load or only loads into Getting Started / wizard page 0 (same as `--instructionFile=` today is fine).

## Plan

[docs/plans/2026-07-13-002-feat-modsync-protocol-os-registration-plan.md](../plans/2026-07-13-002-feat-modsync-protocol-os-registration-plan.md) — deepen/execute from this requirements doc.
