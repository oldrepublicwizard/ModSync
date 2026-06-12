# Plan 116: Managed deployment engine (Phase 4, slice 1)

First sub-slice of Phase 4 (non-destructive deployment + per-mod uninstall) of the
Vortex/MO2 feature-parity roadmap. This slice ships the Core engine only; it is
purely additive (new files, no edits to existing source).

## Goal

A Vortex-hardlink-style deployment engine that operates on an already-staged
per-component folder tree:

- deploy staged files into the game directory via hardlink (automatic fallback to
  copy on cross-device, unsupported filesystem, or any hardlink failure),
- record exactly what was deployed in a per-component JSON manifest (relative path,
  SHA-256, size, method used, overwrite/backup provenance),
- uninstall a single component by removing exactly what its manifest lists,
  restoring any displaced game files from backup and pruning empty directories,
- purge all deployed components (MO2-style), newest deployment first,
- record cross-component same-path conflicts in the manifest so later phases can
  build the conflict UI on this data.

## Scope

### ModSync.Core (new folder `Services/Deployment/`)

- `DeploymentManifest.cs` — `DeploymentManifest` (ComponentGuid, ComponentName,
  DeployedUtc, Entries) + `DeploymentManifestEntry` (RelativePath, SourceHash,
  Size, DeploymentMethod, OverwroteExisting, BackupRelativePath) +
  `DeploymentMethod` enum (Hardlink, Copy). Newtonsoft.Json persistence matching
  existing Core services (`CheckpointService` precedent).
- `DeploymentService.cs` — constructor `(gameDirectory, stagingRoot, manifestRoot)`
  (absolute paths; internal code may use absolute paths per repo rules — the
  `<<modDirectory>>`/`<<kotorDirectory>>` placeholder rule applies to TOML
  instruction definitions only):
  - `DeployComponentAsync(Guid, string componentName, string stagedDirectory, CancellationToken)`
  - `UninstallComponentAsync(Guid, CancellationToken)` — skip-and-warn on
    user-modified files (current hash differs from manifest hash)
  - `PurgeAsync(CancellationToken)` — newest deployment first
  - `GetDeployedComponents()` / `TryGetManifest(Guid)`
  - Manifests are written atomically (temp file + move). Displaced game files are
    backed up under `manifestRoot/backups/{componentGuid}/` before overwrite.
- `HardLinkHelper.cs` — internal static P/Invoke helper: `CreateHardLink`
  (kernel32) on Windows, `link()` (libc) on Unix, guarded by OS checks; callers
  fall back to `File.Copy` on any failure and record the method actually used.

### Tests (`src/ModSync.Tests`, single project, NUnit)

`DeploymentServiceTests.cs`:

- deploy creates hardlinks (same file proven by writing through one path, else
  recorded method is Copy on filesystems without hardlink support)
- manifest JSON round-trip
- overwrite of a pre-existing game file creates a backup; uninstall restores it
- uninstall skips files modified after deployment (hash mismatch) with a warning
- purge removes all components' files and restores backups
- empty directories created by deployment are pruned on uninstall
- two components deploying the same relative path: second records
  OverwroteExisting + backup, conflict is observable from the manifests

Unique temp directories per test; cleanup in TearDown; well under 2 minutes.

## Out of scope (later Phase 4 slices)

- Instruction-execution redirect: staging Extract/Move/Copy/Rename instructions
  into `.staging/{componentGuid}/` during install.
- Patcher provenance capture via the `ImmutableCheckpoint` machinery
  (CheckpointService / ContentAddressableStore / FileProvenance).
- `AppSettings` toggle (classic destructive mode vs managed deployment).
- All GUI surfaces: per-mod uninstall action in `ModListSidebar`, deployment
  state indicator, purge/deploy commands.
- Conflict resolution UI (Phase 5) — this slice only records the data.

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~DeploymentService"
```
