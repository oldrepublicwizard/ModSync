---
title: "test: headless ApplyGettingStartedTabIndicators coverage"
status: shipped
pr: pending
stacks_on: "#137"
---

# test: headless ApplyGettingStartedTabIndicators coverage

## Problem

`DownloadIndicatorUiHelper` unit tests covered string formatters only; `ApplyGettingStartedTabIndicators` and LED brush wiring had no Avalonia headless coverage.

## Solution

Add `DownloadIndicatorUiHelperHeadlessTests` for Getting Started tab indicator apply behavior and LED brush selection. Stacks on #137.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter DownloadIndicatorUiHelper`
