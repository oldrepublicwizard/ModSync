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

`[OPEN]` Not yet wired into instruction execution: staging redirect during install,
patcher provenance capture (ImmutableCheckpoint), the `AppSettings` classic-vs-managed
toggle, and GUI surfaces are later Phase 4 slices.

Tests: `src/ModSync.Tests/DeploymentServiceTests.cs`
(`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~DeploymentService"`).
