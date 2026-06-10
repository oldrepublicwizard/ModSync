---
title: "test: headless DownloadOrchestrationService coverage"
status: shipped
pr: pending
---

# test: headless DownloadOrchestrationService coverage

## Problem

`DownloadOrchestrationService` coordinates GUI download sessions but had no dedicated headless tests. `DownloadQueueHeadlessTests` covers `DownloadProgressWindow` queue behavior only.

## Solution

Add `DownloadOrchestrationServiceTests` for constructor guards, idle initial state, cancel state-change signaling, no-URL single-component guard, and null-safe single-component entry points.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter DownloadOrchestrationService`
