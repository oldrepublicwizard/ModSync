---
title: "refactor: rebrand KOTORModSync to ModSync (multi-game posture)"
type: refactor
status: completed
date: 2026-06-03
branch: feat/rebrand-kotormodsync-to-modsync
---

# refactor: rebrand KOTORModSync to ModSync (multi-game posture)

## Summary

Rename product, solution, projects, namespaces, paths, CI/scripts, release metadata, and user-facing copy from `KOTORModSync` to **ModSync**. KotOR remains the default supported game in copy and fixtures; branding and instruction placeholders become game-agnostic where mod authors and users see them.

## Requirements

| ID | Requirement | Verification |
|----|-------------|--------------|
| R1 | Solution and project paths use `ModSync` | `ModSync.sln`, `src/ModSync.{Core,GUI,Tests}/` |
| R2 | C# root namespaces: `ModSync.Core`, `ModSync` (GUI), `ModSync.Tests` | `dotnet build ModSync.sln` |
| R3 | No remaining `KOTORModSync` in tracked source, workflows, agent docs, release config | `rg KOTORModSync` clean (historical `docs/plans/*` optional footnotes only) |
| R4 | User-facing strings say ModSync; KotOR called out as default game, not product name | README, landing/wizard copy spot-check |
| R5 | `<<gameDirectory>>` accepted everywhere `<<kotorDirectory>>` was; `<<kotorDirectory>>` remains valid alias | Path tests + `UtilityHelper` |
| R6 | Telemetry/config dirs use `ModSync` (not `KOTORModSync`) | `TelemetryConfiguration.cs`, docs |
| R7 | HoloPatcher / HolocronToolset names unchanged | No renames in `src/HoloPatcher*` |
| R8 | CI and agent scripts build/test via new paths | `dotnet test src/ModSync.Tests/ModSync.Tests.csproj` |

## Decisions

| Topic | Decision | Rationale |
|-------|----------|-----------|
| Placeholder | Canonical `<<gameDirectory>>`; keep `<<kotorDirectory>>` as read/write alias | Existing mod TOMLs in the wild |
| Restore paths | `RestoreCustomVariables` emits `<<gameDirectory>>` when destination is set | New saves trend generic; input still accepts legacy token |
| Repo URLs in props | `https://github.com/th3w1zard1/ModSync` | Workspace/remote already ModSync |
| Historical plans | Do not mass-edit archived `docs/plans/*` unless they block R3 | Reduce noise; optional follow-up |
| Doc filenames | Rename `docs/KOTORModSync_*` → `docs/ModSync_*`; root `KOTORModSync - Official Documentation.md` → `ModSync - Official Documentation.md` | Discoverability |
| `.idea` | Rename or remove JetBrains folder referencing old solution name | IDE hygiene |

## Implementation units

### U1. Git moves (paths before content)

- `KOTORModSync.sln` → `ModSync.sln`
- `src/KOTORModSync.props` → `src/ModSync.props`
- `src/KOTORModSync.Core/` → `src/ModSync.Core/`; csproj → `ModSync.Core.csproj`
- `src/KOTORModSync.GUI/` → `src/ModSync.GUI/`; `KOTORModSync.csproj` → `ModSync.csproj`
- `src/KOTORModSync.Tests/` → `src/ModSync.Tests/`; csproj → `ModSync.Tests.csproj`
- Rename `docs/KOTORModSync_*.md`, `KOTORModSync - Official Documentation.md`, `docs/KOTORModSync_Codebase_Map.json` if present

### U2. Bulk identifier replace (ordered)

1. `KOTORModSyncVersion` → `ModSyncVersion` (and assembly/file version props)
2. `KOTORModSync.Core` → `ModSync.Core`
3. `KOTORModSync.GUI` → `ModSync.GUI`
4. `KOTORModSync.Tests` → `ModSync.Tests`
5. `KOTORModSync` → `ModSync` (remaining)
6. Copyright header `KOTORModSync` → `ModSync`

**Exclude:** `src/HoloPatcher/**` except project references to Core; external URL strings for HolocronToolset.

### U3. Game directory placeholder

**Files:** `src/ModSync.Core/Utility/Utility.cs`, `Instruction.cs`, `Serializer.cs`, validators, parsers, tests referencing `<<kotorDirectory>>`

- `ReplaceCustomVariables`: resolve both tokens to `MainConfig.DestinationPath`
- `RestoreCustomVariables`: write `<<gameDirectory>>` for destination
- Validation/error messages: mention `<<modDirectory>>` or `<<gameDirectory>>`
- Tests: add coverage for `<<gameDirectory>>` alias; keep existing `<<kotorDirectory>>` cases

### U4. Telemetry and app data

**Files:** `TelemetryConfiguration.cs`, `TelemetryService.cs`, `docs/TELEMETRY_SETUP_GUIDE.md`, `docs/GITHUB_SECRET_SETUP.md`, `telemetry-auth/**`

- Application folder name `ModSync` under `%AppData%` / `~/.config`

### U5. CI, release, packaging

**Files:** `.github/workflows/*.yml`, `.github/mod-build-validation.yml`, `release-please-config.json`, `appcast.xml`, `Info.plist`, `scripts/agents/*.sh`, `scripts/agents/common.sh`, `.editorconfig`, `.cursorrules`, `AGENTS.md`

### U6. Documentation and KB

**Files:** `README.md`, `docs/knowledgebase/**`, agent skills referencing old paths, `docs/TELEMETRY_SETUP_GUIDE.md`

- Product description: multi-game installer; KotOR default today

### U7. Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName!~LongRunning" --configuration Debug --no-build
rg -n 'KOTORModSync' --glob '!docs/plans/*'  # expect zero
```

## Test scenarios

| Scenario | Unit / area |
|----------|-------------|
| Build solution after renames | U7 |
| `ReplaceCustomVariables` with `<<gameDirectory>>` resolves destination | U3 / path tests |
| Legacy `<<kotorDirectory>>` still resolves | U3 |
| Restore emits `<<gameDirectory>>` | U3 |
| Headless test project path in `scripts/agents/run_headless_tests.sh` | U5 |

## Risks

| Risk | Mitigation |
|------|------------|
| Missed string in axaml `x:Class` | Build GUI project; grep `KOTORModSync` |
| Broken ProjectReference paths | U1 before U2; full solution build |
| User config path change | Document one-time migration in README (optional read legacy folder) |

## Gap-fill pass (2026-06-04)

- README branding, log prefix `modsync_`, updater temp dirs, telemetry metric names
- `MODSYNC_SIGNING_SECRET` + legacy env/config migration from `%AppData%/KOTORModSync`
- JSON/XML root key `KOTORModSync` read compatibility in serialization
- macOS bundle ID `com.th3w1zard1.modsync`
- `<<gameDirectory>>` test coverage

**Intentionally unchanged:** HoloPatcher `KOTORModSync.*` namespaces; `telemetry.kotormodsync.com` DNS; GitHub secret name `KOTORMODSYNC_SIGNING_SECRET`; DeadlyStream permalink slug.

## Out of scope

- Renaming `HoloPatcher`, `HolocronToolset`, or game-specific fixture paths under `Fixtures/kotor/`
- Multi-game selection UI (future feature)
- Rewriting all historical plan files
