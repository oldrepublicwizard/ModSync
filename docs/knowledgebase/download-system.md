# Download system

`[REPO]` How mod archives are resolved and downloaded: per-component URL registry, handler chain, cache service, and GUI vs CLI entry points.

Sources: `src/ModSync.Core/Services/Download/DownloadHandlerFactory.cs`, `src/ModSync.Core/Services/DownloadCacheService.cs`, `src/ModSync.Core/Parsing/MarkdownParser.cs` (ResourceRegistry population), `src/ModSync.GUI/MainWindow.axaml.cs`.

## Resource registry

`[REPO]` Each **`ModComponent`** may carry a **`ResourceRegistry`**: `Dictionary<string, ResourceMetadata>` keyed by download URL (or link string). Entries are populated when parsing mod-build markdown/TOML (e.g. links extracted from the component **Name** field in `MarkdownParser`).

Downloads and validation use this registry to know which archives a component expects under `<<modDirectory>>`.

## Handler priority order

`[REPO]` **`DownloadHandlerFactory.CreateHandlers`** returns handlers in **fixed order**. The first handler that can process a URL wins; **`DirectDownloadHandler` must be last** (generic HTTP/HTTPS fallback).

| Order | Handler | Role |
|-------|---------|------|
| 1 | `DeadlyStreamDownloadHandler` | DeadlyStream-specific URLs |
| 2 | `MegaDownloadHandler` | MEGA links |
| 3 | `NexusModsDownloadHandler` | Nexus Mods (optional API key) |
| 4 | `GameFrontDownloadHandler` | GameFront hosts |
| 5 | `DirectDownloadHandler` | Any remaining HTTP/HTTPS |

**Nexus API key**: constructor parameter or `MainConfig.NexusModsApiKey`; CLI **`--nexus-api-key`** or env `KOTOR_MODSYNC_NEXUS_API_KEY` / `NEXUS_MODS_API_KEY`.

**Timeouts**: factory default HttpClient timeout is **180 minutes** per handler creation; CLI **`--download-timeout-hours`** (install/convert) caps the download phase (default **48** hours on install).

## DownloadCacheService

`[REPO]` **`DownloadCacheService`** is the orchestration layer used by GUI and CLI:

- Tracks cached files and download failures (`GetFailures()` for error reporting).
- **`ResolveOrDownloadAsync`** resolves URLs to files under the mod/source directory (used when downloading or refreshing cache entries).
- Wired to **`DownloadManager`** created via **`DownloadHandlerFactory.CreateDownloadManager`**.

GUI: **`MainWindow`** constructs a shared **`DownloadCacheService`** and exposes it to **`GettingStartedTab`**, **`DownloadLinksControl`**, and download orchestration (`DownloadOrchestrationService`).

## GUI entry points

| Surface | Control / page | Behavior |
|---------|----------------|----------|
| Install wizard | `DownloadsExplainPage` | Explains downloads; background fetch may continue while advancing |
| Legacy Getting Started | `ScrapeDownloadsButton` (`Fetch Downloads`) | Triggers scrape/download flow via main window handlers |
| Editor / links UI | `DownloadLinksControl` | Per-component link resolution using `DownloadCacheService` |
| Status | `DownloadStatusButton`, `StopDownloadsButton` | Live progress and cancel `[UI]` — no first-class CLI equivalent |

`[UI]` Full-build agent workflow: after **Select All**, use **Fetch Downloads** on the downloads step, then run validation — see [local desktop agent runbook](../local_desktop_agent_runbook.md).

## CLI entry points

| Verb | Flag | Requirement |
|------|------|-------------|
| `install` | `-d` / `--download` | Requires `--source-dir` (mod workspace) |
| `install` | `--concurrent` | Parallel downloads (harder to debug) |
| `convert` | `-d` / `--download` | Requires `--source-path` |
| `convert` | `--concurrent` | Same as install |

Helper: **`scripts/agents/cli_validate.sh`** does not download; use **`install -d`** or pipeline scripts that include download when archives are missing.

**Parity gap**: CLI can download archives; it does not replicate the GUI download status panel or stop button. See [agent-action-parity.md](agent-action-parity.md).

## Typical agent workflow

1. Clone **`./mod-builds`** at repo root if testing full builds — [mod-builds-sources.md](mod-builds-sources.md).
2. Create template dirs: **`scripts/agents/create_template_kotor_install.sh`**.
3. Either populate `<<modDirectory>>` manually, or run **`install -d`** / GUI **Fetch Downloads** with Nexus key when needed.
4. Run **`validate`** before **`install`** unless using **`--skip-validation`** / **`install_best_effort.sh`** (documented in [cli-selection-semantics.md](cli-selection-semantics.md)).

## Related docs

- [install-lifecycle.md](install-lifecycle.md) — wizard order and install orchestration
- [instruction-format.md](instruction-format.md) — `<<modDirectory>>` placeholder
- [validation-pipeline.md](validation-pipeline.md) — archive presence in ComponentValidation stage
- [core-cli-reference.md](core-cli-reference.md) — all CLI flags
