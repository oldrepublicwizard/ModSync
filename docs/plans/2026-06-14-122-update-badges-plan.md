---
title: "feat: Nexus update badges and check-for-updates menu action"
type: feat
status: completed
date: 2026-06-14
origin: docs/plans/2026-06-12-117-nexus-update-checks-plan.md (Phase 2, GUI slice)
branch: feat/update-badges-gui
---

# feat: Nexus update badges and check-for-updates menu action

Second sub-slice of Phase 2 (mod version tracking + update checks) of the Vortex/MO2
feature-parity roadmap.

## Goal

Surface `ModUpdateCheckService` results in the mod list and expose a user-triggered
"Check for Nexus Updates" action without changing download or install pipelines.

## Landed

- `ModComponent.NexusUpdateAvailable` + `NotifyNexusUpdateStateChanged()` for list binding.
- `ModListItem` "Update" badge when any tracked Nexus resource has `UpdateAvailable`.
- `MenuBuilderService` common menu item runs `ModUpdateCheckService` and refreshes badges.
- Knowledgebase and living-plan delta updated.

## Out of scope

- Sidebar-only badges, endorsement UI, persisting check results via `DownloadCacheService`.
- Migrating `NexusModsDownloadHandler` onto `NexusApiClient`.
- Desktop GUI validation of badge rendering (headless agent skipped).

## Tests

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~ModUpdateCheck"
```
