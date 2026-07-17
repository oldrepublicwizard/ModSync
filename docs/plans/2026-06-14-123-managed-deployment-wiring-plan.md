---
title: "feat: Managed deployment wiring (Phase 4 slice 2)"
type: feat
status: draft
date: 2026-06-14
origin: docs/brainstorms/2026-06-14-managed-deployment-wiring-requirements.md
depends_on: docs/plans/2026-06-12-116-managed-deployment-engine-plan.md (#158)
branch: feat/managed-deployment-wiring
---

# feat: Managed deployment wiring (Phase 4 slice 2)

Second sub-slice of Phase 4 (non-destructive deployment) of the Vortex/MO2
feature-parity roadmap. Wires the existing `DeploymentService` into the install
pipeline behind an opt-in Settings toggle.

**Requirements source:** `docs/brainstorms/2026-06-14-managed-deployment-wiring-requirements.md`

## Goal

When managed deployment is enabled and an active named profile is loaded:

1. Extract / Move / Copy / Rename instructions targeting the game directory stage
   under that profile, then deploy via `DeploymentService` after each component
   succeeds.
2. Patcher instructions continue writing directly to the game directory.
3. Classic direct-to-game installs remain the default (toggle off).

Ship P0 (wizard full install) first; P1 adds single-mod + CLI parity.

## Resolved planning inputs

These items were listed under **Resolve Before Planning** in the requirements doc.
This plan locks them as implementation decisions.

### Active profile identity

- Persist `activeProfileName` (string, nullable) in `%AppData%/ModSync/settings.json`.
- GUI: set when user **Activate**s a profile in `ProfileManagerDialog`; clear only
  when user explicitly deactivates (add optional "No active profile" if needed) or
  when the named profile is deleted (clear + warn).
- CLI: add `--profile <name>` on `install` verb; when omitted, use
  `activeProfileName` from settings; when managed mode is on and neither is set,
  fail fast with R6 error text.
- Install start reads active profile once; managed path requires the profile JSON
  to exist on disk (`ProfileService.LoadProfile`).

### Profile deployment artifact layout

Keep existing flat profile metadata at `{settingsDir}/profiles/{sanitized}.json`.
Add a sibling artifact root per profile:

```
{settingsDir}/profiles/{sanitized}/
  staging/{componentGuid}/     # per-component staged tree (input to DeployComponentAsync)
  deployment/                    # manifestRoot for DeploymentService
    manifests/
    backups/
```

- `ProfileService` gains `GetProfileArtifactDirectory(profileName)` using the same
  `SanitizeProfileFileName` helper.
- `DeploymentService` is constructed with:
  - `gameDirectory` = `MainConfig.DestinationPath`
  - `stagingRoot` = `{artifactDir}/staging`
  - `manifestRoot` = `{artifactDir}/deployment`
- Does not relocate or rewrite existing `profiles/*.json` files.

### R3 action boundary (slice 2)

| Action | Managed mode behavior |
|--------|----------------------|
| Extract, Move, Copy, Rename | Stage when resolved destination is under game dir |
| Patcher | Always direct to game (R5); track component for R7 warning |
| Delete, CleanList, DelDuplicate, Execute, Run | **Pass-through** to game; not in manifest |

Redirect applies to **destination** paths (and Move/COPY/Rename sources that
resolve under game dir only when they are the write target — follow existing
instruction semantics; do not stage mod-directory-only reads).

### Core-readable managed toggle (R10)

- Add `managedDeploymentEnabled` (bool, default `false`) to `settings.json`.
- Introduce `ModSync.Core/Services/Settings/ModSyncSettings.cs`:
  - `Load(string settingsDirectory)` / `Save(...)` using **System.Text.Json**
    property names matching GUI `AppSettings` (`managedDeploymentEnabled` camelCase).
  - Shared constants for known keys; path resolution matches GUI
    (`ModSync` then legacy `KOTORModSync` migration — reuse path logic from
    `SettingsManager` or extract a tiny `SettingsPaths` helper in Core).
- GUI `AppSettings` + `SettingsManager` add the field and read/write through the
  same JSON key.
- CLI `ModBuildConverter.SettingsData` adds the field; stop duplicating path-only
  fields over time (out of scope: full settings unification).

## Architecture

### `ManagedInstallSession` (Core)

New type created at install start when `managedDeploymentEnabled && activeProfileName`:

- Holds profile name, artifact paths, `DeploymentService` instance, and a
  `HashSet<string>` of component names that ran Patcher.
- `ValidateOrThrow()` — R6 gate.
- `bool ShouldStage(Instruction.ActionType action, string resolvedDestinationPath)`
- `string MapToStagingPath(Guid componentGuid, string resolvedGamePath)`
- `Task<InstallExitCode> DeployComponentAsync(ModComponent component, CancellationToken)`

Called from `InstallationService` (wizard + single-mod) and CLI install path —
not from GUI pages directly.

### Staging redirect hook

Minimal-blast-radius: after `instruction.SetRealPaths(...)` in
`ModComponent.ExecuteSingleInstructionAsync`, when session is active and
`ShouldStage` is true, rewrite `instruction.Destination` (and any resolved game
write paths the action uses) from game path to
`{stagingRoot}/{componentGuid}/<relative-to-game>`.

Do **not** change TOML placeholders or VFS dry-run in P0 (P2).

### Post-component deploy hook

In `ModComponent.InstallAsync`, after `ExecuteInstructionsAsync` returns Success
and session is active:

1. If component had any staged file ops, call `session.DeployComponentAsync`.
2. On deploy failure: mark component failed (R4), do not write manifest
   (`DeploymentService` already persists on success only).
3. If staging directory is empty (Patcher-only mod), skip deploy; no manifest.

Wire into:

- `InstallationService.InstallAllSelectedComponentsAsync` (wizard)
- `InstallationService.InstallSingleComponentAsync` (P1)
- CLI `install` handler (P1)

### Install result / R7 messaging

Add `ManagedInstallResult` on session (or static collector cleared per install):

- `bool ManagedModeUsed`
- `IReadOnlyList<string> PatcherComponentNames`
- `int ManifestsWritten`

Pass to wizard finish pages via existing install completion wiring
(`InstallWizardDialog` / `WizardHostControl` statistics object or new optional
field on install summary DTO).

Surfaces (P0 wizard only first):

- `BaseInstallCompletePage`, `WidescreenCompletePage` (when shown), `FinishedPage`
- P1: single-mod `InformationDialog` in `MainWindow.InstallModSingle_Click`
- P1: CLI `WARN:` line in install summary

Copy rules per R7/R9 in requirements doc.

## Implementation phases

### Phase P0 — Ship gate

| Step | Work |
|------|------|
| 1 | `ModSyncSettings` + `managedDeploymentEnabled` + `activeProfileName` in settings.json |
| 2 | `ProfileService.GetProfileArtifactDirectory` + directory ensure |
| 3 | `ManagedInstallSession` + staging redirect in `ModComponent` |
| 4 | Post-component deploy in `InstallAsync` / `InstallationService` |
| 5 | Settings UI: Deployment section, checkbox, tooltip, disabled without active profile |
| 6 | Profile activate persists `activeProfileName` |
| 7 | R6 error when managed on without profile (block install start) |
| 8 | R7 wizard finish messaging |
| 9 | Tests (see below) |

### Phase P1 — Parity

| Step | Work |
|------|------|
| 10 | Single-mod success dialog (R8) |
| 11 | CLI `--profile`, settings load, managed path, `WARN:` output (R10) |
| 12 | CLI + single-mod tests |

### Phase P2 — Document only (no code required in this PR)

- `ValidatePage` / dry-run: either honor staging redirect or document
  **install-only validation** for managed mode (requirements P2).

## Files (expected touch set)

### ModSync.Core

- `Services/Settings/ModSyncSettings.cs` (new)
- `Services/Profiles/ProfileService.cs` — artifact directory helper
- `Services/Installation/ManagedInstallSession.cs` (new)
- `ModComponent.cs` — staging redirect + deploy hook
- `Services/InstallationService.cs` — session lifecycle, R6 gate, result propagation
- `CLI/ModBuildConverter.cs` — settings field, `--profile`, install summary (P1)

### ModSync.GUI

- `Models/AppSettings.cs` — `ManagedDeploymentEnabled`, `ActiveProfileName`
- `Dialogs/SettingsDialog.axaml(.cs)` — Deployment section
- `Dialogs/ProfileManagerDialog.axaml.cs` — persist active profile on activate
- `Dialogs/WizardPages/BaseInstallCompletePage.axaml(.cs)` — managed summary + R7
- `Dialogs/WizardPages/WidescreenCompletePage.axaml(.cs)` — R7 when shown
- `Dialogs/WizardPages/FinishedPage.axaml(.cs)` — R7
- `Dialogs/InstallWizardDialog.axaml.cs` / `Controls/WizardHostControl.axaml.cs` —
  pass install result into finish pages
- `MainWindow.axaml.cs` — single-mod completion (P1)

### Tests (`src/ModSync.Tests` only)

- `ManagedInstallSessionTests.cs` (new) — path mapping, action classification, R6 gate
- `ManagedInstallIntegrationTests.cs` (new) — classic unchanged, stage+deploy,
  Patcher exclusion, empty staging skip, deploy failure marks component failed
- `ModSyncSettingsTests.cs` (new) — round-trip `managedDeploymentEnabled` +
  `activeProfileName`
- Extend `ProfileServiceTests` — artifact directory paths

Use unique temp dirs per test; no `LongRunning` suffix unless a test exceeds 2 minutes.

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~ManagedInstall|FullyQualifiedName~ModSyncSettings&FullyQualifiedName~ProfileService"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~DeploymentService"
```

P0 manual (desktop): enable managed mode, activate profile, run wizard install
with preload args per `AGENTS.md`; confirm staging tree + manifest under profile
artifacts and R7 text when a Patcher mod is included.

## Out of scope

Per requirements doc: uninstall/purge GUI, Patcher in manifests, managed-as-default,
dry-run parity (P2), validation/VFS managed simulation, FOMOD/Nexus handler work.

## Risks / notes

- **Deploy failure rollback:** `DeploymentService` backs up overwritten game files
  per file during deploy; on mid-deploy failure, plan should call
  `UninstallComponentAsync` for partial manifest if one was written, or rely on
  deploy atomicity (manifest written only at end — verify in implementation).
- **Checkpoint interaction:** existing post-install checkpoints remain; managed
  manifests are orthogonal. Do not conflate in user messaging (R9).
- **Mixed profile history:** completion copy must not imply full-profile tracking.

## Doc updates (same PR)

- `docs/knowledgebase/` — short managed-deployment wiring page + README index entry
- `docs/plans/vortex-mo2-feature-parity-living-plan.md` — mark slice 2 in progress /
  landed when complete
