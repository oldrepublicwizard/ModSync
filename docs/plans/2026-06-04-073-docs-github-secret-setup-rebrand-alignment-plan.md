---
title: "docs: align GITHUB_SECRET_SETUP client dev examples with ModSync rebrand"
type: docs
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-068-docs-telemetry-setup-rebrand-alignment-plan.md
branch: docs/github-secret-setup-rebrand-alignment
---

# docs: align GITHUB_SECRET_SETUP client dev examples with ModSync rebrand

## Summary

Plan 068 aligned `TELEMETRY_SETUP_GUIDE.md` and `ModSync_Client_Integration_Guide.md` but left `docs/GITHUB_SECRET_SETUP.md` with client-facing dev examples still showing only `KOTORMODSYNC_SIGNING_SECRET`. Update client sections to document `MODSYNC_SIGNING_SECRET` first with legacy fallback, matching `TelemetryConfiguration.LoadSigningSecret`.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | Client secret loading priority documents `MODSYNC_SIGNING_SECRET` then `KOTORMODSYNC_SIGNING_SECRET` | Read §How Authentication Works → Client Side |
| R2 | Development env examples use `MODSYNC_SIGNING_SECRET` with legacy fallback note | §Development Builds → Option 2 |
| R3 | GitHub Actions secret name `KOTORMODSYNC_SIGNING_SECRET` unchanged | CI/workflow sections untouched |
| R4 | Server-side nginx/Docker env examples unchanged (ops infra) | Server sections preserved |
| R5 | KB cross-link from `rebrand-legacy-strings.md` | Doc listed in telemetry docs table |

## Scope boundaries

### In scope

- `docs/GITHUB_SECRET_SETUP.md` client-facing sections only
- `docs/knowledgebase/rebrand-legacy-strings.md` doc inventory row

### Out of scope

- `docs/ModSync_Master.md` aggregate (embedded copy updates separately)
- Renaming GitHub Actions secrets or `kotormodsync-auth` Docker service

## Implementation units

### U1. GITHUB_SECRET_SETUP client alignment

**Files:** `docs/GITHUB_SECRET_SETUP.md`

**Approach:** Update header note, client-side loading priority (lines ~78–80), dev env examples (~366–372), and troubleshooting client env check (~438) to match `TELEMETRY_SETUP_GUIDE.md` pattern. Add one-line legacy fallback note linking `rebrand-legacy-strings.md`.

**Test expectation:** none — documentation only.

**Verification:** Spot-check against `TelemetryConfiguration.cs` lines 164–165.

### U2. KB doc inventory

**Files:** `docs/knowledgebase/rebrand-legacy-strings.md`

**Approach:** Add or update telemetry docs table to note `GITHUB_SECRET_SETUP.md` client examples aligned in plan 073.

**Test expectation:** none — documentation only.
