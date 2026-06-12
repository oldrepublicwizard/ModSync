---
title: "test: ValidationService component validity helpers"
status: shipped
pr: pending
---

# test: ValidationService component validity helpers

## Problem

`ValidationService` gates install eligibility and error messaging but only had indirect `IsStep1Complete` coverage in headless UI tests.

## Solution

Add `ValidationServiceTests` for `IsComponentValidForInstallation` and `GetComponentErrorDetails`.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ValidationServiceTests
