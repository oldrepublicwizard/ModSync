# ModSync knowledgebase

Canonical index for humans and coding agents working on this repository. Start here when routing a task or verifying how agent workflows map to product behavior.

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
| GUI / install wizard / full-build | `docs/local_desktop_agent_runbook.md` | `agent-action-parity.md` → [install-lifecycle.md](install-lifecycle.md) |
| Release or versioning | `docs/manual-release.md` | `docs/solutions/manual-release-workflow.md` |
| Agent capability gaps | `agent-native-audit.md` | `agent-action-parity.md` |
| telemetry-auth sidecar | [telemetry-auth-routing.md](telemetry-auth-routing.md) → `telemetry-auth/README.md` | Do not use Avalonia runbooks |

## Topic index

### Domain model (start here for TOML and validation)

- [Product overview](product-overview.md) — what ModSync is, audiences, workflows
- [Instruction file format](instruction-format.md) — path placeholders, action types, minimal examples
- [Mod component model](mod-component-model.md) — component/instruction fields, selection semantics
- [Validation pipeline](validation-pipeline.md) — five stages, fail-fast, CLI/GUI mapping
- [GUI validation surfaces](gui-validation-surfaces.md) — `ValidationPipelineDialogMapper`, wizard vs dialog UI ([PR #110](https://github.com/th3w1zard1/ModSync/pull/110))
- [GUI architecture (deferred)](gui-architecture-deferred.md) — MainWindow split, wizard hosts, scope guidance

### Workflows

- [Install lifecycle](install-lifecycle.md) — wizard page order, `InstallationService`, checkpoints, widescreen, CLI flags
- [Download system](download-system.md) — ResourceRegistry, handler order, `DownloadCacheService`, GUI vs CLI

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

### Agent scripts

- [scripts/agents/README.md](../../scripts/agents/README.md) — helper script catalog and when to use each

### Institutional learnings

- [docs/solutions/](../solutions/) — durable postmortems and patterns (prefer over chat history)
- [Manual release workflow](../solutions/manual-release-workflow.md)
- [Agent guidance layering](../solutions/agent-guidance-layering.md)

### Plans (implementation history)

- [docs/plans/](../plans/) — dated plans with requirements and status frontmatter

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

**Open PRs (tests + menus + KB):**

| Plan | PR | Topic |
|------|-----|--------|
| 078 | [#126](https://github.com/th3w1zard1/ModSync/pull/126) | Harden `SettingsService` picker tests |
| 072 / 079 | [#127](https://github.com/th3w1zard1/ModSync/pull/127) | `MenuBuilderService` wiring (supersedes [#120](https://github.com/th3w1zard1/ModSync/pull/120)) |

KB routing: [rebrand-legacy-strings.md](rebrand-legacy-strings.md), [gui-architecture-deferred.md](gui-architecture-deferred.md).

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
