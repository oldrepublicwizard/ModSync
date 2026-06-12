# Plan 117 — nxm Phase 5 handler conflict detection

**Status:** in progress  
**Branch:** `feat/nxm-phase5-handler-conflict`  
**Depends on:** PR #159 (merged)

## Goals

1. Read-only probe for who owns `nxm://` on Windows and Linux (`NxmHandlerProbe`).
2. Settings shows a separate OS status line (distinct from the preference helper).
3. Confirm before takeover when MO2/Vortex (or another handler) is active.
4. Re-apply registration when ModSync is registered but not the default handler.
5. Fixture-based unit tests for pure parsers and classification.

## Out of scope

- In-progress nxm download UI
- macOS handler conflict probing (informational bundle status only)
- macOS `.app` CI bundling
- Desktop E2E

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~NxmHandlerProbe"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~Nxm"
```
