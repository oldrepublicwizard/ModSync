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

`[OPEN]` Still deferred: patcher provenance (ImmutableCheckpoint), per-component
uninstall GUI polish, managed dry-run/VFS validation parity, CLI `--profile`
polish (U3). Follow-up plan:
[docs/plans/2026-07-13-004-managed-deployment-validation-plan.md](../plans/2026-07-13-004-managed-deployment-validation-plan.md).

Tests: `DeploymentServiceTests`, `ManagedInstallSessionTests`, `ModSyncSettingsTests`,
`ParityPortsTests` (backend selector).
