# CI test matrix vs local runs

`[REPO]` GitHub Actions does **not** run the same filter as `./scripts/agents/run_headless_tests.sh`.

## Local default (agents)

```bash
./scripts/agents/run_headless_tests.sh
# Equivalent filter: FullyQualifiedName!~LongRunning
```

Runs the **full** test project (minus `LongRunning` suffix if any exist). Longer and broader than CI.

## CI smoke subsets

### `build-and-test.yml` / `push-integration.yml` (Linux)

| Step filter | Classes exercised |
|-------------|-------------------|
| `FullyQualifiedName~InstallCoordinatorTests` | Install coordination |
| `FullyQualifiedName~CliInstallIntegrationTests` | CLI install pipeline |
| `FullyQualifiedName~DownloadQueueHeadlessTests` | Download queue (push-integration only) |
| `FullyQualifiedName~InstallingPageHeadlessTests\|FullyQualifiedName~WizardFlowHeadlessTests` | Wizard headless (push-integration only) |
| `FullyQualifiedName~MarkdownTomlParityTests` | Markdown/TOML parity (push-integration only) |
| `FullyQualifiedName~AutoUpdateServiceTests` | Auto-update service |
| `FullyQualifiedName~AutoUpdateClientHeadlessTests\|FullyQualifiedName~AutoUpdateServiceHeadlessTests\|FullyQualifiedName~HeadlessBootstrapTests` | Headless bootstrap / client |
| `FullyQualifiedName~SelfUpdateIntegrationTests` | Self-update integration |

### `code-cleanup.yml`

- `InstallCoordinatorTests`, `DownloadQueueHeadlessTests`

### `mod-build-validation.yml`

- `FullyQualifiedName~DocumentationRoundTripTests` (after cloning community mod-builds markdown)

## Implications for agents

| Claim | Accurate? |
|-------|-----------|
| “CI passed” = every test in the project ran | **No** — subset only |
| `run_headless_tests.sh` matches CI | **No** — local is wider |
| Green CI + green local script = strong signal | **Yes** — run both before large merges |

## PR-targeted local filters (merge-ready open PRs)

`[REPO]` CI does not run these filters as a dedicated job; run locally before merging the matching PR.

| PR | Branch | Filter |
|----|--------|--------|
| [#110](https://github.com/th3w1zard1/ModSync/pull/110) | `feat/wizard-archive-validation-parity` | `FullyQualifiedName~WizardValidationStagePresenter` |
| [#110](https://github.com/th3w1zard1/ModSync/pull/110) | same | `FullyQualifiedName~ValidationPipelineDialogMapper` |
| [#111](https://github.com/th3w1zard1/ModSync/pull/111) | `feat/holocron-erf-nested-open` | `FullyQualifiedName~KotorFormatBridgeCliTests` |

Holocron tests skip when PyKotor is not importable. Wrappers: `./scripts/agents/test_pr110_validation.sh`, `./scripts/agents/test_pr111_holocron_bridge.sh`. See [agent-action-parity.md](agent-action-parity.md).

## Related

- [removed-features.md](removed-features.md) — no `DistributedCache` CI job
- [scripts/agents/README.md](../../scripts/agents/README.md)
