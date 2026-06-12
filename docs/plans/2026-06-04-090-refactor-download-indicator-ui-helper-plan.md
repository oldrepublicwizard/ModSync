---
title: "refactor: shared download indicator UI helper"
status: shipped
pr: pending
---

# refactor: shared download indicator UI helper

## Problem

`MainWindow.UpdateDownloadIndicators` and `WizardHostControl.UpdateDownloadSidebarUI` duplicated download LED brush selection and progress label strings.

## Solution

Add `DownloadIndicatorUiHelper` with shared formatting and `ApplyGettingStartedTabIndicators`; wire MainWindow Getting Started tab and wizard sidebar to use it. Unit tests cover label formatting without Avalonia headless setup.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter DownloadIndicatorUiHelper`
