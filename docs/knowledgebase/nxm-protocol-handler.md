# nxm:// protocol handler

`[REPO]` How a Nexus Mods "Mod Manager Download" click reaches ModSync: URL parsing, OS scheme registration, single-instance hand-off, and the API download path that works for free users.

Sources: `src/ModSync.Core/Services/Download/NxmUrl.cs`, `src/ModSync.Core/Services/Download/NexusModsDownloadHandler.cs`, `src/ModSync.GUI/Services/SingleInstanceService.cs`, `src/ModSync.GUI/Services/NxmHandoffQueue.cs`, `src/ModSync.GUI/Services/NxmProtocolRegistrationService.cs`, `src/ModSync.GUI/CLIArguments.cs`, `src/ModSync.GUI/Program.cs`. Plan: `docs/plans/2026-06-11-113-nxm-protocol-handler-plan.md`.

## URL model

`[REPO]` **`NxmUrl`** parses `nxm://{gameDomain}/mods/{modId}/files/{fileId}?key=...&expires=...&user_id=...` (KOTOR domains: `kotor`, `kotor2`). The `key`/`expires` pair is a one-time authorization that `download_link.json` accepts for non-Premium users. `MatchesNexusUrl(string)` compares game domain + mod id against a nexusmods.com URL so an incoming nxm link can be routed to a loaded `ModComponent` via its `ResourceRegistry` keys.

## Pipeline

| Stage | Component | Behavior |
|-------|-----------|----------|
| OS registration | `NxmProtocolRegistrationService` | Windows: `HKCU\Software\Classes\nxm` via `reg.exe`; Linux: `~/.local/share/applications/modsync-nxm.desktop` + `xdg-mime default`; macOS: `CFBundleURLTypes` in app bundle `Info.plist` (declarative, requires `ModSync.app`) |
| Launch | `CLIArguments` | `--nxm=<url>` or bare positional `nxm://...` sets `CLIArguments.NxmUrl` |
| Single instance | `SingleInstanceService` | Per-user named pipe (System.IO.Pipes, cross-platform). Primary claims pipe + listens; secondary forwards the URL and exits (`Program.Main`) |
| Buffering | `NxmHandoffQueue` | Static thread-safe queue + event; URLs arriving before the main window is ready are retained |
| Download | `NexusModsDownloadHandler` | `CanHandle` accepts nxm URLs. Resolves filename via `files/{fileId}.json`, download URI via `download_link.json?key=...&expires=...` (apikey header added when configured), then reuses the shared temp-file + atomic-move streaming flow |

`[REPO]` Handler priority order in `DownloadHandlerFactory` is unchanged; nxm URLs are not HTTP(S) so only `NexusModsDownloadHandler` claims them.

## MainWindow hand-off (Plan 114)

`[REPO]` `NxmHandoffService` drains `NxmHandoffQueue` on window open (after CLI instruction preload when applicable), subscribes to `UrlEnqueued`, matches via `NxmComponentResolver`, downloads with `DownloadOrchestrationService.DownloadModFromUrlAsync` using the **nxm://** URL, and copies the file into `MainConfig.SourcePath`.

`[UI]` Desktop validation still recommended for OS registration + browser click end-to-end.

## Phase 3 (Plan 115)

`[REPO]` **Settings → Download Settings** checkbox `Register ModSync as Nexus Mod Manager` persists `registerNxmProtocolHandler` in `AppSettings`. On save, calls `NxmProtocolRegistrationService.Register()` / `Unregister()`. On startup, `MainWindow.LoadSettings` re-registers when the preference is on but OS state drifted (e.g. after an app update).

`[REPO]` After a successful nxm hand-off copy, `NxmHandoffService` calls `DownloadCacheService.UpdateResourceMetadataWithFilenamesAsync` with the matched **HTTPS** registry URL from `NxmComponentResolver.TryResolve`.

`[UI]` Desktop E2E still recommended for OS registration + browser click.

## Phase 4 (Plan 116)

`[REPO]` Secondary non-nxm launches forward `ApplicationLaunchCoordinator.ActivateMessage` over the named pipe and exit; primary raises `ActivationRequested` so `MainWindow` restores/focuses. Dev escape hatch: `--allow-multiple-instances`.

`[REPO]` macOS: both `Info.plist` files declare `CFBundleURLTypes` → `nxm`. Settings shows an informational **Nexus Mod Manager Download** status block instead of the Win/Linux toggle. `Register()`/`IsRegistered()` on macOS reflect app-bundle execution only.

`[UI]` Desktop E2E still recommended (browser click, single-instance, macOS `.app` bundle).

## Still deferred

`[REPO]` Handler conflict detection; macOS release `.app` bundling in CI; in-progress nxm download UI integration.

## Tests

`[REPO]` `NxmUrlTests`, `NxmProtocolRegistrationServiceTests`, `SingleInstanceServiceTests`, `NexusModsDownloadHandlerTests` in `src/ModSync.Tests`.
