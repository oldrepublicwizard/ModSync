# modsync:// protocol handler

`[REPO]` How an "Install with ModSync" deep link reaches the app: URL parsing, CLI launch args, and single-instance hand-off. OS scheme registration is planned but not shipped in this slice.

Sources: `src/ModSync.Core/Services/Protocol/ModSyncUrl.cs`, `src/ModSync.GUI/CLIArguments.cs`, `src/ModSync.GUI/Program.cs`, `src/ModSync.GUI/Services/SingleInstanceService.cs`, `src/ModSync.GUI/Services/ModSyncHandoffQueue.cs`, `src/ModSync.GUI/Services/ApplicationLaunchCoordinator.cs`. Requirements: [docs/brainstorms/2026-07-13-modsync-protocol-requirements.md](../brainstorms/2026-07-13-modsync-protocol-requirements.md). Follow-up: [docs/plans/2026-07-13-002-feat-modsync-protocol-os-registration-plan.md](../plans/2026-07-13-002-feat-modsync-protocol-os-registration-plan.md). Related shipped pattern: [nxm-protocol-handler.md](nxm-protocol-handler.md).

## URL model

`[REPO]` **`ModSyncUrl`** parses build deep links that point at an instruction file over http(s):

| Form | Example |
|------|---------|
| Action host + `url` | `modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml` |
| `instruction` alias + optional `game` | `modsync://open?instruction=https%3A%2F%2F...&game=kotor` |
| Game host + action path | `modsync://kotor/install?url=https%3A%2F%2F...` |
| Action host + game path | `modsync://install/kotor2?url=https%3A%2F%2F...` |

Rules:

- Scheme must be `modsync://`.
- Action is `install` or `open` (same meaning for now: load the linked instruction file).
- Optional game is `kotor` or `kotor2` (host, path segment, or `game=` query; conflicting values fail parse).
- Instruction location must be an absolute `http` or `https` URL (`url` or `instruction` query). Local `file://` and bare paths are rejected in this slice.

`[SYNTH]` This is the share-a-build counterpart to Nexus `nxm://` file downloads: one link hands a whole instruction file into ModSync.

## Pipeline (shipped)

| Stage | Component | Behavior |
|-------|-----------|----------|
| Parse | `ModSyncUrl` | `IsModSyncUrl` / `TryParse` in Core |
| Launch | `CLIArguments` | `--modsync=<url>` or bare positional `modsync://...` sets `CLIArguments.ModSyncUrl` |
| Single instance | `SingleInstanceService` + `ApplicationLaunchCoordinator` | Same named-pipe primary/secondary model as nxm; secondary with a protocol URL uses `ForwardProtocolUrlAndExit` |
| Buffering | `ModSyncHandoffQueue` | Thread-safe queue + `UrlEnqueued` for URLs that arrive before the main window is ready |

`[REPO]` `Program.Main` enqueues CLI `nxm://` into `NxmHandoffQueue` and CLI `modsync://` into `ModSyncHandoffQueue`. Pipe messages are routed by scheme prefix the same way.

## Not shipped yet

| Stage | Status |
|-------|--------|
| OS registration (Windows/Linux/macOS) | Deferred — see follow-up plan |
| MainWindow / wizard consumption of `ModSyncHandoffQueue` | Deferred (mirror `NxmHandoffService`: download/open instruction URL, preload into load flow) |
| Settings toggle | Deferred with OS registration |

## Relationship to nxm://

| | `nxm://` | `modsync://` |
|--|----------|--------------|
| Purpose | Nexus "Mod Manager Download" for one file | Share a whole build (instruction file) |
| Parse type | `NxmUrl` | `ModSyncUrl` |
| Handoff queue | `NxmHandoffQueue` | `ModSyncHandoffQueue` |
| OS registration | Shipped (`NxmProtocolRegistrationService`) | Planned |
| MainWindow consumer | `NxmHandoffService` | Not yet |

## Tests

`[REPO]` `ModSyncUrlTests`, `CLIArgumentsModSyncTests`, `ApplicationLaunchCoordinatorTests`, and `SingleInstanceServiceTests` (modsync pipe → `ModSyncHandoffQueue`) in `src/ModSync.Tests`.
