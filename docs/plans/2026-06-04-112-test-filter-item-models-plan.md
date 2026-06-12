---
title: "test: TierFilterItem and SelectionFilterItem models"
status: shipped
pr: pending
---

# test: TierFilterItem and SelectionFilterItem models

## Problem

Filter sidebar models format tier/category labels and selection state but had no dedicated tests beyond `FilterUIService` integration coverage.

## Solution

Add `FilterItemModelTests` for `DisplayText`, `EffectiveSelection`, and selection toggles.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter FilterItemModel
