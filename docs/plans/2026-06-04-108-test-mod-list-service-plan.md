---
title: "test: ModListService search and list UI helpers"
status: shipped
pr: pending
---

# test: ModListService search and list UI helpers

## Problem

`ModListService` filters the mod list and updates list/count UI but had no dedicated tests.

## Solution

Add `ModListServiceTests` for `FilterModList`, `PopulateModList`, and `UpdateModCounts`.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ModListService`
