---
title: "docs: ModSync_Master.md client-facing rebrand alignment"
type: docs
status: completed
date: 2026-06-04
origin: docs/plans/2026-06-04-068-docs-telemetry-setup-rebrand-alignment-plan.md
branch: docs/modsync-master-rebrand-alignment
---

# docs: ModSync_Master.md client-facing rebrand alignment

## Summary

`docs/ModSync_Master.md` is a 70k-line stale aggregate (banner at L3) with ~181 legacy `kotormodsync`/`KOTORMODSYNC` strings, mostly duplicated telemetry/setup excerpts. Apply a scoped replace pass for **client-facing** branding and paths aligned with plans 067–068, without renaming Docker `kotormodsync-auth` infrastructure or GitHub Actions secret keys.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | User FAQ copy says ModSync not kotormodsync | FAQ archive answer updated |
| R2 | Client config paths use `~/.config/ModSync` | No `~/.config/kotormodsync` remains |
| R3 | Example OTLP service name is `ModSync` | `.AddService("ModSync")` |
| R4 | Broken `KOTORMODSYNC_TELEMETRY_SETUP` links fixed | Points to `TELEMETRY_SETUP_GUIDE.md` |
| R5 | Infrastructure names unchanged | `kotormodsync-auth`, `KOTORMODSYNC_SIGNING_SECRET` in CI/nginx remain |
| R6 | Stale banner notes 2026 rebrand | Links `rebrand-legacy-strings.md` |

## Scope boundaries

### In scope

- `docs/ModSync_Master.md` targeted replace pass

### Out of scope

- Regenerating the 70k-line aggregate from source
- Renaming telemetry-auth Docker services or GitHub secret names

## Implementation units

### U1. Stale banner and TOC anchor fixes

**Files:** `docs/ModSync_Master.md`

**Approach:** Extend STALE banner; fix `#kotormodsyncs-solution` → `#modsyncs-solution`.

### U2. Client-facing bulk alignment

**Approach:** Replace FAQ headings, user copy, `~/.config/kotormodsync`, dev `MODSYNC_SIGNING_SECRET` examples, `AddService("ModSync")`, `LoadSigningSecret` single-env line, broken doc links.

**Verification:**

```bash
rg '~/.config/kotormodsync' docs/ModSync_Master.md  # expect 0
rg 'kotormodsync can find' docs/ModSync_Master.md   # expect 0
rg 'KOTORMODSYNC_TELEMETRY' docs/ModSync_Master.md  # expect 0
```
