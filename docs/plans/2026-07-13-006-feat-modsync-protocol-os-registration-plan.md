---
title: modsync protocol OS registration
type: feature
status: active
date: 2026-07-13
origin: docs/knowledgebase/modsync-protocol-handler.md
---

# Plan: modsync:// OS registration (follow-up)

Phase 2 of the "Install with ModSync" build-link track. Phase 1 (parse + CLI + single-instance handoff + tests) ships separately.

## Goal

Register ModSync as the OS handler for `modsync://` and drain `ModSyncHandoffQueue` in the GUI (fetch instruction URL + load).

## Units

1. `ModSyncProtocolRegistrationService` mirroring `NxmProtocolRegistrationService` (Win/Linux/macOS).
2. Settings preference `registerModSyncProtocolHandler`.
3. Optional handler probe / conflict UX.
4. `ModSyncHandoffService` MainWindow consumption.

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~ModSyncUrl|FullyQualifiedName~ModSyncProtocol|FullyQualifiedName~CLIArgumentsModSync"
```
