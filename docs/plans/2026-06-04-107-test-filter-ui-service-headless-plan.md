---
title: "test: headless FilterUIService tier and category filters"
status: shipped
pr: pending
---

# test: headless FilterUIService tier and category filters

## Problem

`FilterUIService` builds mod-list tier/category filters and applies bulk selections but had no dedicated tests.

## Solution

Add `FilterUIServiceHeadlessTests` for filter initialization, `SelectByTier`, and `ApplyCategorySelections` with dispatcher pumping.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter FilterUIService`
