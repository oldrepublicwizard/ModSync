# Agent helper scripts

`[REPO]` Bash wrappers for coding agents and CI-like local runs. All paths are repo-relative; invoke from repository root unless noted.

## Prerequisites

- .NET 9 SDK on `PATH`
- For GUI scripts: X11 `DISPLAY`, `mod-builds` clone, template dirs (see below)

## Wizard validation tests

`[REPO]` After changing `ValidatePage` or validation services (see [gui-validation-surfaces.md](../../docs/knowledgebase/gui-validation-surfaces.md)):

```bash
./scripts/agents/test_pr110_validation.sh
./scripts/agents/verify_open_pr_ready.sh   # tests + gh pr checks when a PR is open
```

## Catalog

| Script | Purpose |
|--------|---------|
| `create_template_kotor_install.sh` | Create minimal KOTOR tree + empty mod workspace |
| `ensure_linux_holopatcher.sh` | Link Linux HoloPatcher into GUI `Resources/` |
| `launch_gui_desktop.sh` | Build and launch Avalonia with preload args `[UI]` |
| `install_best_effort.sh` | Headless full-list install: `-d`, `--best-effort`, **`--skip-validation`** (see [cli-selection-semantics.md](../../docs/knowledgebase/cli-selection-semantics.md)) |
| `cli_validate.sh` | Wrapper around Core `validate` verb; `--full`, `--use-file-selection`, `--dry-run`, `--dry-run-only`; links HoloPatcher via `common.sh` |
| `cli_full_build_pipeline.sh` | Merge mod-builds `full.md` + `KOTOR*_Full.toml`, export formats, `--auto-generate-local`, `--dry-run` / `--dry-run-only`, optional `--install` |
| `common.sh` | `ensure_core_resources_symlink` helper (sourced by other scripts) |
| `run_headless_tests.sh` | `dotnet test` excluding `LongRunning` |
| `test_pr110_validation.sh` | Wizard presenter + dialog mapper tests |
| `test_current_open_pr.sh` | Alias for `test_pr110_validation.sh` |
| `verify_open_pr_ready.sh` | Wizard tests + `gh pr checks` for current branch PR |
| `mcp_filesystem.sh` | MCP filesystem server scoped to repo |
| `mcp_playwright.sh` | MCP Playwright server |
| `mcp_desktop_commander.sh` | MCP Desktop Commander |

## Common flows

### Guide ingest (draft instructions)

```bash
dotnet run --project src/ModSync.Core/ModSync.Core.csproj -f net9.0 -- \
  convert -i ./mod-builds/content/k1/full.md --parse-directions -f toml -o ./tmp/draft.toml
```

See [guide-ingestion.md](../../docs/knowledgebase/guide-ingestion.md).

### Headless smoke test

```bash
./scripts/agents/run_headless_tests.sh
```

Optional filter:

```bash
./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~ReleaseVersionAlignment"
```

### Avalonia GUI UX smoke (no desktop / no X11)

Uses `Avalonia.Headless.XUnit` via `HeadlessTestApp` — do **not** launch the real GUI for paste-flow, wizard page-order, or page-0 layout smoke:

```bash
# Expanded filter: GuiSmokeHeadlessTests + all *Headless* suites
./scripts/agents/run_headless_tests.sh \
  --filter "FullyQualifiedName~Headless|FullyQualifiedName~GuiSmoke"

# Narrow GuiSmoke only (paste import, wizard order, compact layout, ValidatePage splitter)
./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~GuiSmokeHeadlessTests"
```

`GuiSmokeHeadlessTests` covers: `ImportFromClipboardButton`, `LoadInstructionTextAsync` sample markdown (no OS clipboard), Welcome→ValidatePage key controls, Welcome/Landing/early-page ScrollViewer layout, ValidatePage log `GridSplitter`.

### Validate instruction file

```bash
./scripts/agents/create_template_kotor_install.sh ./tmp/kotor_template ./tmp/mod_downloads

# Linux: full validation needs HoloPatcher in Core output Resources (see ensure_linux_holopatcher.sh)
./scripts/agents/ensure_linux_holopatcher.sh
ln -sfn "$PWD/src/ModSync.GUI/bin/Debug/net9.0/Resources" \
  "$PWD/src/ModSync.Core/bin/Debug/net9.0/Resources"

./scripts/agents/cli_validate.sh \
  --input ./mod-builds/TOMLs/KOTOR1_Full.toml \
  --game-dir ./tmp/kotor_template \
  --source-dir ./tmp/mod_downloads \
  --full
```

Validate only mods marked `IsSelected=true` in the TOML (matches GUI Mod Selection):

```bash
./scripts/agents/cli_validate.sh \
  --input ./mod-builds/TOMLs/KOTOR1_Full.toml \
  --game-dir ./tmp/kotor_template \
  --source-dir ./tmp/mod_downloads \
  --full \
  --use-file-selection
```

`--full` without HoloPatcher may exit non-zero after loading components; that still confirms the wrapper invokes Core correctly.

### Mod-builds merge + dry-run pipeline

Requires `./mod-builds` clone. Merges canonical TOML instructions with markdown metadata, then optionally dry-runs:

```bash
./scripts/agents/create_template_kotor_install.sh ./tmp/kotor_template ./tmp/mod_downloads
./scripts/agents/cli_full_build_pipeline.sh \
  --game k1 \
  --game-dir ./tmp/kotor_template \
  --source-dir ./tmp/mod_downloads \
  --dry-run-only
```

With an empty mod workspace, `--dry-run` fails archive checks; use `--dry-run-only` for VFS-only validation (as in the example above).

Export all four formats and chain install (after downloads are present):

```bash
./scripts/agents/cli_full_build_pipeline.sh \
  --game k1 \
  --game-dir ./tmp/kotor_template \
  --source-dir ./tmp/mod_downloads \
  --export-all-formats \
  --auto-generate-local \
  --dry-run-only \
  --install
```

`--install` uses the same best-effort flags as `install_best_effort.sh` (`--skip-validation`, `--best-effort`). Run `--dry-run-only` first on empty workspaces; use full `--dry-run` once archives exist.

### Desktop GUI (full-build style)

```bash
git clone https://github.com/th3w1zard1/mod-builds ./mod-builds  # if missing
./scripts/agents/create_template_kotor_install.sh ./tmp/kotor_template ./tmp/mod_downloads
./scripts/agents/ensure_linux_holopatcher.sh
./scripts/agents/launch_gui_desktop.sh \
  --instruction-file ./mod-builds/TOMLs/KOTOR1_Full.toml \
  --kotor-dir ./tmp/kotor_template \
  --mod-dir ./tmp/mod_downloads
```

Then follow `docs/local_desktop_agent_runbook.md` for wizard clicks.

### Best-effort install (long-running)

```bash
./scripts/agents/install_best_effort.sh \
  ./mod-builds/TOMLs/KOTOR1_Full.toml \
  ./tmp/kotor_template \
  ./tmp/mod_downloads
```

Requires Nexus credentials for many mods; may run for hours.

## Documentation

- [docs/knowledgebase/README.md](../../docs/knowledgebase/README.md)
- [docs/knowledgebase/core-cli-reference.md](../../docs/knowledgebase/core-cli-reference.md)
- [docs/knowledgebase/agent-action-parity.md](../../docs/knowledgebase/agent-action-parity.md)
