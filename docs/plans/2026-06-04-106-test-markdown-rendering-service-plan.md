---
title: "test: MarkdownRenderingService render helpers"
status: shipped
pr: pending
---

# test: MarkdownRenderingService render helpers

## Problem

`MarkdownRenderingService` renders component markdown into Avalonia text blocks but only had indirect plain-text coverage via `MarkdownRendererPlainTextTests`.

## Solution

Add `MarkdownRenderingServiceTests` for string/inline extraction and headless TextBlock rendering paths.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter MarkdownRenderingService`
