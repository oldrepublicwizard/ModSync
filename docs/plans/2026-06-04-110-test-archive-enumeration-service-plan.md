---
title: "test: ArchiveEnumerationService file tree building"
status: shipped
pr: pending
---

# test: ArchiveEnumerationService file tree building

## Problem

`ArchiveEnumerationService` builds archive/file trees for component browsing but had no dedicated tests.

## Solution

Add `ArchiveEnumerationServiceTests` for null/empty handling and mixed file plus archive tree construction.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter ArchiveEnumerationService`
