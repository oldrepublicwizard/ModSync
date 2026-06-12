# Mod update checking

How ModSync knows whether a tracked Nexus Mods component has a newer version available.

## Components

| Piece | Location | Role |
|-------|----------|------|
| `NexusApiClient` | `src/ModSync.Core/Services/Download/NexusApiClient.cs` | `[REPO]` Standalone typed client for the Nexus REST API (`/games/{domain}/mods/{id}.json`, `files.json`, `md5_search`, `/users/validate.json`). Handles 429 Retry-After (single retry) and tracks `X-RL-Daily-Remaining` / `X-RL-Hourly-Remaining` as `DailyRemaining` / `HourlyRemaining`. |
| `ResourceMetadata` version fields | `src/ModSync.Core/ModComponent.cs` | `[REPO]` `ModVersion` (baseline), `LatestKnownVersion`, `LastUpdateCheck`, `UpdateAvailable` — plain serializable properties on each `ResourceRegistry` entry. |
| `ModUpdateCheckService` | `src/ModSync.Core/Services/ModUpdateCheckService.cs` | `[REPO]` Batch checker: extracts `(gameDomain, modId)` from `nexusmods.com/{domain}/mods/{id}` URLs in each component's `ResourceRegistry`, fetches mod info once per unique mod across all components, writes results onto the metadata, stops early when the rate-limit budget hits zero. |
| `ModComponent.NexusUpdateAvailable` | `src/ModSync.Core/ModComponent.cs` | `[REPO]` Computed flag for list binding; call `NotifyNexusUpdateStateChanged()` after mutating metadata. |
| Mod list badge | `src/ModSync.GUI/Controls/ModListItem.axaml` | `[REPO]` "Update" badge when `NexusUpdateAvailable` is true. |
| Menu action | `src/ModSync.GUI/Services/MenuBuilderService.cs` | `[REPO]` **Check for Nexus Updates** runs the batch checker (requires API key) and refreshes badges. |

## Semantics

- First check on a resource with no stored `ModVersion` adopts the provider version as the baseline (`UpdateAvailable` stays false).
- Subsequent checks set `UpdateAvailable` when the provider version differs (ordinal, case-insensitive) from `ModVersion`; `ModVersion` itself is never overwritten by a check.
- Non-Nexus URLs are counted as skipped and never hit the network.
- Fetch failures are cached per mod for the run (one error entry, no retries within the same run) and do not abort the remaining mods.
- `ModUpdateCheckResult` reports `CheckedCount`, `SkippedCount`, `UpdatesFound`, `Errors`, and `RateLimitReached`.

## Gotchas

- `ModComponent.ResourceRegistry` getter returns a defensive copy of the dictionary, but the `ResourceMetadata` values are shared references; the service mutates them in place — that is the supported persistence path. `[REPO]`
- `ModSync.Core` compiles as C# 7.3 and multi-targets net48; keep additions to these classes within that language level. `[REPO]`
- `NexusModsDownloadHandler` still has its own inline API code; migrating it onto `NexusApiClient` is a planned follow-up, as are endorsements and persisting check results via `DownloadCacheService`. `[REPO]`

## Tests

- `src/ModSync.Tests/NexusApiClientTests.cs` — JSON parsing, headers, 429 retry, rate-limit tracking (fake `HttpMessageHandler`, no network).
- `src/ModSync.Tests/ModUpdateCheckServiceTests.cs` — version adoption, update detection, skip/dedupe/error/rate-limit behavior.

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~NexusApiClient|FullyQualifiedName~ModUpdateCheck"
```
