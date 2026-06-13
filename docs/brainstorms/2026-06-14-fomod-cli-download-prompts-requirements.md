---
title: "CLI FOMOD post-download prompts"
status: reviewed
date: 2026-06-14
origin: docs/brainstorms/2026-06-14-fomod-archive-discovery-requirements.md
supersedes_deferred: CLI download/install parity from GUI FOMOD discovery slice
---

# CLI FOMOD post-download prompts

## Summary

After CLI download phases on `install`, `convert`, and `merge`, detect FOMOD installers in downloaded archives and offer the same configuration outcomes as the GUI post-download hook. Interactive terminals run a full step wizard; non-interactive environments warn and continue by default, with global configuration via CLI flags, environment variables, and ModSync settings.

## Problem Frame

PR #169 added GUI post-download FOMOD detection and optional configuration, but headless `install -d` and `convert -d` workflows still download FOMOD archives without surfacing installer choices. Agents and power users running full-build CLI installs hit validation/install with mapper defaults only, diverging from GUI-configured `Choose` options and instructions.

## Requirements

**Post-download detection and scope**

- R1: After CLI download completes on `install -d`, `convert -d`, and `merge -d`, scan **selected** components' downloaded archive files in the mod/source directory using `FomodArchiveProbe` (entry listing, no full extract for detection).
- R2: When `fomod/ModuleConfig.xml` is found and `FomodDownloadPromptState.ShouldPrompt` is true, run the post-download FOMOD flow for that archive.
- R3: Component scope matches GUI: only components with `IsSelected == true` at the hook point (after selection filters are applied for each verb).

**Interactive TTY behavior**

- R4: When stdin is an interactive TTY, ask Yes/No to configure now; **No** calls `MarkDismissed` (GUI parity).
- R5: On **Yes**, extract the archive, run the **full** FOMOD wizard in the terminal (all install steps, groups, and plugin choices) using the same presenter rules as `FomodInstallerDialog`, then merge into the existing component via `FomodConfiguredComponentMerger` and `MarkConfigured`.

**Non-interactive behavior**

- R6: When stdin is not an interactive TTY (CI, agents, redirected I/O), default mode is **warn and continue**: emit a structured `WARN:` line per detected FOMOD archive and proceed without persisting dismiss state.
- R7: Global `--fomod-skip` (and equivalent env/settings) persists `MarkDismissed` for archives handled under that skip policy in the session.
- R8: Non-interactive mode is configurable globally via environment variable, CLI startup/global flags, and ModSync settings dialog; v1 ships the `warn-continue` mode but the resolution pipeline must accept future modes without redesign.

**Agent / automation path**

- R9: Non-interactive runs may supply a FOMOD choices sidecar file (CLI flag and `MODSYNC_FOMOD_CHOICES` env) so cloud agents can configure without TTY; when present, apply selections and `MarkConfigured` without prompting.

**Persistence by verb**

- R10: On `convert -d` (and `merge -d` when output is written), after FOMOD configuration, persist merged component state (options, instructions, `HandlerMetadata` including `fomodPromptStatus`) to the instruction output path.
- R11: On `install -d`, FOMOD changes apply in-memory for the current run only unless the user sets an explicit output/save flag documented in the plan.

**Shared orchestration**

- R12: Core owns post-download FOMOD orchestration; GUI and CLI are thin hosts over the same service (unified pipeline parity with validation).
- R13: Headless `FomodInstallerPresenter` lives in Core so CLI does not reference Avalonia.

**Correctness fixes bundled in this slice**

- R14: Applying wizard selections must not merge plugins from steps that are no longer visible after condition-flag changes (hidden-step parity with visible validation).
- R15: Re-configuring the same archive updates existing option selections and replaces superseded FOMOD-generated instructions instead of silently skipping duplicate option GUIDs.

## Success Criteria

- Headless `install -d` against a fixture with a FOMOD archive prompts on TTY, applies choices, and proceeds to validation with merged options.
- Non-TTY `convert -d` emits `WARN:` once per unprompted archive and does not hang on `ReadLine`.
- `--fomod-skip` on a non-TTY run persists dismissed state into serialized TOML on convert output.
- `--fomod-choices` configures a FOMOD mod without TTY and marks configured state.
- GUI **Fetch Downloads** behavior unchanged after GUI delegates to the Core orchestrator.
- `docs/knowledgebase/agent-action-parity.md` documents post-download FOMOD configure for CLI.

## Scope Boundaries

**In scope**

- Core orchestrator, CLI console host, GUI adapter refactor, settings/env/CLI config resolution, tests, KB updates.
- Hooks on `install -d`, `convert -d`, `merge -d`.
- Spectre.Console or equivalent injectable console prompt layer behind a host interface.

**Deferred for later**

- Validation blocking when FOMOD choices are unset.
- Plugin images in terminal wizard.
- Download scope alignment so CLI only downloads `IsSelected` components (separate download-system change).
- Additional non-TTY modes beyond `warn-continue` (fail-fast, auto-dismiss-all) once v1 config plumbing exists.

**Outside this product's identity**

- Headless API server or MCP tools inside the desktop app (scripts/CLI remain the agent path).

## Key Decisions

- Architecture: Core `FomodPostDownloadOrchestrator` + `IFomodPostDownloadHost` adapters (GUI dialog host, console TTY host, warn-continue host, choices-file host).
- TTY detection uses input redirect, output redirect, `Environment.UserInteractive`, and explicit `--non-interactive` / `--interactive` overrides.
- Config precedence: CLI flags > environment variables > `settings.json` > TTY-derived default.
- Selected-only FOMOD scan even when download fetched more components.
- Convert/merge output persistence uses existing `-o` / output path; no silent in-place overwrite of input without documented flag.

## Dependencies / Assumptions

- PR #169 Core pieces (`FomodArchiveProbe`, `FomodDownloadPromptState`, `FomodConfiguredComponentMerger`) remain the persistence and detection layer.
- Instruction paths continue to use `<<modDirectory>>` / `<<kotorDirectory>>` placeholders after FOMOD merge.
- `ModSync.Tests` remains the single test project.

## Outstanding Questions

- None blocking — user confirmed synthesis call-outs (2026-06-14 session).
