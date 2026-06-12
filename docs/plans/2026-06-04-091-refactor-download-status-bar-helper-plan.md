---
title: "refactor: extend download indicator helper for wizard status bar"
status: shipped
pr: stacks #090
---

# refactor: extend download indicator helper for wizard status bar

## Problem

Plan `090` centralized Getting Started and wizard sidebar download labels; `WizardHostControl.UpdateDownloadStatusBarUI` and MainWindow running-animation text still duplicated strings.

## Solution

Extend `DownloadIndicatorUiHelper` with wizard status bar text/icon formatters and `FormatRunningAnimationText`; wire `WizardHostControl` status bar and MainWindow animation timer.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter DownloadIndicatorUiHelper`
