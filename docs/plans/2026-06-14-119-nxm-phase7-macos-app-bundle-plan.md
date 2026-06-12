# Plan 119 — nxm Phase 7 macOS .app CI bundling

**Status:** completed  
**Branch:** `feat/nxm-phase7-macos-app-bundle`  
**Depends on:** PR #163 (merged)

## Goals

1. macOS release artifacts ship `ModSync.app` (not only a flat publish folder).
2. CI uses `Dotnet.Bundle` (`-t:BundleApp`) on `osx-x64` / `osx-arm64` release builds.
3. Bundle `Info.plist` retains `CFBundleURLTypes` → `nxm` for Nexus hand-off.
4. `scripts/ci/bundle-macos-app.ps1` validates bundle layout after publish.

## Out of scope

- Desktop E2E browser-click validation
- Code signing / notarization

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~MacOsReleaseBundle"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~NxmInfoPlist"
```

Release workflow (manual dispatch on macOS runners):

- `ModSync.app` present in `package/osx-*/ModSync <version>/`
- `ModSync.app/Contents/Info.plist` contains `nxm` URL scheme
