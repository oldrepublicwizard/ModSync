# Plan 116 — nxm Phase 4 handler hardening

**Status:** completed  
**Branch:** `feat/nxm-phase4-hardening`  
**Depends on:** PR #155 (merged)

## Goals

1. Strict single-instance for non-nxm launches (forward activate, exit secondary).
2. Dev escape hatch: `--allow-multiple-instances`.
3. macOS `CFBundleURLTypes` in both `Info.plist` files + declarative registration branch.
4. macOS Settings informational status block (not disabled checkbox).
5. `ApplicationLaunchCoordinator` for testable launch policy.

## Out of scope

- Handler conflict detection (MO2/Vortex)
- macOS `.app` bundle CI packaging
- Desktop E2E

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~Nxm"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~ApplicationLaunchCoordinator"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~NxmInfoPlist"
```
