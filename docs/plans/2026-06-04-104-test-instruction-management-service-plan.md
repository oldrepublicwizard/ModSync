---
title: "test: InstructionManagementService editor mutations"
status: shipped
pr: pending
---

# test: InstructionManagementService editor mutations

## Problem

`InstructionManagementService` wraps instruction/option create, delete, and move operations for the component editor but had no dedicated tests.

## Solution

Add `InstructionManagementServiceTests` for CRUD/move helpers and null-argument guards.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter InstructionManagementService`
