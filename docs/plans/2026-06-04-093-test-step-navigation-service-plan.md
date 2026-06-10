---
title: "test: StepNavigationService GetCurrentIncompleteStep coverage"
status: shipped
pr: pending
---

# test: StepNavigationService GetCurrentIncompleteStep coverage

## Problem

`StepNavigationService.GetCurrentIncompleteStep` drives Getting Started step jumping but had no dedicated unit tests beyond indirect UI state tests.

## Solution

Add `StepNavigationServiceTests` covering step 1–5 progression, constructor null guards, and temp-directory cleanup.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter StepNavigationService`
