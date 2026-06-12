---
title: "refactor: shared Getting Started step progress helper"
status: shipped
pr: pending
---

# refactor: shared Getting Started step progress helper

## Problem

`StepNavigationService` and `UIStateService` duplicated Getting Started step 1–4 completion logic and progress messaging.

## Solution

Add `StepProgressUiHelper` for preparation-step computation, incomplete-step mapping, step 5 validation gate, progress counts, and progress-bar messages. Wire `StepNavigationService` and `UIStateService` to delegate.

Includes `StepProgressUiHelperTests` and retains `StepNavigationServiceTests` from plan `093`.

## Verification

`dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "StepProgressUiHelper|StepNavigationService"`
