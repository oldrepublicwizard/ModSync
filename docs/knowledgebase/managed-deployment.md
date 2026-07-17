# Managed deployment engine

`[REPO]` Phase 4 slice 1 of the Vortex/MO2 feature-parity roadmap (plan
[2026-06-12-116](../plans/2026-06-12-116-managed-deployment-engine-plan.md)) added a
non-destructive deployment engine in `src/ModSync.Core/Services/Deployment/`:

- `DeploymentService` — deploys an already-staged per-component folder tree into the
  game directory via hardlink (`HardLinkHelper`: `CreateHardLink` on Windows, `link()`
  on Unix) with automatic fallback to copy; the method used is recorded per file.
- `DeploymentManifest` / `DeploymentManifestEntry` — per-component JSON manifest
  (relative path, SHA-256, size, method, overwrite/backup provenance) persisted
  atomically under `{manifestRoot}/manifests/{componentGuid}.json`.
- Displaced game files are backed up under `{manifestRoot}/backups/{componentGuid}/`
  and restored on uninstall.
- `UninstallComponentAsync` removes exactly what the manifest lists, skip-and-warns
  on hash mismatches (user/other-mod modifications), restores backups, and prunes
  empty directories. `PurgeAsync` unwinds all components newest-first (MO2-style).
- Cross-component same-path deployments are logged as warnings and observable from
  the persisted manifests (`OverwroteExisting` + `BackupRelativePath`); Phase 5
  conflict UI builds on this data.

`[REPO]` The service takes absolute paths (`gameDirectory`, `stagingRoot`,
`manifestRoot`); the `<<modDirectory>>`/`<<kotorDirectory>>` placeholder rules apply
to TOML instruction definitions only, not to internal services.

`[REPO]` Install wiring (Phase 4 slice 2 / architecture U2): opt-in
`managedDeploymentEnabled` + active profile stages Extract/Move/Copy/Rename under
the profile artifact tree, then deploys via `IInstallBackend` /
`InstallBackendSelector` → `ManagedDeploymentInstallBackend` → `DeploymentService`.
Classic remains default when the toggle is off. Session entry:
`ManagedInstallSession` + `InstallationService.RunWithManagedInstallSessionAsync`
(wizard + single-mod). Settings UI: Deployment checkbox (requires active profile).

`[REPO]` CLI overrides (parity U3 / [#177](https://github.com/oldrepublicwizard/ModSync/pull/177)):
`install --managed`, `--no-managed`, and `--profile` force classic vs managed without
editing `settings.json`. Fail-closed when managed is on without a resolvable profile.
Documented in [core-cli-reference.md](core-cli-reference.md).

`[REPO]` Uninstall / purge GUI (parity U5 / [#179](https://github.com/oldrepublicwizard/ModSync/pull/179)):
Tools menu **Managed Deployment Status** and **Purge Managed Deployments**; mod-list
**Deployed** badge; context menu **Uninstall Managed Deployment** when a manifest
exists. Purge/uninstall are blocked while `ManagedInstallSession.Current` is set.

`[REPO]` Patcher provenance (parity U6): when a managed component runs a Patcher
instruction, ModSync snapshots game-dir file hashes before install, diffs after
staged deploy, and merges added/changed paths into the component manifest via
`DeploymentService.RecordLiveGameFilesAsync` so uninstall can delete those files.
In-place overwrite **restore** of pre-patcher bytes (full ImmutableCheckpoint CAS)
is still deferred.

## Validation vs managed installs (parity U4 decision B)

`[REPO]` **Decision (2026-07-17):** keep classic VFS DryRun as-is; do **not** redirect
DryRun through managed staging in this release. When `managedDeploymentEnabled` is on:

- Pre-install validation (wizard `ValidatePage`, CLI `validate`, `install` pre-check)
  still runs the shared `InstallationValidationPipeline` against **game + mod
  directories as classic paths** (`<<kotorDirectory>>` / `<<modDirectory>>`).
- DryRun therefore answers “what would classic direct-to-game instructions do?”,
  **not** “what lands in profile staging before hardlink deploy.”
- Agents and humans should treat DryRun as a **classic-path sanity check**. Trust
  managed correctness to: FOMOD gate, archive presence, install-time staging +
  `DeploymentService` manifests, then post-install Status / Deployed badge /
  uninstall.

Full managed DryRun/VFS staging parity remains an optional future unit (plan option
A). See [validation-pipeline.md](validation-pipeline.md#managed-deployment-caveat).

`[OPEN]` Still deferred: full ImmutableCheckpoint CAS restore for in-place patcher
overwrites (U6 records live added/changed paths for uninstall-delete; pre-image
restore via CAS remains optional). Follow-up plan:
[docs/plans/2026-07-13-004-managed-deployment-validation-plan.md](../plans/2026-07-13-004-managed-deployment-validation-plan.md).

Tests: `DeploymentServiceTests`, `ManagedInstallSessionTests`, `ModSyncSettingsTests`,
`ParityPortsTests` (backend selector), `ManagedInstallCliOverridesTests`.
