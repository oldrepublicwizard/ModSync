---
title: modsync protocol OS registration
type: feature
status: active
date: 2026-07-13
origin: docs/brainstorms/2026-07-13-modsync-protocol-requirements.md
related: docs/knowledgebase/modsync-protocol-handler.md
---

# Plan: modsync:// OS registration (follow-up)

Phase 2 of the "Install with ModSync" build-link track. Phase 1 (parse + CLI + single-instance handoff + tests) is shipped separately.

## Goal

Register ModSync as the OS handler for the `modsync://` URL scheme so browsers and other apps can open build deep links without a manual `--modsync=` launch.

## Context

`[REPO]` `nxm://` already has a full registration stack (`NxmProtocolRegistrationService`, Settings toggle, `NxmHandlerProbe`, macOS `Info.plist`). Reuse that phase model rather than inventing a second registration architecture.

`[REPO]` Phase 1 already accepts `--modsync=` / positional `modsync://...`, forwards over the single-instance pipe, and buffers in `ModSyncHandoffQueue`. OS registration only needs to launch the same CLI surface the OS already uses for nxm (`"<exe>" "%1"` / desktop `Exec=`).

## Implementation units (suggested)

### U1. `ModSyncProtocolRegistrationService`

Mirror `NxmProtocolRegistrationService`:

- Windows: `HKCU\Software\Classes\modsync` via `reg.exe` → open command with `--modsync="%1"` or bare `"%1"`.
- Linux: `~/.local/share/applications/modsync-protocol.desktop` with `MimeType=x-scheme-handler/modsync;` + `xdg-mime default`.
- macOS: add `modsync` to `CFBundleURLTypes` alongside `nxm` in app-bundle `Info.plist` (declarative; requires `ModSync.app`).

Pure builders (`BuildDesktopFileContent`, `BuildWindowsRegCommands`) must be unit-tested like nxm.

### U2. Settings preference

- Persist `registerModSyncProtocolHandler` in `AppSettings` / `MainConfig`.
- Settings → Download (or a dedicated "Install with ModSync") checkbox parallel to the nxm toggle.
- On save / startup drift repair: `Register()` / `Unregister()` like nxm Phase 3.

### U3. Handler probe + conflict UX (optional, later phase)

- Read-only probe for who owns `modsync://` (usually only ModSync; conflict risk is low vs nxm/MO2/Vortex).
- Confirm before overwrite only if another handler is detected.

### U4. MainWindow consumption (can ship with or before U1)

- `ModSyncHandoffService` drains `ModSyncHandoffQueue`, parses via `ModSyncUrl.TryParse`, fetches the instruction URL over http(s), and loads it through the existing instruction-file pipeline (wizard preload / Getting Started load).
- Fail closed on non-http(s) or parse failure; surface a clear error dialog.

## Out of scope for this follow-up plan body

- Changing the Phase 1 URL grammar.
- Publishing builds to a ModSync-hosted CDN (links may point at GitHub raw / any https host).
- Desktop E2E browser-click validation (still recommended after U1+U4).

## Verification (when implemented)

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~ModSyncUrl|FullyQualifiedName~ModSyncProtocol|FullyQualifiedName~CLIArgumentsModSync"
```

## Dependencies

- Phase 1 parse/CLI/handoff (this repo, already landed).
- Prefer modeling after plans 113–119 under `docs/plans/2026-06-*-nxm-*.md`.
