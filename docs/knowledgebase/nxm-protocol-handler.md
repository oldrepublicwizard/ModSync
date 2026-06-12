# nxm:// protocol handler

`[REPO]` How a Nexus Mods "Mod Manager Download" click reaches ModSync: URL parsing, OS scheme registration, single-instance hand-off, and the API download path that works for free users.

Sources: `src/ModSync.Core/Services/Download/NxmUrl.cs`, `src/ModSync.Core/Services/Download/NexusModsDownloadHandler.cs`, `src/ModSync.GUI/Services/SingleInstanceService.cs`, `src/ModSync.GUI/Services/NxmHandoffQueue.cs`, `src/ModSync.GUI/Services/NxmProtocolRegistrationService.cs`, `src/ModSync.GUI/CLIArguments.cs`, `src/ModSync.GUI/Program.cs`. Plan: `docs/plans/2026-06-11-113-nxm-protocol-handler-plan.md`.

## URL model

`[REPO]` **`NxmUrl`** parses `nxm://{gameDomain}/mods/{modId}/files/{fileId}?key=...&expires=...&user_id=...` (KOTOR domains: `kotor`, `kotor2`). The `key`/`expires` pair is a one-time authorization that `download_link.json` accepts for non-Premium users. `MatchesNexusUrl(string)` compares game domain + mod id against a nexusmods.com URL so an incoming nxm link can be routed to a loaded `ModComponent` via its `ResourceRegistry` keys.

## Pipeline

| Stage | Component | Behavior |
|-------|-----------|----------|
| OS registration | `NxmProtocolRegistrationService` | Windows: `HKCU\Software\Classes\nxm` via `reg.exe`; Linux: `~/.local/share/applications/modsync-nxm.desktop` + `xdg-mime default`. Builders are pure/unit-tested; macOS out of scope |
| Launch | `CLIArguments` | `--nxm=<url>` or bare positional `nxm://...` sets `CLIArguments.NxmUrl` |
| Single instance | `SingleInstanceService` | Per-user named pipe (System.IO.Pipes, cross-platform). Primary claims pipe + listens; secondary forwards the URL and exits (`Program.Main`) |
| Buffering | `NxmHandoffQueue` | Static thread-safe queue + event; URLs arriving before the main window is ready are retained |
| Download | `NexusModsDownloadHandler` | `CanHandle` accepts nxm URLs. Resolves filename via `files/{fileId}.json`, download URI via `download_link.json?key=...&expires=...` (apikey header added when configured), then reuses the shared temp-file + atomic-move streaming flow |

`[REPO]` Handler priority order in `DownloadHandlerFactory` is unchanged; nxm URLs are not HTTP(S) so only `NexusModsDownloadHandler` claims them.

## MainWindow hand-off (Plan 114)

`[REPO]` `NxmHandoffService` drains `NxmHandoffQueue` on window open (after CLI instruction preload when applicable), subscribes to `UrlEnqueued`, matches via `NxmComponentResolver`, downloads with `DownloadOrchestrationService.DownloadModFromUrlAsync` using the **nxm://** URL, and copies the file into `MainConfig.SourcePath`.

`[UI]` Desktop validation still recommended for OS registration + browser click end-to-end.

## Still deferred

`[REPO]` Settings toggle for register/unregister; macOS `Info.plist` registration; automatic `ResourceRegistry` / cache updates after nxm download.

## Tests

`[REPO]` `NxmUrlTests`, `NxmProtocolRegistrationServiceTests`, `SingleInstanceServiceTests`, `NexusModsDownloadHandlerTests` in `src/ModSync.Tests`.
