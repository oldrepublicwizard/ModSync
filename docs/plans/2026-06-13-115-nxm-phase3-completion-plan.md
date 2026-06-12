# Plan 115 — nxm Phase 3 completion

**Status:** completed  
**Branch:** `feat/nxm-protocol-handler`  
**Depends on:** Plans 113, 114

## Goals

1. Settings toggle (Windows/Linux) to register/unregister `nxm://` handler, persisted in `AppSettings`.
2. Post-download `ResourceRegistry` / download-cache parity via `DownloadCacheService.UpdateResourceMetadataWithFilenamesAsync`.
3. Resolver returns matched HTTPS registry URL; ambiguous match gets distinct UX.

## Out of scope

- macOS `Info.plist` registration
- Strict single-instance for non-nxm launches
- Desktop E2E (manual per `AGENTS.md`)

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~Nxm|FullyQualifiedName~AppSettingsNxm"
```
