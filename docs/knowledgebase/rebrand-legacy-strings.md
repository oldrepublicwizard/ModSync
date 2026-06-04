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

**Automated tests:** `SettingsManagerLegacyPathTests`, `TelemetryConfigurationTests.Load_UsesLegacyTelemetryKeyPath_WhenModSyncKeyMissing`

## Related plans

- `docs/plans/2026-06-03-065-refactor-rebrand-kotormodsync-to-modsync-plan.md`
- `docs/plans/2026-06-04-066-refactor-holopatcher-namespace-rebrand-plan.md`
- `docs/plans/2026-06-04-067-docs-rebrand-closure-plan-footnotes-plan.md`
