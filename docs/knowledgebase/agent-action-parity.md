# Agent action parity

`[REPO]` Maps user-visible flows to headless agent capabilities. Label key: **Full** = achievable without desktop; **Partial** = CLI/script with gaps; **UI** = desktop session required; **N/A** = out of scope. Last light refresh: **2026-07-18** (profile CLI verb).

## Install wizard (primary flow)

Wizard order from `src/ModSync.GUI/Dialogs/InstallWizardDialog.axaml.cs` and `AGENTS.md`:

| Step | Page | User action | Agent path | Parity |
|------|------|-------------|------------|--------|
| 1 | `LoadInstructionPage` | Load TOML | `-i` on CLI; `--instructionFile=` on GUI | Full |
| 2 | `WelcomePage` | Continue | N/A (informational) | Full |
| 3 | `PreamblePage` | Read preamble | Optional; content in instruction file | Full |
| 4 | `ModDirectoryPage` | Pick mod dir | `-s` / `--modDirectory=` | Full |
| 5 | `GameDirectoryPage` | Pick game dir | `-g` / `--kotorPath=` | Full |
| 6 | `AspyrNoticePage` | Acknowledge (K2) | No CLI equivalent | UI |
| 7 | `ModSelectionPage` | Select mods, filters | `install` without `--select` = select all; `install --select category:X` / `tier:X` | Full (install); Partial (subset only with `--select`) |
| 8 | `DownloadsExplainPage` | Continue (downloads may run) | `install -d` or `convert -d` (+ FOMOD: TTY / `--fomod-choices` / `--fomod-skip`) | Partial — live download status is `[UI]`; FOMOD configure is Full via CLI (see [fomod-support.md](fomod-support.md)) |
| 9 | `ValidatePage` | Run validation | `validate --full --dry-run --use-file-selection` (same Core `InstallationValidationPipeline` as GUI) | Full |
| 10 | `InstallStartPage` | Confirm install | `install -y` (runs `InstallationValidationPipeline` / `WizardFull` pre-check unless `--skip-validation`) | Full |
| 10b | Managed deploy | Opt-in hardlink deploy via active profile | `install --managed` / `--no-managed` / `--profile` (#177); `profile --action activate`; settings toggle still GUI | Full (CLI overrides + profile CRUD) — [managed-deployment.md](managed-deployment.md) |
| 11 | `InstallingPage` | Watch progress | `install` (console progress) | Full — see [install-lifecycle.md](install-lifecycle.md) |
| 12 | `BaseInstallCompletePage` | Continue | N/A | Full |
| 13+ | Widescreen pages | `WidescreenNoticePage`, `WidescreenModSelectionPage`, `WidescreenInstallingPage`, `WidescreenCompletePage` (dynamic) | No dedicated CLI | UI |
| 14 | `FinishedPage` | Done | N/A | Full |

## Legacy Getting Started tab

| Control | Agent path | Parity |
|---------|------------|--------|
| `Step1ModDirectoryPicker` | `--modDirectory=` / `-s` | Full |
| `Step1KotorDirectoryPicker` | `--kotorPath=` / `-g` | Full |
| `Step2Button` (load file) | `--instructionFile=` / `-i` | Full |
| `ImportFromClipboardButton` (paste guide / TOML) | Core: `convert --stdin` / `-i` + optional `--parse-directions` ([guide-ingestion.md](guide-ingestion.md)); headless button smoke: `GuiSmokeHeadlessTests` | Full (file/stdin ingest); Partial (OS clipboard paste still `[UI]`) |
| `ScrapeDownloadsButton` | `install -d` or `convert -d` (+ FOMOD flags as needed) | Full for download+FOMOD configure; Partial for live status/stop UI |
| `ValidateButton` | `validate --full --dry-run --use-file-selection` (via `InstallationValidationPipeline`) | Full |
| `OpenModDirectoryButton` | `ls` / file tools on mod dir | Full |
| Download status / stop | No first-class CLI | UI |
| Profiles… (`ProfileManagerDialog`) | `profile --action list|show|create|delete|activate|clone|rename` (+ `--settings-dir`, `--json`); `install --profile` selects existing | Full — [install-profiles.md](install-profiles.md) |
| `modsync://` deep link | `--modsync=` or bare URI → handoff / fetch | Full (consume); Settings toggle deferred — [modsync-protocol-handler.md](modsync-protocol-handler.md) |

## Common agent workflows

| Goal | Recommended path |
|------|------------------|
| Smoke-test repo | `./scripts/agents/run_headless_tests.sh` |
| GUI UX smoke (paste import, wizard page order, page-0 layout, validate log splitter) | `./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~Headless\|FullyQualifiedName~GuiSmoke"` (Avalonia.Headless — **no desktop**) |
| Ingest guide → draft TOML | `convert -i guide.md --parse-directions -f toml -o out.toml` or `convert --stdin --parse-directions` — [guide-ingestion.md](guide-ingestion.md) |
| Open `modsync://` instruction URL | Launch GUI with `--modsync=` / URI, or rely on OS handler after registration — [modsync-protocol-handler.md](modsync-protocol-handler.md) |
| Managed install with profile | `profile --action activate --name <name>` then `install … --managed --profile <name>` (or `--no-managed`) — [managed-deployment.md](managed-deployment.md) |
| Create/list install profiles | `profile --action create|list|delete|clone|rename` — [core-cli-reference.md](core-cli-reference.md) |
| Validate TOML structure only | `./scripts/agents/cli_validate.sh --input path.toml` |
| Full validation | `cli_validate.sh` with `--game-dir`, `--source-dir`, `--full` |
| Validate only TOML-selected mods | `cli_validate.sh` … `--use-file-selection` (matches GUI Mod Selection) |
| Template dirs | `./scripts/agents/create_template_kotor_install.sh ./tmp/kotor_template ./tmp/mod_downloads` |
| GUI full-build check | `./scripts/agents/launch_gui_desktop.sh` + runbook wizard clicks `[UI]` |
| Long headless install | `./scripts/agents/install_best_effort.sh` (uses `--best-effort` + `--skip-validation`; see [cli-selection-semantics.md](cli-selection-semantics.md)) |
| Linux HoloPatcher for GUI | `./scripts/agents/ensure_linux_holopatcher.sh` |

## Headless tests as parity proxies

| Test area | Example filter | What it proves |
|-----------|----------------|----------------|
| CLI install | `CliInstallIntegrationTests`, `ValidationPipelineParityTests` | End-to-end install; install pre-check uses same pipeline as wizard |
| VFS validation | `VirtualFileSystemDryRunValidationTests` | Dry-run matches VFS rules |
| Wizard UI | `WizardFlowHeadlessTests` | Page flow without full desktop |
| GUI UX smoke | `GuiSmokeHeadlessTests` | Paste-import button + `LoadInstructionTextAsync` markdown (no clipboard), Welcome→ValidatePage key controls, compact ScrollViewer layout, ValidatePage log splitter |
| Guide ingest | `GuideIngestionTests` | `--stdin` / `--parse-directions` draft + sandboxed paths |
| Profile CLI | `ProfileCliTests` | `profile` verb CRUD + activate persists `activeProfileName` |
| Wizard validation UX | `WizardValidationStagePresenter`, `ValidationPipelineDialogMapper` | Stage cards / dialog mapper parity ([PR #110](https://github.com/th3w1zard1/ModSync/pull/110)) |
| Version alignment | `ReleaseVersionAlignmentTests` | Release metadata consistency |

## ValidatePage presentation (shipped)

`[REPO]` PR [#110](https://github.com/th3w1zard1/ModSync/pull/110) — desktop wizard `[UI]`; stage logic via `WizardValidationStagePresenterTests` and `ValidationPipelineDialogMapperTests`. See [gui-validation-surfaces.md](gui-validation-surfaces.md).

## Gaps to respect in plans

1. **Widescreen block** — wizard-only; document when testing K2 widescreen mods `[UI]`.
2. **Download UX** — CLI can download; no equivalent to live download status UI.
3. **Rich text / spoilers** — GUI rendering; agents edit source TOML/markdown instead.
4. **File pickers** — never automate in cloud agents; always use preload args or CLI paths.
5. **Validate vs install selection** — GUI validates **selected** mods only; CLI defaults differ unless `--use-file-selection`. `install` without flags selects all (full-build). See [cli-selection-semantics.md](cli-selection-semantics.md).
6. **CI test coverage** — green CI runs subsets only; local `run_headless_tests.sh` is broader. See [ci-test-matrix.md](ci-test-matrix.md).
7. **Validation pipeline fail-fast** — `InstallationValidationPipeline` stops after environment failure (no conflict/order/archive/dry-run stages). GUI and CLI both use `ValidationPipelineResult.IsSuccess`; do not infer pass from an empty dry-run result.
8. **Install pre-check opt-out** — `install --skip-validation` and `install_best_effort.sh` skip the wizard-equivalent pipeline; default `install` does not.
9. **FOMOD post-download** — GUI prompts after Fetch Downloads (PR #169). CLI: TTY wizard, `--fomod-skip`, `--fomod-choices` / `MODSYNC_FOMOD_CHOICES`, or non-TTY **warn-continue** (marks `warned`; `FomodConfigurationGate` still blocks validate/install until `configured`). See [fomod-support.md](fomod-support.md).
10. **Guide drafts** — `--parse-directions` / GUI paste drafts are review-flagged; never treat as trusted install instructions without review. See [guide-ingestion.md](guide-ingestion.md).
11. **Managed / profile CLI** — `install --managed` / `--no-managed` / `--profile` shipped (#177). Profile CRUD via `profile` verb (`list|show|create|delete|activate|clone|rename`). See [managed-deployment.md](managed-deployment.md) and [core-cli-reference.md](core-cli-reference.md).
12. **Validation presentation vs pipeline** — CLI and GUI share `InstallationValidationPipeline`; stage-card UX / copy-report / go-to-first-issue remain `[UI]` ([gui-validation-surfaces.md](gui-validation-surfaces.md)).

See [agent-native-audit.md](agent-native-audit.md) for scored principles and [core-cli-reference.md](core-cli-reference.md) for flags.
