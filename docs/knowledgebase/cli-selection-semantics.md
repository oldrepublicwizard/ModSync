# CLI selection semantics

`[REPO]` How `install` and `validate` treat component selection vs TOML `IsSelected` flags.

Source: `ModBuildConverter.ApplySelectionFilters` and validate/install handlers in `src/ModSync.Core/CLI/ModBuildConverter.cs`.

## `install` without `--select` (default)

When `--select` is omitted and **`--use-file-selection` is not set**, **every component is marked selected** (`IsSelected = true`), regardless of TOML defaults.

This matches “Select All” on `ModSelectionPage` for full-build TOMLs that ship with `IsSelected = false`.

## `install` with `--use-file-selection`

Only components already marked `IsSelected = true` in the file are installed. Use after editing a TOML or when mirroring a partial GUI selection without `--select` filters.

## `install` with `--select`

Repeatable filters: `category:Name`, `tier:Name`, `mod:Name` (case-insensitive exact or substring match on component `Name`). Only matching components stay selected. Filters combine with AND semantics when multiple types are used together. Repeat `--select` for multiple mod names (e.g. `--select mod:A --select mod:B`).

## `install` with `--download`

`[REPO]` On `install`, `--select` (or `--use-file-selection` / TOML `IsSelected`) is applied **before** downloads. Only components with `IsSelected == true` are fetched when `--download` is set. Use this for single-mod network smoke instead of `convert --download`, which downloads all URL-bearing components before selection filters run.

Example (merged neocities K2 + golden TOML):

```bash
./scripts/agents/k2_ingested_merge_download_smoke.sh --download-mod "Silent Sion Restoration"
```

Full round-trip (golden URLs + ingested NLP → download → extract → Override install; network + `mod-builds`):

```bash
./scripts/agents/k2_merged_roundtrip_download_install_smoke.sh
```

Wraps `K2FullGuideFixture_RoundTripSilentSion_DownloadAndInstalls_LongRunning`. NLP Move drafts for single loose files include nested `<<modDirectory>>/*…/filename` sources so post-extract paths resolve. Folder advisories (e.g. Prestige Class “only install the Jedi Master/Sith Lord fixes”) add nested folder wildcards plus slug/fuzzy variants (`jedimaster_sithlord fixes`) via `DraftInstructionService.BuildFolderMoveSources`.

## `validate` without `--select` (default)

Validates **all loaded components**. TOML `IsSelected` is **not** used unless `--use-file-selection` or `--select` is provided.

## `validate` with `--use-file-selection`

Validates only components with `IsSelected = true` in the file (matches the install wizard after Mod Selection).

## Unified validation pipeline

`[REPO]` GUI (`ValidatePage`, legacy validate) and CLI `validate` both call `InstallationValidationPipeline` in Core. Wizard-equivalent CLI:

```bash
./scripts/agents/cli_validate.sh --input path.toml --game-dir ... --source-dir ... --full --dry-run --use-file-selection
```

Without `--full`, CLI skips environment/conflict/order stages (component archive checks only). Without `--dry-run`, archive checks run but VFS dry-run does not.

## `install` pre-check (default)

`[REPO]` When `--skip-validation` is **not** set, `install` runs `InstallationValidationPipeline` with the same preset as the wizard `ValidatePage` (`ValidationPipelineOptions.WizardFull`: environment, conflicts, order, archives, VFS dry-run). The pre-check always validates components with `IsSelected == true` only (same as wizard Mod Selection → Validate), regardless of `--use-file-selection` on the install command itself.

`--skip-validation` skips the full pipeline (environment-only behavior removed). `install_best_effort.sh` always passes `--skip-validation`.

## `validate` with `--select`

Applies the same category/tier filters, then validates only components with `IsSelected == true` after filtering.

## `--best-effort` (install)

`[REPO]` Implies `-y`, `ContinueOnMissingSources`, and `ContinueOnModFailure`.

If no Nexus API key is configured, **Nexus-only mods are deselected** (`DeselectComponentsWithNexusUrlsWithoutApiKey`). Agents may think “everything installable” ran; check logs for deselected Nexus mods.

## Nexus API key flag names

| Verb | Flag |
|------|------|
| `convert`, `merge` | `--nexus-mods-api-key` |
| `install` | `--nexus-api-key` (or env `KOTOR_MODSYNC_NEXUS_API_KEY` / `NEXUS_MODS_API_KEY`) |

## `install_best_effort.sh`

`[REPO]` Passes `--best-effort`, `-d`, `--concurrent`, and **`--skip-validation`** (not implied by `--best-effort` alone). Does not mirror GUI `ValidatePage` unless you run `validate` separately first.

## Related

- [core-cli-reference.md](core-cli-reference.md)
- [agent-action-parity.md](agent-action-parity.md)
