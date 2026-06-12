# Plan 121 — FomodInstallerDialog GUI (Phase 6 slice 2)

**Status:** completed  
**Branch:** `feat/fomod-installer-dialog` (stacks on `feat/fomod-installer`)  
**Depends on:** PR #162

## Goals

1. `FomodInstallerDialog` walks `installSteps` from parsed `ModuleConfig.xml` with group-aware selection (radio vs checkbox semantics).
2. `FomodInstallerPresenter` builds a headless session model, evaluates step visibility from condition flags, validates group rules, and applies `Option.IsSelected` on the mapped `ModComponent`.
3. Entry: Mod Management **Configure FOMOD Mod** (folder picker for an extracted archive with `fomod/ModuleConfig.xml`).

## Out of scope

- Archive enumeration / download-cache auto-hook
- Plugin images from `image path`
- Full dependency-type plugin descriptors and live conditional file installs beyond mapper defaults

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~FomodInstallerPresenter"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~FomodParser"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~FomodToComponentMapper"
```
