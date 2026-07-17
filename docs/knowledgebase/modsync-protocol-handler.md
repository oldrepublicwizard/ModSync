# modsync:// protocol handler

`[REPO]` How an "Install with ModSync" deep link reaches the app: URL parsing, CLI launch args, and single-instance hand-off. OS scheme registration is planned but not shipped in this slice.

Sources: `src/ModSync.Core/Services/Protocol/ModSyncUrl.cs`, `src/ModSync.GUI/CLIArguments.cs`, `src/ModSync.GUI/Program.cs`, `src/ModSync.GUI/Services/SingleInstanceService.cs`, `src/ModSync.GUI/Services/ModSyncHandoffQueue.cs`, `src/ModSync.GUI/Services/ApplicationLaunchCoordinator.cs`. Follow-up: `docs/plans/2026-07-13-006-feat-modsync-protocol-os-registration-plan.md`. Related: [nxm-protocol-handler.md](nxm-protocol-handler.md).

## URL model

`[REPO]` **`ModSyncUrl`** parses build deep links pointing at an instruction file over http(s):

| Form | Example |
|------|---------|
| Action host + `url` | `modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml` |
| `instruction` + optional `game` | `modsync://open?instruction=https%3A%2F%2F...&game=kotor` |
| Game host + action path | `modsync://kotor/install?url=https%3A%2F%2F...` |
| Action host + game path | `modsync://install/kotor2?url=https%3A%2F%2F...` |

Rules: scheme `modsync://`; action `install` or `open`; optional game `kotor`/`kotor2`; instruction must be absolute http(s). Local `file://` and bare paths are rejected.

## Pipeline (shipped)

| Stage | Component | Behavior |
|-------|-----------|----------|
| Parse | `ModSyncUrl` | `IsModSyncUrl` / `TryParse` |
| Launch | `CLIArguments` | `--modsync=<url>` or bare `modsync://...` → `ModSyncProtocolUrl` |
| Single instance | `SingleInstanceService` + `ApplicationLaunchCoordinator` | `ForwardProtocolUrlAndExit` for nxm/modsync |
| Buffering | `ModSyncHandoffQueue` | Queue + `UrlEnqueued` until MainWindow consumes (deferred) |

## Not shipped yet

OS registration, Settings toggle, MainWindow drain/fetch/load — see follow-up plan.

## Tests

`ModSyncUrlTests`, `CLIArgumentsModSyncTests`, `ApplicationLaunchCoordinatorTests`, `SingleInstanceServiceTests` (modsync pipe path).
