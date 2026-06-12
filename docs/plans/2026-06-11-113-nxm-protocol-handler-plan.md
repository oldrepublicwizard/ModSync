---
title: nxm protocol handler
type: feature
status: completed
date: 2026-06-11
---

# Plan 113: nxm:// protocol handler + Nexus "Download with Manager" integration

Phase 1 of the Vortex/MO2 feature-parity roadmap.

## Goal

Clicking "Mod Manager Download" on a Nexus Mods page hands the `nxm://` URL to ModSync,
which resolves it to a direct download via the Nexus API and queues it — including for
free (non-Premium) users, because nxm URLs carry a one-time `key`/`expires` pair that
`download_link.json` accepts without Premium.

## Scope

### ModSync.Core

- `Services/Download/NxmUrl.cs` — immutable parser for
  `nxm://{gameDomain}/mods/{modId}/files/{fileId}?key=...&expires=...&user_id=...`.
  - `NxmUrl.TryParse(string, out NxmUrl)` and `NxmUrl.IsNxmUrl(string)`.
  - `ToModPageUrl()` / `ToFileUrl()` produce the canonical `https://www.nexusmods.com/...` URLs.
  - `MatchesNexusUrl(string)` — true when a nexusmods.com URL refers to the same game + mod,
    used to route an incoming nxm URL to a loaded `ModComponent` via its `ResourceRegistry` keys.
- `NexusModsDownloadHandler`:
  - `CanHandle` accepts `nxm://` URLs.
  - `DownloadAsync` branches to a dedicated nxm path: fetch the file's metadata from
    `files/{fileId}.json` (filename), then `download_link.json?key=...&expires=...`
    (works for free users; falls back to API-key-only resolution when no key/expires present),
    then streams the file with the existing temp-file + atomic-move flow.

### ModSync.GUI

- `CLIArguments` — recognizes a bare `nxm://...` positional argument and `--nxm=<url>`,
  exposed as `CLIArguments.NxmUrl`.
- `Services/SingleInstanceService.cs` — named-pipe single-instance coordination:
  - `TryBecomePrimary()` claims the per-user pipe; the primary listens for forwarded args.
  - `SendToPrimaryAsync(message)` lets a secondary instance forward its nxm URL and exit.
  - `MessageReceived` event; received URLs also flow into `NxmHandoffQueue` so URLs that
    arrive before the main window is ready are not lost.
- `Services/NxmHandoffQueue.cs` — tiny static queue decoupling IPC arrival from GUI readiness.
- `Services/NxmProtocolRegistrationService.cs` — OS registration for the `nxm` scheme:
  - Windows: `HKCU\Software\Classes\nxm` via `reg.exe` (no registry package dependency).
  - Linux: `~/.local/share/applications/modsync-nxm.desktop` with
    `MimeType=x-scheme-handler/nxm;` plus `xdg-mime default`.
  - `BuildDesktopFileContent` and `BuildWindowsRegCommands` are pure and unit-tested.
- `Program.Main` — when launched with an nxm URL and a primary instance is already running,
  forwards the URL over the pipe and exits; otherwise becomes primary and starts listening.

### Tests (`src/ModSync.Tests`, single project)

- `NxmUrlTests` — parse round-trips, invalid inputs, query handling, `MatchesNexusUrl`.
- `NxmProtocolRegistrationServiceTests` — desktop-file/reg-command content generation.
- `SingleInstanceServiceTests` — primary claim, secondary forward, message receipt.
- `NexusModsDownloadHandlerTests` — `CanHandle` accepts nxm URLs; non-nexus unaffected.

## Out of scope (Phase 1 follow-up slice)

- MainWindow consumption of `NxmHandoffQueue` (auto-opening `SingleModDownloadDialog`
  for the matching component) — needs a real desktop validation session.
- Settings toggle UI for register/unregister.
- macOS protocol registration (requires app-bundle Info.plist work).

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~Nxm|FullyQualifiedName~SingleInstance"
```
