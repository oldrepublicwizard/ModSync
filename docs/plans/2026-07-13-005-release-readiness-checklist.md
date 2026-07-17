---
title: "docs: release readiness quadruple-check checklist"
type: docs
status: active
date: 2026-07-13
origin: release push after FOMOD gate + paste ingestion + GuiSmokeHeadlessTests
related:
  - docs/manual-release.md
  - docs/plans/2026-07-13-002-release-gap-inventory.md
  - docs/knowledgebase/product-overview.md
  - STRATEGY.md
  - docs/knowledgebase/product-vision.md
---

# Plan 2026-07-13-005 — Release readiness checklist

## Summary

Quadruple-check gate before cutting the next intentional GitHub Release (see [docs/manual-release.md](../manual-release.md)). Each row must pass **four** independent confirmations: (1) command/evidence run, (2) docs match reality, (3) tests or headless proxies green, (4) no known P0 from the gap inventory.

Do **not** set `create_github_release=true` until every section below is checked.

## Quadruple-check matrix

### 1. Build

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 Solution builds Debug | `dotnet build ModSync.sln --configuration Debug` exits 0 | local log |
| Q2 GUI framework target | GUI builds with `--framework net9.0` | same |
| Q3 No secret/config drift in release inputs | Version files align (`MainConfig`, manifest, plists) | `ReleaseVersionAlignment` |
| Q4 Docs still say manual release only | [manual-release.md](../manual-release.md) + this checklist linked | `[REPO]` |

### 2. Tests (non-LongRunning)

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 Headless suite | `./scripts/agents/run_headless_tests.sh` exits 0 | local log |
| Q2 Exclude LongRunning only | Filter `FullyQualifiedName!~LongRunning` | CI + local |
| Q3 Wizard validation regression | `./scripts/agents/test_pr110_validation.sh` | script |
| Q4 FOMOD gate unit coverage | `dotnet test … --filter "FullyQualifiedName~FomodConfigurationGate"` | tests |

### 3. Headless GUI smoke

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 GuiSmoke filter green | `./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~GuiSmokeHeadlessTests"` | Avalonia.Headless |
| Q2 Paste-import control present | `ImportFromClipboardButton` exercised in smoke | `[REPO]` `GuiSmokeHeadlessTests` |
| Q3 Page-0 / Welcome layout constraints | Welcome + Landing scroll smoke green | same |
| Q4 ValidatePage log splitter smoke | Log expander/splitter assertions green | same |

Desktop polish and full-build installs remain `[UI]` and are **not** replaced by this section.

### 4. FOMOD configuration gate

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 Gate blocks validate/install until configured | `FomodConfigurationGate` reject path covered | `[REPO]` gate + tests |
| Q2 Unreadable archives fail closed | Enumerate failure ≠ non-FOMOD bypass | `[REPO]` `FomodArchiveProbe` / gate tests |
| Q3 Missing mod directory fails closed | Gate does not proceed without mod dir | tests |
| Q4 KB matches behavior | [fomod-support.md](../knowledgebase/fomod-support.md) post-download + gate section current | docs |

CLI TTY FOMOD wizard (Plan 123) may still be open — document as known gap, not a silent pass.

### 5. Paste / guide ingestion

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 GUI clipboard import | Getting Started / MainWindow paste entry → format sniff | `[REPO]` `ImportFromTextAsync` |
| Q2 Draft instructions from prose | `DraftInstructionService` + `NaturalLanguageInstructionParser` | `[REPO]` + `GuideIngestionTests` |
| Q3 Path sandboxing on drafts | Draft paths only `<<modDirectory>>` / `<<kotorDirectory>>` | tests |
| Q4 Vision table current | product-vision 1b/1c marked shipped | [product-vision.md](../knowledgebase/product-vision.md) |

### 6. CLI

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 Convert `--stdin` ingest | Guide/instruction content from stdin | `[REPO]` `ModBuildConverter` |
| Q2 `--parse-directions` drafts | Draft + review flag in output | same + tests |
| Q3 Validate / install still work | `cli_validate.sh` smoke on a small TOML | scripts |
| Q4 Parity doc lists paste + CLI | [agent-action-parity.md](../knowledgebase/agent-action-parity.md), [core-cli-reference.md](../knowledgebase/core-cli-reference.md) | docs — update if stale |

### 7. Docs

| Check | Pass criteria | Evidence |
|-------|---------------|----------|
| Q1 KB index currency | [knowledgebase/README.md](../knowledgebase/README.md) dated / July arcs listed | this release push |
| Q2 Manual release cross-link | [manual-release.md](../manual-release.md) → this checklist | docs |
| Q3 Product overview workflows | Paste + headless GUI smoke listed | [product-overview.md](../knowledgebase/product-overview.md) |
| Q4 Product vision vs current state | Matches shipped paste / NL drafts / headless smoke | [product-vision.md](../knowledgebase/product-vision.md) |

### 8. Strategy tracks (`STRATEGY.md`)

| Track | Release expectation | Pass when |
|-------|---------------------|-----------|
| Guide ingestion | Paste + draft NL instructions shipped; fidelity still iterative | 1b/1c shipped in vision table; parser gaps tracked as P2+ |
| Guide emission | Already shipped (`GenerateModDocumentation`) | Round-trip tests still green |
| "Install with ModSync" entry points | `nxm://` shipped; `modsync://` explicitly deferred | No accidental claim that `modsync://` exists |
| Multi-author builds | Merge + profiles shipped; publish/share deferred | Vision table row 4 stays Partial |

## Pre-publish gate (after all Qs)

1. Merge release branch / feature work to `master`.
2. Optional Release Please version PR.
3. Build and Release with `create_github_release=false` first (artifact smoke).
4. Re-run Q1–Q3 of Build + Tests against the published artifact if practical.
5. Only then `create_github_release=true`.

## Out of scope for this checklist

- Cutting the actual GitHub Release (human workflow dispatch).
- Closing every P2/P3 from the gap inventory.
- Full-build desktop install of `KOTOR1_Full.toml` / `KOTOR2_Full.toml` (tracked separately under full-build skills).
