# modsync:// protocol handler

`[REPO]` How an "Install with ModSync" deep link reaches the app: URL parsing, CLI
launch args, single-instance hand-off, fetch/consume, and OS scheme registration.

Sources: `src/ModSync.Core/Services/Protocol/ModSyncUrl.cs`,
`ModSyncInstructionFetcher.cs`, `src/ModSync.Core/Ports/Protocol/`,
`src/ModSync.GUI/Services/ModSyncHandoffQueue.cs`,
`ModSyncHandoffService.cs`, `ModSyncProtocolRegistrationService.cs`,
`CLIArguments.ModSyncProtocolUrl`, `Program.cs`, `SingleInstanceService`,
`ApplicationLaunchCoordinator`. Related: [nxm-protocol-handler.md](nxm-protocol-handler.md).
OS registration follow-up notes:
`docs/plans/2026-07-13-006-feat-modsync-protocol-os-registration-plan.md`.

## URL model

`[REPO]` **`ModSyncUrl`** parses build deep links pointing at an instruction file over http(s):

| Form | Example |
|------|---------|
| Action host + `url` | `modsync://install?url=https%3A%2F%2Fexample.com%2Fbuild.toml` |
| `instruction` + optional `game` | `modsync://open?instruction=https%3A%2F%2F...&game=kotor` |
| Game host + action path | `modsync://kotor/install?url=https%3A%2F%2F...` |
| Action host + game path | `modsync://install/kotor2?url=https%3A%2F%2F...` |

Rules: scheme `modsync://`; action `install` or `open`; optional game `kotor`/`kotor2`;
instruction must be absolute http(s). Local `file://` and bare paths are rejected.

## Status

| Layer | Location | Notes |
|-------|----------|-------|
| Parse | `ModSyncUrl` / `ModSyncProtocolHandler` | http(s) instruction URL only |
| CLI / queue | `--modsync=` or bare `modsync://` → `ModSyncHandoffQueue` | Single-instance forward |
| Consume | `ModSyncHandoffService` | Fetch → temp file → `FileLoadingService` → activate |
| OS registration | `ModSyncProtocolRegistrationService` | Win/Linux/macOS builders + Register; Settings checkbox deferred |

Accepted forms: `modsync://install?url=…`, `modsync://open?instruction=…&game=kotor`,
`modsync://kotor/install?url=…`.

Tests: `ModSyncUrlTests`, `CLIArgumentsModSyncTests`, `ModSyncProtocolConsumeTests`,
`ApplicationLaunchCoordinatorTests`, `SingleInstanceServiceTests` (modsync pipe handoff).
