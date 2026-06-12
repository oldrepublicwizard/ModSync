---
title: "test: ThemeService style path and apply coverage"
status: shipped
pr: pending
---

# test: ThemeService style path and apply coverage

## Problem

`ThemeService` maps theme enums to Avalonia style resources and applies themes for Getting Started theme buttons but lacked dedicated tests beyond MainWindow integration checks.

## Solution

Add `ThemeServiceTests` for `GetStylePathForTheme`, per-theme `ApplyTheme` metadata, and null/empty path guard behavior.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ThemeService`
