---
title: "feat: managed deployment install wiring + validation parity"
status: active
date: 2026-07-13
origin: Audit of install pipeline vs STRATEGY / managed-deployment.md / validation-pipeline.md
depends_on: docs/plans/2026-06-12-116-managed-deployment-engine-plan.md (#158); PR #168 (wiring draft)
---

# Plan 2026-07-13-004 — Managed deployment + validation remaining work

## Summary

`DeploymentService` (Phase 4 slice 1 / #158) and fail-closed `FomodConfigurationGate` (validate + install) are in-tree. The remaining STRATEGY-relevant install-pipeline gap is **wiring managed deployment into real installs** and keeping the validation pipeline honest about that mode. This plan lists concrete units only — no greenfield redesign.

## Already shipped (do not re-do)

| Capability | Evidence |
|------------|----------|
| Hardlink/copy deploy + manifests + uninstall/purge engine | `Services/Deployment/*`, `DeploymentServiceTests` |
| Unified validation stages + Environment fail-fast | `InstallationValidationPipeline` |
| Fail-closed FOMOD gate on validate + install + InstallStartPage | `FomodConfigurationGate`, pipeline stage `FomodConfiguration`, `InstallationService` |
| Progress `CountStages` matches skip flags | Fixed 2026-07-13 (this audit) |
| KB stage table includes FOMOD | `docs/knowledgebase/validation-pipeline.md` |

## Problem frame

Classic install still writes Extract/Move/Copy/Rename straight into `<<kotorDirectory>>`. Managed mode (Vortex/MO2 parity Phase 4 slice 2) must stage under a profile artifact tree, deploy via `DeploymentService`, and surface classic-vs-managed + uninstall UX. Open PR [#168](https://github.com/th3w1zard1/ModSync/pull/168) (`feat/managed-deployment-wiring`) already drafts most of U1–U3; land or re-apply rather than rewrite.

## Requirements

| ID | Requirement | Unit |
|----|-------------|------|
| R1 | Opt-in managed install: stage game-bound Extract/Move/Copy/Rename, deploy after component success | U1 |
| R2 | Classic destructive install remains default when toggle off | U1 |
| R3 | Settings + active profile required for managed mode; fail fast without profile | U1 |
| R4 | Patcher stays direct-to-game; warn when unmanaged in managed session | U1 |
| R5 | CLI `--profile` + Core-readable `managedDeploymentEnabled` parity | U2 |
| R6 | Single-mod install uses same session as wizard | U2 |
| R7 | Dry-run / ValidatePage either models staging or documents install-only managed validation | U3 |
| R8 | Per-mod uninstall / purge / deployment indicator GUI | U4 |
| R9 | Patcher provenance via ImmutableCheckpoint (manifest completeness) | U5 |
| R10 | KB + living plan updated when wiring lands | U6 |

## Implementation units

### U1. Land managed install session (wizard P0)

**Goal:** When `managedDeploymentEnabled` and an active profile exist, redirect game-bound Extract/Move/Copy/Rename into `{profileArtifacts}/staging/{componentGuid}/`, then `DeploymentService.DeployComponentAsync` after each successful component; block install start without a profile (R3); wizard finish pages show R7-style patcher/manifest messaging.

**Requirements:** R1–R4

**Files (expected; prefer merge of #168):**

- `src/ModSync.Core/Services/Installation/ManagedInstallSession.cs` (new)
- `src/ModSync.Core/Services/Installation/ManagedInstallResult.cs` (new)
- `src/ModSync.Core/Services/Settings/ModSyncSettings.cs` (new)
- `src/ModSync.Core/ModComponent.cs`, `InstallationService.cs`, `ProfileService.cs`
- `src/ModSync.GUI/Models/AppSettings.cs`, `SettingsDialog.*`, finish pages
- Tests: `ManagedInstallSessionTests`, settings round-trip

**Constraints:** Path sandboxing for TOML unchanged; absolute paths OK inside services. No XAML font/style overrides.

**Depends on:** #158 engine (done). Prefer landing [#168](https://github.com/th3w1zard1/ModSync/pull/168) over a parallel rewrite.

---

### U2. Single-mod + CLI parity

**Goal:** `InstallSingleComponentAsync` and CLI `install` share `ManagedInstallSession`; `--profile <name>`; Core loads `managedDeploymentEnabled` / `activeProfileName` from settings.json; CLI emits `WARN:` for patcher-only components not in manifests.

**Requirements:** R5, R6

**Dependencies:** U1

**Files:** `ModBuildConverter.cs`, `InstallationService.cs`, CLI/settings tests

---

### U3. Managed-mode validation / dry-run parity

**Goal:** Decide and implement one path:

- **A (preferred):** DryRun + VFS honor staging redirect when managed settings are on (same relative tree under a virtual staging root), or
- **B (document):** ValidatePage / `--dry-run` remain classic-path only; install pre-check docs state managed behavior is install-time only.

Update [validation-pipeline.md](../knowledgebase/validation-pipeline.md) and [managed-deployment.md](../knowledgebase/managed-deployment.md) with the chosen behavior.

**Requirements:** R7

**Dependencies:** U1

**Files:** `DryRunValidator` / instruction path resolution and/or KB-only if B

---

### U4. Uninstall / purge GUI surfaces

**Goal:** Per-component uninstall from mod list (manifest-backed), purge-all, and a deployment-state indicator. Builds on `DeploymentService.UninstallComponentAsync` / `PurgeAsync` and cross-component overwrite metadata already in manifests.

**Requirements:** R8

**Dependencies:** U1 (manifests written during managed installs)

**Files:** `ModListSidebar` / mod list item controls, menu commands, headless + desktop smoke

**Out of scope here:** Phase 5 conflict-resolution UI (analyzer already ships separately).

---

### U5. Patcher provenance (ImmutableCheckpoint)

**Goal:** Capture patcher-written files into deployment provenance so managed uninstall does not leave orphaned patcher outputs undocumented. Use existing `CheckpointService` / `ContentAddressableStore` / `FileProvenance` machinery called out in plan 116.

**Requirements:** R9

**Dependencies:** U1

**Files:** Checkpoint integration from patcher instruction completion; tests for provenance round-trip

---

### U6. Docs + living-plan closure

**Goal:** After U1–U2 land: flip `managed-deployment.md` `[OPEN]` → `[REPO]` for wiring; refresh `vortex-mo2-feature-parity-living-plan.md` Phase 4; link U3 decision; keep FOMOD fail-closed called out as done (not blocked on managed mode).

**Requirements:** R10

**Dependencies:** U1 (minimum)

## Non-goals

- Replacing classic install as the default.
- Re-implementing `DeploymentService` or FOMOD gate.
- Phase 5 conflict UI beyond data already in manifests.
- STRATEGY guide-ingestion track (separate plan `2026-07-13-001`).

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~DeploymentService"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~ManagedInstallSession|FullyQualifiedName~FomodConfigurationGate|FullyQualifiedName~ValidationPipelineParity"
```

## Suggested order

1. Merge or cherry-pick #168 (U1) → U2 → U6 (partial).
2. U3 decision before treating ValidatePage as managed-aware.
3. U4 / U5 as follow-on Phase 4 slices (can parallelize after U1).
