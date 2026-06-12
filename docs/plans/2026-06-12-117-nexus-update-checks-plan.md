---
title: "feat: Nexus API client, resource version tracking, and mod update check service"
type: feat
status: completed
date: 2026-06-12
origin: .cursor/plans/vortex_mo2_feature_parity_roadmap_5a9d8745.plan.md (Phase 2, first sub-slice)
branch: feat/nexus-update-checks
---

# feat: Nexus API client, resource version tracking, and mod update check service

First sub-slice of Phase 2 (mod version tracking + update checks) of the Vortex/MO2
feature-parity roadmap.

## Goal

Know when a tracked Nexus Mods component has a newer version available, without any
GUI changes and without touching the existing download pipeline.

## Scope

1. `src/ModSync.Core/Services/Download/NexusApiClient.cs` — new standalone, reusable
   Nexus Mods REST API client:
   - Constructor takes `HttpClient` + API key (injectable handler for tests).
   - `GetModInfoAsync(gameDomain, modId)` -> `NexusModInfo` (Name, Version,
     UpdatedTimestamp, Available).
   - `GetModFilesAsync(gameDomain, modId)` -> `List<NexusModFile>` (FileId, FileName,
     Version, CategoryName, Md5).
   - `Md5SearchAsync(gameDomain, md5)` -> `List<NexusMd5Result>`.
   - `ValidateAsync()` -> user validation via `/users/validate.json`.
   - 429 handling with Retry-After (single retry, matching the convention in
     `NexusModsDownloadHandler.MakeApiRequestAsync`).
   - Tracks last-seen `X-RL-Daily-Remaining` / `X-RL-Hourly-Remaining` response
     headers, exposed as `DailyRemaining` / `HourlyRemaining` properties.
   - Sends `apikey`, `Application-Name`, `Application-Version`, `User-Agent`, and
     `Accept` headers per existing conventions.

2. `ResourceMetadata` (in `src/ModSync.Core/ModComponent.cs`, ResourceMetadata class
   only — no other edits to that file): new plain serializable properties
   `ModVersion`, `LatestKnownVersion`, `LastUpdateCheck` (`DateTime?`), and
   `UpdateAvailable` (`bool`).

3. `src/ModSync.Core/Services/ModUpdateCheckService.cs` — batch update checker:
   - `CheckForUpdatesAsync(IEnumerable<ModComponent>, CancellationToken)`.
   - Finds nexusmods.com URLs among `ResourceRegistry` keys via the existing
     `nexusmods\.com/([^/]+)/mods/(\d+)` pattern.
   - Queries mod info once per unique (gameDomain, modId) across all components.
   - Sets `ModVersion` when absent, updates `LatestKnownVersion`,
     `LastUpdateCheck`, `UpdateAvailable` on matching `ResourceMetadata` entries.
   - Stops early when the client's rate-limit remaining budget reaches zero.
   - Returns `ModUpdateCheckResult` (checked count, updates found, skipped, errors).

4. Tests in `src/ModSync.Tests` (NUnit, no real network — fake
   `HttpMessageHandler`):
   - `NexusApiClientTests.cs`: mod info parse, files parse, md5 search,
     429-then-success retry, rate-limit header tracking.
   - `ModUpdateCheckServiceTests.cs`: version fields updated, update detected on
     version mismatch, non-Nexus URLs skipped, API call deduplication across
     components, rate-limit early stop.

5. Knowledgebase note `docs/knowledgebase/update-checking.md` + index entry.

## Out of scope (follow-ups for later Phase 2 slices)

- Refactoring `NexusModsDownloadHandler` to use `NexusApiClient` (that file is
  actively being modified on another branch; do not touch it or
  `DownloadHandlerFactory.cs`).
- Endorsement support (`endorse`/`abstain` endpoints, `EndorsementState` metadata)
  and any endorsement GUI.
- Update badges on `ModListItem`/`ModListSidebar`, "Check for updates" GUI action,
  or any other GUI change.
- Persisting check results via `DownloadCacheService`.

## Constraints

- `ModSync.Core` compiles as C# 7.3 and multi-targets net48 + net9.0; new code must
  stay within that language level.
- `ResourceRegistry` getter returns a defensive copy of the dictionary, but the
  `ResourceMetadata` values are shared references, so mutating them in place is
  the supported way to persist results.

## Verification

- `dotnet build ModSync.sln --configuration Debug`
- `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter
  "FullyQualifiedName~NexusApiClient|FullyQualifiedName~ModUpdateCheck"`
