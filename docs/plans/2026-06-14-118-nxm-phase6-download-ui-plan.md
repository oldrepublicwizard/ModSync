# Plan 118 — nxm Phase 6 in-progress download UI

**Status:** in progress  
**Branch:** `feat/nxm-phase6-download-ui`  
**Depends on:** PR #161 (merged)

## Goals

1. Show visible progress while `NxmHandoffService` downloads an nxm-linked file.
2. Drive Getting Started / wizard download indicators via `DownloadOrchestrationService` single-download tracking.
3. Reuse shared download progress reporting (`IProgress<DownloadProgress>`) from `DownloadModFromUrlAsync`.

## Out of scope

- macOS `.app` CI bundling
- Desktop E2E
- Full `DownloadProgressWindow` parity for nxm hand-offs

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~DownloadOrchestrationSingleDownload"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~Nxm"
```
