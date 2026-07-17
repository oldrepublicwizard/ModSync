---
title: "feat: multi-author publish/share flows"
status: stub
date: 2026-07-13
strategy_track: Multi-author builds
origin: STRATEGY.md + docs/knowledgebase/product-vision.md
---

# Plan stub 2026-07-13-003 — Multi-author publish/share

## Summary

Close the remaining multi-author gap: after merge tooling and install profiles, authors need a supported way to **publish and share** an instruction file (link or artifact) so players can open it via ModSync — not only the single canonical curator build.

## Current state `[REPO]`

| Piece | Status |
|-------|--------|
| `merge` CLI verb | Shipped |
| Install profiles | Shipped ([install-profiles.md](../knowledgebase/install-profiles.md)) |
| Guide emission (`GenerateModDocumentation`) | Shipped |
| Hosted publish / share UX | **Missing** |
| `modsync://` deep-link consume path | Phase 1 shipped; Phase 2 open — [002](2026-07-13-002-feat-modsync-protocol-os-registration-plan.md) |

## Proposed outcome (v1 sketch)

- Author exports a shareable instruction URL or packaged artifact from an existing instruction file.
- Player opens via `modsync://install?url=…` (depends on plan 002) or downloads then loads as today.
- No requirement to replace `mod-builds` as the community source of truth in v1 — focus on *any* author's encoded build being distributable.

## Non-goals (v1)

- Full Nexus Mods upload integration.
- Multiplayer collab editing.
- Replacing Deadly Stream / GitHub hosting — ModSync links *to* hosts, does not become the CDN.

## Next step

Flesh a brainstorm requirements doc (success criteria, auth/trust model for remote URLs, whether share is "copy link" only vs in-app publish) before a full ce-plan. Prefer sequencing **after** modsync:// consume (002) so share links have a working open path.
