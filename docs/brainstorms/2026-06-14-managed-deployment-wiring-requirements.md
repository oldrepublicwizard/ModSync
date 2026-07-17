---
title: "Managed deployment wiring (Phase 4 slice 2)"
type: requirements
status: reviewed
date: 2026-06-14
origin: docs/plans/2026-06-12-116-managed-deployment-engine-plan.md
actors:
  - A1: Mod installer (wizard or single-mod install)
  - A2: Agent/automation running CLI or scripted installs
flows:
  - F1
  - F2
---

# Managed deployment wiring (Phase 4 slice 2)

## Summary

Ship opt-in managed deployment for ModSync installs. Classic direct-to-game
install remains the default. When enabled in Settings, file-operation instructions
that target the game directory stage per-mod files under the active profile, then
deploy into the game via the existing `DeploymentService` with JSON manifests.
Patcher instructions continue writing directly to the game folder. This slice is
deploy-only: no uninstall or purge GUI.

## Problem Frame

`DeploymentService` (plan 116 slice 1, merged #158) can deploy a pre-staged
per-component tree with hardlink/copy fallback and per-mod manifests, but the
install pipeline still writes straight into the game directory. This slice wires
staging and deploy so manifests are created for file-operation mods; full
MO2/Vortex parity (reversible per-mod uninstall) remains deferred.

## Actors

- A1. **Mod installer** — runs the install wizard or installs a single mod from
  the mod list. Expects classic behavior unless they opt into managed mode.
- A2. **Agent/automation** — runs headless or scripted installs against the same
  Core pipeline. Must observe the same managed-vs-classic semantics as the GUI.

## Requirements

- R1. **Classic default** — New and existing users default to classic install
  (direct writes to the game directory). No behavior change until managed mode is
  enabled.
- R2. **Settings toggle** — A persisted Settings control enables managed
  deployment. The toggle is off by default. Placement: a **Deployment** section
  in `SettingsDialog` (or General if no new section). Use a `CheckBox` with
  label **Managed deployment** and tooltip explaining: file-operation mods stage
  under the active profile then deploy via hardlink/copy; only installs after
  enable are tracked (R9); manifests are not yet actionable for uninstall in
  this slice. When no active profile is loaded, the control is **disabled** with
  inline help pointing to Profiles.
- R3. **Hybrid staging** — When managed mode is on, Extract, Move, Copy, and
  Rename instructions whose resolved destination is under the game directory
  write into the active profile's staging tree instead of the game folder.
  Until planning classifies Delete, CleanList, DelDuplicate, Execute, and Run
  for game-directory targets, those actions **pass through** to the game
  directory as in classic mode and are **not** recorded in deployment manifests
  in slice 2 (same tracking gap as Patcher output).
- R4. **Post-component deploy** — After a component installs successfully in
  managed mode, staged files for that component deploy into the game directory
  via `DeploymentService`, and a per-component manifest is persisted. Component
  success requires deploy **and** manifest write to complete. If deploy fails
  after staging, the component install is **failed**, no manifest is written, and
  any partial game-directory mutations from that deploy attempt must be rolled
  back or prevented before the install reports success.
- R5. **Patcher exclusion** — Patcher instructions always write directly to the
  game directory. Patcher output is not staged and not recorded in deployment
  manifests in this slice. When a component runs staged file ops **then**
  Patcher in instruction order, the persisted manifest reflects **pre-patcher**
  file state; R7 must warn that Patcher may have modified previously deployed
  files and those changes are not tracked for managed uninstall.
- R6. **Profile-scoped artifacts** — Staging trees and deployment manifests for
  a managed install live under the active profile's storage area, not the shared
  mod workspace or game directory. Managed installs require an **active named
  profile** at install start. If managed mode is enabled but no profile is
  active, staging/deploy must not run and the user must see a clear error
  directing them to load or create a profile first (wizard, single-mod, CLI).
- R7. **Patcher mixed-mod warning** — When a managed install completes and at
  least one installed component ran a Patcher instruction, every install
  completion surface notes that Patcher-touched files are not tracked for managed
  uninstall (and pre-patcher manifest staleness per R5 when applicable):
  - **Wizard:** `BaseInstallCompletePage`, optional `WidescreenCompletePage`
    (when shown), and `FinishedPage`.
  - **Single-mod:** success completion dialog after context-menu install (see R8).
  - **CLI:** install summary output.
  Messaging: **warning-tier** (not informational). Minimum copy must state that
  Patcher-modified files are not tracked for managed uninstall. List **mod names**
  that ran Patcher. CLI emits a dedicated `WARN:` line. When no component ran
  Patcher, omit the caveat and show a brief managed-success confirmation instead.
- R8. **Single-mod parity** — Installing one mod from the mod list uses the same
  managed-vs-classic rules as a full wizard install. On success in managed mode,
  show a completion dialog (`InformationDialog` or equivalent) with managed
  summary lines and the R7 Patcher caveat when applicable.
- R9. **No retroactive tracking** — Enabling managed mode does not create
  manifests for mods already installed classically on that profile. Managed
  tracking begins with installs run after the toggle is turned on. Disabling
  managed mode returns future installs to classic behavior; existing manifests
  from prior managed installs remain on disk. Profiles may therefore mix
  classic-era untracked files with managed-era manifests — completion messaging
  must not imply full-profile tracking.
- R10. **CLI parity** — CLI `install` reads the same persisted managed-deployment
  toggle as the GUI via a **Core-readable** settings field (shared `settings.json`
  key, not GUI-only `AppSettings`). When enabled, A2 automation gets staging,
  deploy, and manifest behavior identical to A1. **Priority:** P1 — may ship
  after the wizard managed path (P0) lands.

## Priority

- **P0 (ship gate):** R1, R2, R3 (with pass-through interim rule), R4, R5, R6,
  R7 wizard finish path, F1 managed full install via wizard.
- **P1 (same slice, after P0):** R8 single-mod parity, R7 single-mod + CLI
  completion surfaces, R10 CLI parity and Core-readable settings field.
- **P2 (document or follow-on):** Dry-run / `ValidatePage` simulation must honor
  managed staging redirects when the toggle is on, or the doc must explicitly
  state that managed mode is **install-only validation** until a named follow-up.

## Key Flows

- F1. **Managed full install**
  - **Trigger:** User enables managed deployment in Settings, then runs a full
    install (wizard or equivalent).
  - **Actors:** A1
  - **Steps:** Toggle is read at install start. For each selected component,
    qualifying file-operation instructions stage under the profile; Patcher
    instructions write to the game. On component success, deploy staged files
    and write manifest. On install completion, show summary; include Patcher
    caveat if any component used Patcher.
  - **Covered by:** R2, R3, R4, R5, R6, R7, R8, R10

- F2. **Classic install unchanged**
  - **Trigger:** Managed toggle is off (default).
  - **Actors:** A1, A2
  - **Steps:** Install pipeline behaves as today — direct game writes, no
    staging redirect, no `DeploymentService` deploy step.
  - **Covered by:** R1

## Acceptance Examples

- AE1. **Toggle off preserves classic behavior**
  - **Covers:** R1, F2
  - **Given:** Managed deployment is disabled in Settings.
  - **When:** User runs a full install with file-operation instructions targeting
    the game directory.
  - **Then:** Files land directly in the game folder; no profile staging tree is
    created for that install; no deployment manifest is written.

- AE2. **Toggle on stages and deploys file ops**
  - **Covers:** R3, R4, R6, F1
  - **Given:** Managed deployment is enabled and an active profile is loaded.
  - **When:** User installs a mod whose instructions only use Extract/Move/Copy/Rename
    into game paths.
  - **Then:** Those files appear in the profile staging area during install, then
    in the game directory after deploy; a manifest exists for that component GUID.

- AE3. **Patcher writes through in managed mode**
  - **Covers:** R5, R7
  - **Given:** Managed deployment is enabled.
  - **When:** User installs a mod that includes a Patcher instruction.
  - **Then:** Patcher output appears in the game directory without passing through
    staging; the end-of-install summary includes a note that Patcher files are not
    tracked for managed uninstall.

- AE4. **Single-mod install honors toggle**
  - **Covers:** R8, R7
  - **Given:** Managed deployment is enabled and an active profile is loaded.
  - **When:** User installs one mod from the mod list context menu.
  - **Then:** Staging and deploy behavior matches a full install for that
    component; completion dialog shows managed summary and Patcher caveat when
    that mod used Patcher.

- AE5. **CLI install honors toggle**
  - **Covers:** R10
  - **Given:** Managed deployment is enabled in persisted settings.
  - **When:** Agent runs `ModSync.Core install` against the same profile paths.
  - **Then:** Staging, deploy, and manifest behavior match the GUI managed path;
    completion output includes the Patcher caveat when applicable.

## Scope Boundaries

**In scope**

- Instruction-target redirect for hybrid staging (rewrite game-directory
  destinations to profile staging roots during managed installs).
- Settings toggle and persistence, plus a Core-readable persisted flag for
  CLI parity (R10).
- Profile-scoped staging and manifest roots.
- Post-component `DeploymentService` deploy hook.
- Single-mod install parity (R8) using the same managed-vs-classic rules as
  full wizard installs.
- End-of-install Patcher caveat on wizard finish pages, single-mod completion
  dialog, and CLI install summary (R7).
- Headless/Core tests for managed vs classic paths.

**Deferred for later slices**

- Per-mod uninstall and purge GUI actions.
- Patcher output in deployment manifests (ImmutableCheckpoint / provenance).
- Managed deployment as default for new or upgraded users.
- Conflict resolution UI (manifests remain input for Phase 5).
- Persisting deployment state in `DownloadCacheService` or mod list badges.

**Outside this product slice**

- Replacing HoloPatcher with a staging-capable patcher runtime.
- Cross-profile sharing of staging trees.

## Success Criteria

- **Infrastructure (P0):** A user can enable managed mode, run a wizard install,
  and observe staged-then-deployed file-operation mods with manifests under the
  active profile.
- **User safety:** Classic users see no change until they opt in. Settings and
  completion messaging state that manifests are recorded but **not actionable**
  for uninstall until a later slice ships uninstall/purge surfaces.
- **Partial tracking:** Mixed Patcher + file-op mods complete successfully with
  explicit summary caveats about Patcher and pass-through actions (R3 interim
  rule).
- **Tests:** Cover at least classic unchanged, managed stage+deploy, Patcher
  exclusion, profile-scoped artifact paths, and active-profile gate (R6).
  CLI parity tests (R10) are P1.

## Resolved Questions

- **R7 completion surfaces:** Patcher caveat must appear on wizard finish
  pages, single-mod install completion dialog, and CLI install summary when any
  installed component ran a Patcher instruction.
- **Deploy failure (formerly OQ3):** Staging success + deploy failure marks the
  component failed, writes no manifest, and requires rollback or prevention of
  partial game mutations (see R4).
- **Profile artifact layout (formerly OQ1):** Per-profile deployment artifacts
  live under `{settingsDir}/profiles/{sanitized}/staging/` and
  `.../deployment/manifests/` (plus `backups/`). Profile metadata JSON stays at
  `profiles/{sanitized}.json`. See
  `docs/plans/2026-06-14-123-managed-deployment-wiring-plan.md`.
- **Active profile identity:** `activeProfileName` in shared `settings.json`;
  CLI `--profile <name>` when managed mode is on. See plan 123.
- **R3 action boundary:** Extract/Move/Copy/Rename stage; Patcher excluded;
  Delete/CleanList/DelDuplicate/Execute/Run pass-through in slice 2. See plan 123.
- **Core-readable toggle:** `managedDeploymentEnabled` in `settings.json` via
  Core `ModSyncSettings` + GUI/CLI field. See plan 123.
