---
title: "test: DialogService and FileSystemService basics"
status: shipped
pr: pending
---

# test: DialogService and FileSystemService basics

## Problem

`DialogService` path-folder helper and `FileSystemService` watcher/dispose lifecycle had no dedicated tests.

## Solution

Add `DialogServiceTests` and `FileSystemServiceTests` for constructor validation, empty-path handling, and safe dispose/stop paths.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~DialogServiceTests|FullyQualifiedName~FileSystemServiceTests"
