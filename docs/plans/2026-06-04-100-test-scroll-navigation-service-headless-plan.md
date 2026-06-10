---
title: "test: headless ScrollNavigationService find helpers"
status: shipped
pr: pending
---

# test: headless ScrollNavigationService find helpers

## Problem

`ScrollNavigationService.FindControlRecursive` and `FindScrollViewer` support Getting Started jump-to-step navigation but had no dedicated tests.

## Solution

Add `ScrollNavigationServiceHeadlessTests` for null-parent handling, nested control discovery, and scroll viewer lookup.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ScrollNavigationService`
