---
title: "refactor: rebrand HoloPatcher KOTORModSync namespaces to HoloPatcher"
type: refactor
status: completed
date: 2026-06-04
branch: feat/holopatcher-namespace-rebrand
origin: docs/plans/2026-06-03-065-refactor-rebrand-kotormodsync-to-modsync-plan.md
---

# refactor: rebrand HoloPatcher KOTORModSync namespaces to HoloPatcher

## Summary

The PyKotor port under `src/HoloPatcher/` and `src/HoloPatcher.UI/` still uses the historical root namespace `KOTORModSync.*`. Rename all C# namespaces and usings to **`HoloPatcher.*`** so the patcher stack is independent of the ModSync product name and KotOR-specific branding.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | No `KOTORModSync` in `src/HoloPatcher/**` or `src/HoloPatcher.UI/**` | `rg KOTORModSync src/HoloPatcher src/HoloPatcher.UI` empty |
| R2 | Root namespace is `HoloPatcher` (e.g. `HoloPatcher.Formats.NCS`, `HoloPatcher.TSLPatcher`) | Spot-check + build |
| R3 | `ModSync.sln` GUI/Core/Tests build | `dotnet build ModSync.sln -c Debug` |
| R4 | Do not change ModSync product namespaces or legacy `%AppData%/KOTORModSync` migration paths in Core/GUI | `rg KOTORModSync src/ModSync.Core` only migration/JSON keys |

## Decisions

| Topic | Decision |
|-------|----------|
| Target root | `HoloPatcher` (matches project/assembly names) |
| Replace order | `KOTORModSync` → `HoloPatcher` (single token, no partial collisions) |
| Comments | Update `KOTORModSync.py` references to `holopatcher` where applicable |
| ModSync.sln | HoloPatcher projects stay referenced via GUI csproj graph only |

## Implementation units

### U1. Bulk namespace replace

**Scope:** `src/HoloPatcher/`, `src/HoloPatcher.UI/` (~549 files)

Ordered replace in all `.cs`, `.axaml`, `.csproj` comments if any.

### U2. Verify build

```bash
dotnet build ModSync.sln --configuration Debug
```

### U3. Update plan 065 footnote

Note HoloPatcher namespace work completed in plan 066 (optional one-line in 065 out-of-scope section).

## Test scenarios

| Scenario | Verification |
|----------|--------------|
| Solution builds | U2 |
| HoloPatcher CLI still launches via GUI resource path | Existing install tests / manual smoke optional |

## Risks

| Risk | Mitigation |
|------|------------|
| Missed qualified name in string literal | R1 ripgrep gate |
| Alias/type confusion with `ModSync` product | Separate root token `HoloPatcher` |

## Out of scope

- Renaming `telemetry-auth` Docker images
- `Fixtures/kotor/` paths
- Historical `docs/plans/*` mass edit
