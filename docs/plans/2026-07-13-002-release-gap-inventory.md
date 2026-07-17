---
title: "RELEASE_GAP_LIST — ModSync release readiness inventory"
type: docs
status: active
date: 2026-07-13
origin: non-interactive release-readiness scan (brainstorms, plans, STRATEGY, KB vision, git, open PRs)
related:
  - docs/plans/2026-07-13-005-release-readiness-checklist.md
  - docs/manual-release.md
  - STRATEGY.md
  - docs/knowledgebase/product-vision.md
  - docs/plans/vortex-mo2-feature-parity-living-plan.md
---

# RELEASE_GAP_LIST — 2026-07-13

Ruthless inventory of what still blocks an intentional GitHub Release (see [docs/manual-release.md](../manual-release.md)). Current published tag remains **v2.0.0a1** (pre-release, 2025-11-12); `MainConfig.CurrentVersion` is still `2.0.0a1`.

**Scan snapshot (2026-07-13):**

| Source | Finding |
|--------|---------|
| Branch under scan | Local work sits on `feat/fomod-configuration-gate` lineage; working tree also named `feat/modsync-protocol-handler` with uncommitted `modsync://` WIP |
| vs `origin/master` | **7 commits** not on master: FOMOD gate (3) + vision/paste/smoke (4) |
| Open PRs | **#170** MERGEABLE + CI green (gate only on remote); **#168** CONFLICTING (managed deployment wiring) |
| Active plans | `001` paste ingestion, `123` FOMOD CLI prompts, living Vortex/MO2 plan, `005` release checklist |
| Brainstorms | FOMOD CLI reqs `reviewed`; archive discovery `reviewed`; mod-builds + GUI/CLI pipeline `completed` |
| AGENTS.md June queue | Stale — lists #133–#140 as in-flight; those PRs are already merged |

**Kind key:** `docs` = documentation-only · `code` = implementation · `both` = docs + code must move together.

---

## P0 — Ship blockers

Do not cut a release (`create_github_release=true`) until these are closed or explicitly waived with release notes.

| ID | Gap | Kind | Evidence | Suggested owner slice |
|----|-----|------|----------|------------------------|
| P0-1 | **FOMOD configured-only gate not on `master`** — dismiss/warned/skip can still validate/install without wizard completion on master | code | PR [#170](https://github.com/bodencrouch/ModSync/pull/170) OPEN, MERGEABLE, CI green; commits `7b27a582`…`b09a4e7e`; gate code in `FomodConfigurationGate.cs` + `InstallationValidationPipeline`; living plan Phase 6 partial | Land #170 alone (no paste/`modsync://` baggage). Desktop: skip FOMOD → validation fails; configure → passes |
| P0-2 | **Release candidate branch is dirty / mixed-scope** — uncommitted wizard XAML churn, FOMOD host tweaks, and **half-built `modsync://`** (`Protocol/`, `ModSyncHandoffQueue`, `ModSyncUrlTests`) on top of unreleased gate+paste commits | both | `git status` on `feat/modsync-protocol-handler`; untracked `src/ModSync.Core/Services/Protocol/`, `ModSyncUrlTests.cs`; STRATEGY/`product-vision` still say `modsync://` is future | Stash or branch-split: (A) #170 gate-only, (B) paste/vision PR, (C) `modsync://` WIP park. Zero uncommitted noise on the cut branch |
| P0-3 | **Paste + vision commits not on remote / not in #170** — four local commits ahead of `origin/feat/fomod-configuration-gate`; PR body only lists gate commits | both | `git log origin/feat/fomod-configuration-gate..HEAD` → `625cb41b`…`653c7771`; `gh pr view 170` commits = gate trio only | Open separate PR for paste/NL drafts/headless smoke **or** explicitly exclude from this release |
| P0-4 | **PR #170 desktop validation unchecked** — gate correctness for real Fetch Downloads → dismiss → Validate is unproven in desktop session | code | PR #170 test plan: desktop checkbox open; living plan notes desktop skipped for FOMOD prompts | One desktop pass per `AGENTS.md` / `launch_gui_desktop.sh` with a FOMOD fixture |
| P0-5 | **Docs claim state the tree does not match** — vision table still says paste **Missing** / NL parser **Dead code**; FOMOD KB still "Planned: CLI post-download" while orchestrator + `--fomod-choices`/`--fomod-skip` exist on master via #169 | docs | `docs/knowledgebase/product-vision.md` rows 1b/1c; `docs/knowledgebase/fomod-support.md` §Planned; `agent-action-parity.md` still "GUI-only until Plan 123"; Plan `123` still `status: active` | Docs honesty pass before release notes: mark CLI FOMOD post-download shipped (with known gaps), update 1b/1c only for what actually merges |

---

## P1 — Must-fix before release

Not necessarily merge-blockers for every niche feature, but must be true for a credible player-facing cut.

| ID | Gap | Kind | Evidence | Suggested owner slice |
|----|-----|------|----------|------------------------|
| P1-1 | **Close or re-scope Plan 123** — plan + brainstorm still active/`reviewed` while Core CLI hosts already shipped in #169; gate (R16–R21) is the remaining correctness slice (#170) | both | `docs/plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md` `status: active`; brainstorm `2026-06-14-fomod-cli-download-prompts-requirements.md`; code: `FomodPostDownloadOrchestrator`, `FomodCliPostDownloadHosts`, `FomodCliPostDownloadIntegrationTests` | Mark Plan 123 shipped-with-caveats after #170; list residual open questions (TTY wizard polish, `install -y` interaction) as P2 |
| P1-2 | **Plan 001 paste ingestion still `active`; checklist expects vision table shipped** — desktop paste OS-clipboard still Partial; `.cursor` plan todos disagree with code (`u2-nlparser` pending while `DraftInstructionService` wires the parser) | both | `docs/plans/2026-07-13-001-…`; `agent-action-parity.md` ImportFromClipboard Partial; `.cursor/plans/modsync_vision_and_ingestion_*.plan.md` stale todos | Finish plan/checklist/vision sync; one real-desktop paste smoke; mark 001 completed or narrow remaining gaps |
| P1-3 | **AGENTS.md / KB June 2026 landing queue is obsolete** — agents still told to prefer merging #133→#140 | docs | `AGENTS.md` §June 2026 in-flight; `docs/knowledgebase/README.md` June arcs table; `gh pr view` shows #133/#130/#140 merged | Replace with July open set: #170 (land), #168 (conflicted/defer), note superseded June queue |
| P1-4 | **Version / release machinery still on `2.0.0a1`** — next public tag needs intentional Release Please + Build and Release | both | `MainConfig.CurrentVersion`; `docs/manual-release.md`; `gh release list` → v2.0.0a1 pre-release | Decide version (e.g. `2.0.0a2` or first non-alpha); dry-run `create_github_release=false` first |
| P1-5 | **Full-build pass-rate metric undated** — STRATEGY tracks full-build validate/install as a key metric; no release-gate evidence that `KOTOR1_Full`/`KOTOR2_Full` validate cleanly with current FOMOD gate semantics | code | `STRATEGY.md` Key metrics; brainstorms `2026-05-29-*` completed but LongRunning/full archives still deferred; living plan desktop skipped | Scoped full-build validate (selected subset or documented known-fail list) before calling release "player ready" |
| P1-6 | **Checklist `005` not yet executed** — quadruple-check matrix exists but is itself uncommitted WIP | docs | `docs/plans/2026-07-13-005-release-readiness-checklist.md` (untracked at scan time); references this inventory | Commit checklist; run every Q-row; link from `manual-release.md` |

---

## P2 — Polish (post-gate / same release train if cheap)

| ID | Gap | Kind | Evidence | Suggested owner slice |
|----|-----|------|----------|------------------------|
| P2-1 | **Managed deployment install wiring stuck** — core engine merged (#158); install path + GUI toggle not on master | code | PR [#168](https://github.com/bodencrouch/ModSync/pull/168) CONFLICTING; living plan Phase 4 "install wiring deferred" | Rebase/resolve #168 or close and re-slice; **not** required for classic direct-to-game default |
| P2-2 | **Managed deployment dry-run/VFS staging parity** | code | Living plan Next §3 | Document install-only validation **or** small staging slice |
| P2-3 | **Nexus update-check leftovers** — no endorsement UI; results not persisted via `DownloadCacheService`; handler not on `NexusApiClient` | code | Living plan Partial / Next | Follow-up after badges (#167) |
| P2-4 | **FOMOD installer UX gaps** — no plugin images; `dependencyType` only `defaultType`; deferred in KB | code | `docs/knowledgebase/fomod-support.md` Deferred; brainstorm archive discovery deferred list | Image/advanced conditionals slice |
| P2-5 | **In-validate / CLI `fomod configure` recovery** — gate errors point to Fetch Downloads only | both | Brainstorm R20 deferred; Plan 123 out of scope | Recovery verb + ValidatePage action |
| P2-6 | **Guide-ingestion fidelity** — NL drafts are review-flagged; import fidelity metric not measured against `./mod-builds` guides | both | `STRATEGY.md` Guide import fidelity; Plan 001 U4 | Corpus eval harness; tighten parser rules |
| P2-7 | **Wizard page layout / log splitter polish** — partial work already in local commits; more XAML churn uncommitted | code | Commits `0efc1724`, `653c7771`; dirty Welcome/Installing/ModSelection pages | Stabilize layout on paste PR only; drop unrelated XAML from release branch |
| P2-8 | **Plan 118 status string "shipped (PR pending)"** — ambiguous closure | docs | `docs/plans/2026-06-14-118-nxm-phase6-download-ui-plan.md` | Mark completed with merge PR # or reopen |

---

## P3 — Later (explicitly out of this release)

| ID | Gap | Kind | Evidence | Suggested owner slice |
|----|-----|------|----------|------------------------|
| P3-1 | **`modsync://` protocol scheme** — STRATEGY future work; WIP already started in dirty tree | both | `STRATEGY.md` Install with ModSync; `product-vision.md` row 2; untracked `ModSyncUrl.cs` / tests | Dedicated protocol-handler plan after park of current WIP |
| P3-2 | **Publish/share flows for multi-author builds** | both | Vision row 4 Partial; STRATEGY Multi-author track | Authoring/distribution epic |
| P3-3 | **Expanded Ending Overhaul + M4-78EP community validation target** | docs | `STRATEGY.md` Milestones (undated, no work unit) | Encode as named full-build once owner exists |
| P3-4 | **Extra non-TTY FOMOD modes** (`fail-fast`, `auto-dismiss-all`) | code | Brainstorm deferred; Plan 123 KTD-4 reserves enum values | Settings follow-up |
| P3-5 | **CLI download scope = selected-only** | code | Brainstorm deferred | Download-system change |
| P3-6 | **macOS code signing / notarization** | code | Plan 119 out of scope | Release ops later |
| P3-7 | **Headless API / MCP inside desktop app** | docs | Brainstorm "Outside this product's identity" | Keep scripts/CLI as agent path |

---

## Brainstorm triage

| File | Status | Release relevance |
|------|--------|-------------------|
| `docs/brainstorms/2026-06-14-fomod-cli-download-prompts-requirements.md` | `reviewed` | **P0/P1** — gate R16–R21 = #170; remainder mostly shipped in #169 |
| `docs/brainstorms/2026-06-14-fomod-archive-discovery-requirements.md` | `reviewed` | Shipped via #169 GUI discovery; deferred CLI/validation items absorbed by Plan 123 / #170 |
| `docs/brainstorms/2026-05-29-mod-builds-pipeline-requirements.md` | `completed` | Baseline shipped; LongRunning full archives still environment-dependent (P1-5) |
| `docs/brainstorms/2026-05-29-gui-cli-unified-pipeline-requirements.md` | `completed` | Baseline shipped |

---

## Active vs done plans (release-relevant)

| Plan | Status | Note |
|------|--------|------|
| `2026-07-13-001` guide paste ingestion | **active** | Code largely present locally; not on master |
| `2026-07-13-005` release checklist | **active** | Companion gate; run before publish |
| `2026-06-14-123` FOMOD CLI prompts | **active** (stale) | Implementation largely on master (#169); close after #170 docs sync |
| `vortex-mo2-feature-parity-living-plan` | **active** | Phases 1–5 largely merged; Phase 4 wiring + FOMOD gate remain |
| Most `2026-06-03` / `2026-06-04` / June NXM plans | completed/shipped | June AGENTS queue should stop listing them as in-flight |

---

## STRATEGY / product-vision gap map

| Track / vision row | Claimed state in docs | Repo reality (scan) | Priority |
|--------------------|----------------------|---------------------|----------|
| Guide ingestion — paste (1b) | Missing | Implemented on unreleased commits (`ImportFromClipboardButton`, `ImportFromTextAsync`) | P0-3 / P1-2 |
| Guide ingestion — NL drafts (1c) | Dead code | `DraftInstructionService` references parser | P0-5 / P1-2 |
| Guide emission (3) | Shipped | Shipped | — |
| Entry points (2) | Partial (`nxm://` yes, `modsync://` no) | Accurate; dirty WIP must not ship half-done | P0-2 / P3-1 |
| Multi-author (4) | Partial | Accurate | P3-2 |
| Full-build pass rate | Metric defined, undated | No release evidence package | P1-5 |

---

## Open PR decision table

| PR | Mergeable | Role for release |
|----|-----------|------------------|
| [#170](https://github.com/bodencrouch/ModSync/pull/170) FOMOD configuration gate | MERGEABLE, CI green | **Merge first** (P0-1). Keep scope gate-only |
| [#168](https://github.com/bodencrouch/ModSync/pull/168) managed deployment wiring | CONFLICTING | **Defer** (P2-1). Classic install remains default |

June landing PRs (#130–#140 etc.) cited in `AGENTS.md` are **not** open release work — refresh the routing doc (P1-3).

---

## Recommended cut order

1. Clean working tree; park `modsync://` WIP on its own branch (P0-2).
2. Merge **#170** to `master` after desktop FOMOD gate smoke (P0-1, P0-4).
3. PR paste/vision/smoke as a **second** merge (P0-3) **or** waive from this tag with notes.
4. Docs honesty pass (P0-5, P1-1, P1-3).
5. Run checklist `005` + headless suite (P1-6).
6. Optional Release Please bump → Build and Release `create_github_release=false` → then `true` (P1-4).

**Do not** wait on #168, `modsync://`, publish/share, or Ending Overhaul encoding to ship a classic-install release.
