---
title: nxm handoff MainWindow consumption
type: feature
status: completed
date: 2026-06-12
origin: docs/plans/2026-06-11-113-nxm-protocol-handler-plan.md
---

# Plan 114: MainWindow nxm handoff consumption

## Summary

Close Phase 1 gap: drain `NxmHandoffQueue` after the main window is ready, match nxm URLs to loaded `ModComponent` entries, download via the nxm:// URL (not https registry keys), and place the file in the mod workspace.

## Requirements

- R1. `NxmComponentResolver` in Core — testable match of `NxmUrl` against `ResourceRegistry` keys via `MatchesNexusUrl`.
- R2. `NxmHandoffService` in GUI — subscribe to `UrlEnqueued`, drain on startup, marshal to UI thread.
- R3. Download uses `DownloadOrchestrationService.DownloadModFromUrlAsync(nxmOriginalUrl, component)` — never `SingleModDownloadDialog` alone.
- R4. Successful downloads copy from temp into `MainConfig.SourcePath` with user-visible feedback.
- R5. Re-process queue after instruction file load completes (CLI preload race).
- R6. Headless tests for resolver; no desktop-only gate for merge.

## Out of scope

- Settings toggle for protocol registration.
- macOS registration.
- Download cache / ResourceRegistry filename updates after nxm download.

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~NxmComponentResolver"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~NxmUrlTests"
```
