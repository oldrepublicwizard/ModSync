---
title: "feat: legacy AppData path migration tests (settings + telemetry key)"
type: feat
status: completed
date: 2026-06-04
origin: docs/knowledgebase/rebrand-legacy-strings.md
branch: feat/legacy-settings-path-migration-tests
---

# feat: legacy AppData path migration tests

## Summary

Plan 067 documented intentional `%AppData%/KOTORModSync` read paths for settings and telemetry. Add headless tests proving `SettingsManager` and `TelemetryConfiguration` load from legacy paths when `ModSync` files are absent, and prefer `ModSync` when both exist.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `SettingsManager.LoadSettings` reads legacy `KOTORModSync/settings.json` when `ModSync/settings.json` missing | NUnit test |
| R2 | `SettingsManager` prefers `ModSync/settings.json` when both exist | NUnit test |
| R3 | `TelemetryConfiguration` loads signing key from legacy `KOTORModSync/telemetry.key` when `ModSync` key missing | NUnit test |
| R4 | Tests clean up AppData files they create | TearDown deletes test artifacts |

## Implementation units

### U1. SettingsManager legacy path tests

**Files:** `src/ModSync.Tests/SettingsManagerLegacyPathTests.cs`

**Test scenarios:**
- Legacy-only: write `KOTORModSync/settings.json` with distinct `sourcePath`, assert loaded
- Precedence: both paths exist with different values, assert ModSync wins

### U2. TelemetryConfiguration legacy key path test

**Files:** `src/ModSync.Tests/TelemetryConfigurationTests.cs`

**Test scenario:** Legacy-only `KOTORModSync/telemetry.key` when `ModSync/telemetry.key` absent; env vars cleared

### U3. KB cross-link

**Files:** `docs/knowledgebase/rebrand-legacy-strings.md`

**Approach:** Note test file names under verification section
