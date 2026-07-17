# ModSync knowledgebase

Canonical index for humans and coding agents working on this repository. Start here when routing a task or verifying how agent workflows map to product behavior.

## Index currency

**Last reviewed:** 2026-07-13.

Reflects shipped paste/guide ingestion, Avalonia headless GUI smoke (`GuiSmokeHeadlessTests`), and the FOMOD configure-before-validate/install gate. Before publishing a GitHub Release, use [release readiness checklist](../plans/2026-07-13-005-release-readiness-checklist.md) and [manual-release.md](../manual-release.md).

## Evidence labels

Use these when citing findings in plans, PRs, or audits:

| Label | Meaning |
|-------|---------|
| `[REPO]` | Observed from source, tests, or scripts in this repository |
| `[UI]` | Requires a real desktop Avalonia session |
| `[OFFICIAL]` | External vendor or upstream documentation |
| `[SYNTH]` | Synthesized conclusion from multiple sources |
| `[OPEN]` | Unverified or environment-dependent |

## Read order by task shape

| If you are… | Read first | Then |
|-------------|------------|------|
| New to the repo | [product-overview.md](product-overview.md) | `AGENTS.md` → `doc-hierarchy.md` |
| Authoring or fixing TOML | [instruction-format.md](instruction-format.md) | [mod-component-model.md](mod-component-model.md) |
| Debugging validation failures | [validation-pipeline.md](validation-pipeline.md) | [gui-validation-surfaces.md](gui-validation-surfaces.md) → [vfs-vs-real-fs.md](vfs-vs-real-fs.md) |
| Install wizard or real install | [install-lifecycle.md](install-lifecycle.md) | [validation-pipeline.md](validation-pipeline.md) |
| Missing mod archives / downloads | [download-system.md](download-system.md) | [install-lifecycle.md](install-lifecycle.md) |
| Headless build/test/core | `.github/copilot-instructions.md` | `core-cli-reference.md` |
| Avalonia GUI UX smoke (no X11) | `AGENTS.md` §Headless Avalonia GUI smoke | `./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~Headless\|FullyQualifiedName~GuiSmoke"` |
| GUI / install wizard / full-build | `docs/local_desktop_agent_runbook.md` | `agent-action-parity.md` → [install-lifecycle.md](install-lifecycle.md) |
| Release or versioning | `docs/manual-release.md` | [Release readiness checklist](../plans/2026-07-13-005-release-readiness-checklist.md) → `docs/solutions/manual-release-workflow.md` |
| Agent capability gaps | `agent-native-audit.md` | `agent-action-parity.md` |
| telemetry-auth sidecar | [telemetry-auth-routing.md](telemetry-auth-routing.md) → `telemetry-auth/README.md` | Do not use Avalonia runbooks |

## Topic index

### Domain model (start here for TOML and validation)

- [Product overview](product-overview.md) — what ModSync is, audiences, workflows
- [Product vision](product-vision.md) — origin, full vision, vision-vs-current-state gaps (see also root `STRATEGY.md`)
- [Guide ingestion](guide-ingestion.md) — paste/clipboard + `convert --stdin --parse-directions` draft instructions
- [Instruction file format](instruction-format.md) — path placeholders, action types, minimal examples
- [Mod component model](mod-component-model.md) — component/instruction fields, selection semantics
- [Validation pipeline](validation-pipeline.md) — five stages, fail-fast, CLI/GUI mapping
- [GUI validation surfaces](gui-validation-surfaces.md) — `ValidationPipelineDialogMapper`, wizard vs dialog UI ([PR #110](https://github.com/th3w1zard1/ModSync/pull/110))
- [GUI architecture (deferred)](gui-architecture-deferred.md) — MainWindow split, wizard hosts, scope guidance

### Workflows

- [Install lifecycle](install-lifecycle.md) — wizard page order, `InstallationService`, checkpoints, widescreen, CLI flags
- [Download system](download-system.md) — ResourceRegistry, handler order, `DownloadCacheService`, GUI vs CLI
- [Mod update checking](update-checking.md) — `NexusApiClient`, `ModUpdateCheckService`, mod list update badges, menu check action
- [Install profiles](install-profiles.md) — named loadouts, `ProfileService` capture/apply, `ProfileManagerDialog`
- [Managed deployment engine](managed-deployment.md) — hardlink deploy, per-component manifests, uninstall/purge (Phase 4 slice 1)
- [File-level conflicts](file-conflicts.md) — `FileConflictAnalyzer` dry-run VFS attribution and `ConflictsDialog` results UI
- [FOMOD installer support](fomod-support.md) — ModuleConfig parser, GUI/CLI post-download configure, `--fomod-choices`, configured-only gate
- [nxm protocol handler](nxm-protocol-handler.md) — nxm:// URL parsing, OS scheme registration, single-instance hand-off, free-user download path
- [modsync protocol handler](modsync-protocol-handler.md) — modsync:// build deep links (parse + CLI + handoff; OS registration planned)

### Architecture and agent parity

- [Documentation hierarchy](doc-hierarchy.md) — which doc is authoritative for what
- [Agent-native architecture audit](agent-native-audit.md) — scored review of eight agent-native principles
- [Agent action parity](agent-action-parity.md) — user/GUI flows vs CLI, scripts, and tests
- [Core CLI reference](core-cli-reference.md) — `ModBuildConverter` verbs (`validate`, `install`, `convert`, …)
- [CLI selection semantics](cli-selection-semantics.md) — install/validate vs TOML `IsSelected`, `--best-effort`
- [CI test matrix](ci-test-matrix.md) — GitHub Actions filters vs local `run_headless_tests.sh`
- [VFS vs real FS](vfs-vs-real-fs.md) — dry-run validation vs real install
- [HoloPatcher resources](holopatcher-resources.md) — Linux symlink and `Resources/` layout
- [mod-builds sources](mod-builds-sources.md) — agent clone vs community validation workflow
- [Removed features](removed-features.md) — distributed cache, stale test filters, deprecated skills
- [Drift audit 2026-05-24](drift-audit-2026-05-24.md) — research snapshot and fixes applied
- [telemetry-auth routing](telemetry-auth-routing.md) — sidecar scope and env vars
- [Rebrand legacy strings](rebrand-legacy-strings.md) — intentional `KOTORModSync` survivors after plan 065/066

### Runbooks (procedural)

- [Local desktop agent runbook](../local_desktop_agent_runbook.md) — GUI launch, wizard order, full-build flow
- [Manual release](../manual-release.md) — when and how to publish GitHub releases
- [Release readiness checklist](../plans/2026-07-13-005-release-readiness-checklist.md) — quadruple-check before `create_github_release=true`

### Agent scripts

- [scripts/agents/README.md](../../scripts/agents/README.md) — helper script catalog and when to use each

### Institutional learnings

- [docs/solutions/](../solutions/) — durable postmortems and patterns (prefer over chat history)
- [Manual release workflow](../solutions/manual-release-workflow.md)
- [Agent guidance layering](../solutions/agent-guidance-layering.md)

### Plans (implementation history)

- [docs/plans/](../plans/) — dated plans with requirements and status frontmatter; active parity tracker: [vortex-mo2-feature-parity-living-plan.md](../plans/vortex-mo2-feature-parity-living-plan.md); release gate: [2026-07-13-005-release-readiness-checklist.md](../plans/2026-07-13-005-release-readiness-checklist.md)

#### June 2026 arcs (rebrand closure + MainWindow extraction)

`[SYNTH]` Merged to `master` through PR #125 unless noted. Open PRs are CI-green slices waiting on merge — check `gh pr list` before assuming they landed.

**Merged on `master` (rebrand + legacy compat + extraction):**

| Plan | PR | Topic |
|------|-----|--------|
| 067 | [#115](https://github.com/th3w1zard1/ModSync/pull/115) | Rebrand KB + plan 065 footnotes |
| 068 | [#116](https://github.com/th3w1zard1/ModSync/pull/116) | Telemetry setup guides client paths |
| 069 | [#117](https://github.com/th3w1zard1/ModSync/pull/117) | `ModSync_Master.md` client strings |
| 070 | [#118](https://github.com/th3w1zard1/ModSync/pull/118) | Legacy settings path tests |
| 071 | [#119](https://github.com/th3w1zard1/ModSync/pull/119) | Legacy compat test completion |
| 073 | [#121](https://github.com/th3w1zard1/ModSync/pull/121) | `GITHUB_SECRET_SETUP.md` client env |
| 074 | [#122](https://github.com/th3w1zard1/ModSync/pull/122) | Remove dead service instantiations |
| 075 | [#123](https://github.com/th3w1zard1/ModSync/pull/123) | `SettingsService` directory pickers |
| 077 | [#125](https://github.com/th3w1zard1/ModSync/pull/125) | Headless `SettingsService` picker tests |

**Open PRs (tests + menus):**

| Plan | PR | Topic |
|------|-----|--------|
| 078 / 085 | [#133](https://github.com/th3w1zard1/ModSync/pull/133) | `SettingsService` picker hardening + edge cases |
| 072 / 082 | [#130](https://github.com/th3w1zard1/ModSync/pull/130) | `MenuBuilderService` wiring |
| 086 | [#134](https://github.com/th3w1zard1/ModSync/pull/134) | KB closure + agent routing (consolidates closed #131/#132) |
| 087 | [#135](https://github.com/th3w1zard1/ModSync/pull/135) | `InitializeTopMenu` → `MenuBuilderService` (stacks #130) |
| 088 | [#136](https://github.com/th3w1zard1/ModSync/pull/136) | `DownloadOrchestrationService` headless tests (`master`) |
| 090 / 091 / 098 | [#137](https://github.com/th3w1zard1/ModSync/pull/137) | `DownloadIndicatorUiHelper` + headless Getting Started indicators (`master`) |
| 094 | [#139](https://github.com/th3w1zard1/ModSync/pull/139) | `ValidationDisplayUiHelper` Getting Started summaries (`master`) |
| 096 | [#140](https://github.com/th3w1zard1/ModSync/pull/140) | `StepProgressUiHelper` + navigation tests (supersedes closed #138) |

Superseded duplicate PRs [#120](https://github.com/th3w1zard1/ModSync/pull/120)–[#129](https://github.com/th3w1zard1/ModSync/pull/129), closed #131/#132/#138 are closed or superseded. Suggested merge order: #133 → #130 → #134 → #135; #136–#140 can land on `master` independently.

KB routing: [rebrand-legacy-strings.md](rebrand-legacy-strings.md), [gui-architecture-deferred.md](gui-architecture-deferred.md).

#### July 2026 arcs (ingestion + FOMOD gate + release hygiene)

`[REPO]` Landed on branch work toward release (see recent commits on `feat/fomod-configuration-gate` and related):

| Topic | Status | Pointers |
|-------|--------|----------|
| Paste / guide ingestion | Shipped | [guide-ingestion.md](guide-ingestion.md); [product-vision.md](product-vision.md) 1b/1c; plan [001](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md) |
| Headless Avalonia GUI smoke | Shipped | `GuiSmokeHeadlessTests`; `AGENTS.md` §Headless Avalonia GUI smoke |
| FOMOD discovery + CLI prompts + gate | Shipped | [fomod-support.md](fomod-support.md); fail-closed until configured; Plan [123](../plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md) |
| `modsync://` Phase 1 (parse/CLI/handoff) | Shipped | [modsync-protocol-handler.md](modsync-protocol-handler.md) |
| `modsync://` Phase 2 (OS reg + consume) | Open / in flight | [brainstorm](../brainstorms/2026-07-13-modsync-protocol-requirements.md); [plan 006](../plans/2026-07-13-006-feat-modsync-protocol-os-registration-plan.md); PR #176 |
| Multi-author publish/share | Open | [plan stub 003](../plans/2026-07-13-003-feat-multi-author-publish-share-plan.md) |
| Release readiness checklist | Active | [005](../plans/2026-07-13-005-release-readiness-checklist.md) |

### Always-on rules (do not duplicate here)


- `.cursorrules` — path sandboxing, VFS, test naming, Avalonia gotchas
- `AGENTS.md` — routing layer and wizard control map
- `.github/copilot-instructions.md` — Copilot default inference and build commands

## Quick commands

```bash
# Wizard validation regression (presenter + mapper)
./scripts/agents/test_pr110_validation.sh

# Standard non-long-running tests
./scripts/agents/run_headless_tests.sh

# Avalonia headless GUI UX smoke (no X11)
./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~GuiSmokeHeadlessTests"

# Validate an instruction file (full validation needs game + mod dirs)
./scripts/agents/cli_validate.sh --input ./mod-builds/TOMLs/KOTOR1_Full.toml \
  --game-dir ./tmp/kotor_template --source-dir ./tmp/mod_downloads --full

# Best-effort headless full-list install (long-running; needs Nexus key for many mods)
./scripts/agents/install_best_effort.sh \
  ./mod-builds/TOMLs/KOTOR1_Full.toml ./tmp/kotor_template ./tmp/mod_downloads

# Launch GUI with preload args (desktop session required)
./scripts/agents/launch_gui_desktop.sh \
  --instruction-file ./mod-builds/TOMLs/KOTOR1_Full.toml \
  --kotor-dir ./tmp/kotor_template \
  --mod-dir ./tmp/mod_downloads
```

## What this knowledgebase is not

- Not a substitute for generated maps (`docs/ModSync_Master.md`, `docs/ModSync_Codebase_Map.json`) — use those for exhaustive symbol lookup, not routing.
- Not a promise that every GUI button has a headless equivalent — see the audit and parity docs for explicit gaps.
