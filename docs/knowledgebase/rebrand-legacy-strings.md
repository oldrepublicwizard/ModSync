# Rebrand — intentional legacy strings

`[REPO]` After the ModSync rebrand (plan 065, PR #113) and HoloPatcher namespace rebrand (plan 066, PR #114), a few `KOTORModSync` strings remain in source on purpose. They support one-time migration from pre-rebrand installs and reading legacy saved data — not current product branding.

## Source inventory

| File | String / usage | Reason |
|------|----------------|--------|
| `src/ModSync.GUI/Models/AppSettings.cs` | `%AppData%/KOTORModSync/settings.json` | Load settings from legacy path when `ModSync/settings.json` is absent |
| `src/ModSync.Core/Services/TelemetryConfiguration.cs` | `%AppData%/KOTORModSync/telemetry_config.json` | Load telemetry config from legacy path |
| `src/ModSync.Core/Services/TelemetryConfiguration.cs` | `%AppData%/KOTORModSync/telemetry.key` | Load signing key from legacy path |
| `src/ModSync.Core/Services/ModComponentSerializationService.cs` | JSON root key `"KOTORModSync"` | Read compatibility for instruction files saved under the old root key |

## Related intentional names (not `KOTORModSync` literal)

| Item | Notes |
|------|--------|
| `KOTORMODSYNC_SIGNING_SECRET` env var | Fallback after `MODSYNC_SIGNING_SECRET` in `TelemetryConfiguration` |
| `telemetry.kotormodsync.com` | Production telemetry host DNS (unchanged) |
| `docs/plans/*065*`, `*066*` | Historical migration narratives; grep may still hit these |

## Verification

```bash
rg -n 'KOTORModSync' --glob '!docs/plans/*'
```

Expect exactly the four rows in the source inventory table above.

**Automated tests:**

| Legacy surface | Test |
|----------------|------|
| `KOTORModSync/settings.json` | `SettingsManagerLegacyPathTests` |
| `KOTORModSync/telemetry_config.json` | `TelemetryConfigurationTests.Load_UsesLegacyTelemetryConfigPath_WhenModSyncConfigMissing` |
| `KOTORModSync/telemetry.key` | `TelemetryConfigurationTests.Load_UsesLegacyTelemetryKeyPath_WhenModSyncKeyMissing` |
| XML root key `KOTORModSync` | `ModComponentSerializationLegacyRootTests` |

## Telemetry setup docs

| Doc | Client-facing alignment | Plan |
|-----|-------------------------|------|
| `docs/TELEMETRY_SETUP_GUIDE.md` | `MODSYNC_SIGNING_SECRET`, `~/.config/ModSync` | 068 (merged) |
| `docs/ModSync_Client_Integration_Guide.md` | `AddService("ModSync")`, dual env vars | 068 (merged) |
| `docs/GITHUB_SECRET_SETUP.md` | Client dev env examples | 073 (merged, PR #121) |

## Related plans

- `docs/plans/2026-06-03-065-refactor-rebrand-kotormodsync-to-modsync-plan.md`
- `docs/plans/2026-06-04-066-refactor-holopatcher-namespace-rebrand-plan.md`
- `docs/plans/2026-06-04-067-docs-rebrand-closure-plan-footnotes-plan.md`
- `docs/plans/2026-06-04-068-docs-telemetry-setup-rebrand-alignment-plan.md`
- `docs/plans/2026-06-04-070-feat-legacy-settings-path-migration-tests-plan.md`
- `docs/plans/2026-06-04-071-feat-legacy-compat-test-coverage-completion-plan.md`
- `docs/plans/2026-06-04-073-docs-github-secret-setup-rebrand-alignment-plan.md`
