---
title: "feat: complete legacy compat test coverage (telemetry config + XML root)"
type: feat
status: completed
date: 2026-06-04
origin: docs/knowledgebase/rebrand-legacy-strings.md
branch: feat/legacy-compat-test-coverage
---

# feat: complete legacy compat test coverage

## Summary

Plan 070 tested settings.json and telemetry.key legacy paths. Complete coverage for the remaining `rebrand-legacy-strings.md` inventory: `KOTORModSync/telemetry_config.json` and XML `KOTORModSync` root key deserialization.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | `TelemetryConfiguration.Load` reads legacy `telemetry_config.json` | NUnit test |
| R2 | `DeserializeModComponentFromXmlString` accepts `KOTORModSync` root wrapper | NUnit test |
| R3 | KB lists all four legacy compat tests | `rebrand-legacy-strings.md` |

## Implementation units

### U1. Telemetry legacy config path test

**Files:** `src/ModSync.Tests/TelemetryConfigurationTests.cs`

### U2. XML legacy root key test

**Files:** `src/ModSync.Tests/ModComponentSerializationLegacyRootTests.cs`

### U3. Update KB verification section

**Files:** `docs/knowledgebase/rebrand-legacy-strings.md`
