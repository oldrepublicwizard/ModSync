# modsync:// protocol handler

Sources: `src/ModSync.Core/Services/Protocol/ModSyncUrl.cs`,
`ModSyncInstructionFetcher.cs`, `src/ModSync.Core/Ports/Protocol/`,
`src/ModSync.GUI/Services/ModSyncHandoffQueue.cs`,
`ModSyncHandoffService.cs`, `ModSyncProtocolRegistrationService.cs`,
`CLIArguments.ModSyncProtocolUrl`, `Program.cs`, `SingleInstanceService`.

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
`SingleInstanceServiceTests` (modsync pipe handoff).
