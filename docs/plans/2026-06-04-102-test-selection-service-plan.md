---
title: "test: SelectionService bulk select coverage"
status: shipped
pr: pending
---

# test: SelectionService bulk select coverage

## Problem

`SelectionService` drives select-all, deselect-all, select-by-tier, and select-by-category actions in the mod list but had no dedicated tests.

## Solution

Add `SelectionServiceTests` covering constructor validation, bulk selection helpers, and tier/category filtering behavior.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter SelectionService`
