---
title: "refactor: ValidationDisplayUiHelper for Getting Started summaries"
status: shipped
pr: pending
---

# refactor: ValidationDisplayUiHelper for Getting Started summaries

## Problem

`ValidationDisplayService` duplicated validation summary strings and error-navigation bounds inline in Getting Started validation UI.

## Solution

Add `ValidationDisplayUiHelper` for summary/error-counter formatting, invalid-component collection, and navigation bounds; wire `ValidationDisplayService` to delegate.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ValidationDisplayUiHelper`
