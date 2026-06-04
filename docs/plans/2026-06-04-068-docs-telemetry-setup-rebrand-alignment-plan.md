---
title: "docs: align telemetry setup guides with ModSync client paths"
type: docs
status: completed
date: 2026-06-04
origin: docs/knowledgebase/rebrand-legacy-strings.md
branch: docs/telemetry-setup-rebrand-alignment
---

# docs: align telemetry setup guides with ModSync client paths

## Summary

Post-rebrand telemetry setup docs still tell developers to use `KOTORMODSYNC_SIGNING_SECRET` and `~/.config/kotormodsync/` paths. The client loads `MODSYNC_SIGNING_SECRET` first, writes to `%AppData%/ModSync` / `~/.config/ModSync`, and falls back to legacy `KOTORModSync` folders. Align setup guides and the integration guide example code without renaming Docker `kotormodsync-auth` infrastructure.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | Secret loading priority documents `MODSYNC_SIGNING_SECRET` then `KOTORMODSYNC_SIGNING_SECRET` | Read TELEMETRY_SETUP_GUIDE §Secret Loading |
| R2 | Client config paths use `ModSync` with legacy `KOTORModSync` fallback note | Linux/Mac examples updated |
| R3 | Integration guide example uses `AddService("ModSync")` and current `LoadSigningSecret` shape | Matches `TelemetryService.cs` / `TelemetryConfiguration.cs` |
| R4 | Docker/CI `kotormodsync-auth` and GitHub secret name unchanged | Infrastructure names preserved |

## Scope boundaries

### In scope

- `docs/TELEMETRY_SETUP_GUIDE.md`
- `docs/ModSync_Client_Integration_Guide.md` (client sections only)

### Out of scope

- `docs/ModSync_Master.md` (large aggregate; separate pass)
- Renaming `telemetry-auth` Docker service names or GitHub Actions secret keys

## Implementation units

### U1. TELEMETRY_SETUP_GUIDE client path and env alignment

**Files:** `docs/TELEMETRY_SETUP_GUIDE.md`

**Approach:** Update secret priority, dev env examples, and Linux config file paths. Add one-line legacy fallback note linking `rebrand-legacy-strings.md`.

**Test expectation:** none — documentation only.

### U2. ModSync_Client_Integration_Guide example alignment

**Files:** `docs/ModSync_Client_Integration_Guide.md`

**Approach:** Update `LoadSigningSecret` example (dual env vars, legacy path fallback), `AddService("ModSync")`, Linux config path examples.

**Test expectation:** none — documentation only.

### U3. Verification

**Verification:** Spot-check against `src/ModSync.Core/Services/TelemetryConfiguration.cs` and `TelemetryService.cs`.
