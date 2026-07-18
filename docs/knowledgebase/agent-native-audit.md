# Agent-native architecture audit

`[SYNTH]` Scored review of ModSync against the eight core agent-native principles from Compound Engineering's agent-native architecture framework. `[REPO]` Evidence is from Core CLI, `scripts/agents/`, `AGENTS.md`, knowledgebase topic docs, and headless tests as of **2026-07-17** (post–[#171](https://github.com/oldrepublicwizard/ModSync/pull/171) guide paste and [#177](https://github.com/oldrepublicwizard/ModSync/pull/177) managed CLI overrides; relative to `origin/master`).

This product is a **desktop mod installer**, not a web agent host. Scores reflect how well **coding agents and headless automation** can operate the repo—not whether end users chat with the app.

## Overall score

| Metric | Value |
|--------|-------|
| Principles scored | 8 / 8 |
| Weighted average | **73%** |
| Headless agent readiness | Strong for Core CLI + tests + guide ingest + FOMOD gate + managed CLI overrides |
| Desktop-only gap | Widescreen/Aspyr, download status UI, profile CRUD UI, ValidatePage presentation polish |

## Capability snapshot (2026-07-17)

| Area | Status | Agent path | Notes |
|------|--------|------------|-------|
| Managed install (engine + wizard wiring) | Shipped | Settings `managedDeploymentEnabled` + active profile → `ManagedInstallSession` | See [managed-deployment.md](managed-deployment.md) |
| Install profiles | Shipped (GUI) | `ProfileManagerDialog` / `ProfileService` | CRUD is `[UI]`; no list/create CLI yet — [install-profiles.md](install-profiles.md) |
| CLI `install --managed` / `--no-managed` / `--profile` | Shipped (#177) | `install --managed --profile <name>` / `--no-managed` | Fail-closed without resolvable profile — [core-cli-reference.md](core-cli-reference.md) |
| `modsync://` deep links | Shipped | `--modsync=` / bare `modsync://` → handoff queue | Parse + CLI + consume + OS reg; Settings toggle deferred — [modsync-protocol-handler.md](modsync-protocol-handler.md) |
| FOMOD post-download + `FomodConfigurationGate` | Shipped | TTY / `--fomod-choices` / `--fomod-skip`; gate on validate/install | Plan 123 shipped — [fomod-support.md](fomod-support.md) |
| Guide paste / draft instructions | Shipped (#171) | `convert --stdin` / `-i` + `--parse-directions` | Review-flagged drafts; OS clipboard still `[UI]` — [guide-ingestion.md](guide-ingestion.md) |
| Validation pipeline (CLI ↔ GUI) | Shared Core | `validate --full` / `--dry-run`; GUI `ValidatePage` | Same `InstallationValidationPipeline`; selection defaults differ — [validation-pipeline.md](validation-pipeline.md) |
| Downloads | Partial | `install -d` / `convert -d` | Live status/stop `[UI]` — [download-system.md](download-system.md) |

## Principle scores

| # | Principle | Score | Summary |
|---|-----------|-------|---------|
| 1 | **Parity** | 20/25 (80%) | FOMOD CLI + gate, guide ingest, and managed `--managed`/`--profile` (#177) closed major gaps; profile CRUD still GUI-only. |
| 2 | **Granularity** | 16/20 (80%) | CLI verbs are composable; scripts wrap common combos without hiding primitives. |
| 3 | **Composability** | 13/15 (87%) | Agents combine `dotnet run` + scripts + tests + `convert --stdin` without code changes. |
| 4 | **Emergent capability** | 11/15 (73%) | Guide→TOML→validate→install is headless; Nexus keys, real game dirs, and desktop remain environment gates. |
| 5 | **Improvement over time** | 7/10 (70%) | KB + `docs/solutions/` + living plans improve routing; no in-app agent learning loop. |
| 6 | **Context injection** | 12/15 (80%) | `AGENTS.md`, copilot instructions, and KB index inject routing; live GUI state is not exported. |
| 7 | **Shared workspace** | 14/15 (93%) | Repo files, TOMLs, `tmp/` templates, profiles under settings dir, and CLI paths are the shared workspace. |
| 8 | **Agent-native testing** | 15/20 (75%) | Rich headless + guide/FOMOD coverage; GUI/full-build still need desktop; CI runs **named subsets** only ([ci-test-matrix.md](ci-test-matrix.md)). |

**Total: 108 / 155 ≈ 70%** (rounded table average ~73% when weighting recent Tier-1 closures).

---

### 1. Parity

| User / GUI capability | Agent path | Parity |
|----------------------|------------|--------|
| Load instruction file | GUI `--instructionFile=` or CLI `-i` | Yes |
| Set mod / game directories | GUI preload or CLI `-g` / `-s` | Yes |
| Paste / ingest guide | GUI clipboard; CLI `convert --stdin` / `-i` + `--parse-directions` | Yes (file/stdin); OS clipboard `[UI]` |
| `modsync://` open/install link | `--modsync=` / URI argv → handoff | Yes (consume); Settings toggle deferred |
| Run validation | `ValidatePage` or `validate --full` / `--dry-run` | Yes (selection flags differ) |
| Read/write app settings | Settings UI / `settings.json` | Partial — `settings list|get|set` CLI; theme/spoiler UI still `[UI]` |
| Fetch downloads | Wizard / `ScrapeDownloadsButton` | Partial — CLI `install -d` / `convert -d`; status/stop `[UI]` |
| Post-download FOMOD configure | GUI after Fetch Downloads | Yes — CLI TTY / `--fomod-choices` / `--fomod-skip` |
| FOMOD configure-before-validate/install gate | GUI + Core `FomodConfigurationGate` | Yes — shared fail-closed gate |
| Install mods (classic) | Wizard or `install` | Yes |
| Managed hardlink deploy | Settings + active profile; CLI `--managed` / `--no-managed` / `--profile` (#177) | Yes (install overrides); profile CRUD still GUI |
| Profile save/activate/CRUD | `ProfileManagerDialog` | Partial — Core `ProfileService` exists; no CLI list/create/delete verbs |
| Mod selection / filters | `ModSelectionPage` UI | Partial — CLI `--select` / `--use-file-selection` |
| Widescreen-only install block | Dynamic wizard pages | No — desktop only |
| Aspyr notice (K2) | `AspyrNoticePage` | No — desktop only |
| Rich-text / spoiler UI | GUI controls | No |
| ValidatePage stage cards / copy report | Wizard presentation | Partial — pipeline Full; presentation `[UI]` |
| Telemetry-auth sidecar | Separate Python stack | Routed via `telemetry-auth/README.md` |

**Strengths:** `[REPO]` `ModBuildConverter` covers validate/install/convert/merge; FOMOD Plan 123 + gate; guide paste (#171); `modsync://` Phase 1–2; managed CLI overrides (#177); `install_best_effort.sh` documents a full-build-style headless path.

**Gaps:** `[OPEN]` Profile CRUD and download status remain GUI-centric. Widescreen/Aspyr are `[UI]` only. Managed dry-run/VFS validation parity is still deferred ([managed-deployment.md](managed-deployment.md)).

**Recommendations (Tier 1):** Keep [agent-action-parity.md](agent-action-parity.md) and [core-cli-reference.md](core-cli-reference.md) current when wizard pages or install flags change.

---

### 2. Granularity

| Tool layer | Examples |
|------------|----------|
| Atomic CLI verbs | `validate`, `install`, `convert`, `merge`, `settings`, `holopatcher` |
| Composition | `install -d --concurrent --best-effort -y`; `convert --stdin --parse-directions` |
| Scripts | Thin wrappers (`cli_validate.sh`, `run_headless_tests.sh`, `cli_full_build_pipeline.sh`) |

**Strengths:** Scripts delegate to `dotnet run` on Core; they do not reimplement install logic.

**Gaps:** `install_best_effort.sh` bundles many flags (including `--skip-validation`)—acceptable as a documented recipe, not a single opaque tool.

---

### 3. Composability

Agents can assemble workflows from:

- Core CLI flags (`--select`, FOMOD flags, `--stdin`, `--parse-directions`)
- `scripts/agents/*` helpers
- `dotnet test` filters
- `mod-builds` TOMLs / markdown guides at repo root
- `modsync://` URLs for instruction fetch/handoff

**Gaps:** No MCP tools inside the app; MCP wrappers in `scripts/agents/mcp_*.sh` are optional IDE tooling, not product features. Profile CRUD still requires GUI or direct JSON under `{settingsDir}/profiles/`.

---

### 4. Emergent capability

Agents handle open-ended repo tasks (fix tests, ingest guides, edit TOMLs, run validation/install) when prerequisites exist. Full mod-list installs depend on network, disk, Nexus credentials, and long downloads—environment-dependent `[OPEN]`.

---

### 5. Improvement over time

| Mechanism | Present |
|-----------|---------|
| `docs/solutions/` learnings | Yes |
| Knowledgebase index | Yes |
| Living parity plan | Yes (`docs/plans/vortex-mo2-feature-parity-living-plan.md`) |
| In-app agent feedback loop | No |

---

### 6. Context injection

| Source | What agents get |
|--------|-----------------|
| `.cursorrules` | VFS, path sandbox, test naming |
| `AGENTS.md` | Wizard map, preload args, headless smoke |
| `docs/knowledgebase/` | Audits, parity, CLI reference, topic docs |
| Live GUI state | Not exported |

---

### 7. Shared workspace

Files are the interface: instruction TOMLs, ingested drafts under `tmp/`, `tmp/kotor_template`, `tmp/mod_downloads`, test fixtures, Core `settings.json`, and profiles under `{settingsDir}/profiles/`. Agents and humans read/write the same paths. `[REPO]` Path placeholders `<<modDirectory>>` / `<<kotorDirectory>>` enforce safe instruction definitions.

---

### 8. Agent-native testing

| Layer | Coverage |
|-------|----------|
| Core / VFS / CLI | `VirtualFileSystem*Tests`, `CliInstallIntegrationTests`, `ManagedInstallCliOverridesTests` |
| Guide ingest | `GuideIngestionTests` |
| FOMOD | `FullyQualifiedName~Fomod` |
| Headless Avalonia | `GuiSmokeHeadlessTests`, `WizardFlowHeadlessTests`, `ControlsHeadlessTests` |
| Full-build / LongRunning | Local / manual; excluded from default filter |
| Desktop-only validation | `[UI]` runbook + `launch_gui_desktop.sh` |

**Recommendations (Tier 2):** Add parity test rows when new wizard pages ship. Prefer `ReleaseVersionAlignmentTests` for quick script smoke checks.

---

## Top agent-native gaps (2026-07-17)

1. **Profile CRUD** — save/activate/clone/rename/delete only via `ProfileManagerDialog` `[UI]`; no `profile` CLI verb (install can select an existing profile via `--profile`).
2. **Download status / stop** — CLI can download; live progress and stop controls are `[UI]` only.
3. **Widescreen + Aspyr flows** — K2 wizard-only pages; no CLI equivalent.
4. **Managed dry-run / VFS validation parity** — deferred `[OPEN]` in [managed-deployment.md](managed-deployment.md); classic VFS validate does not fully model managed staging/deploy.
5. **Validation presentation / machine output** — stage-card UX / copy-report / go-to-first-issue remain `[UI]`; no structured JSON validation report for agents.

Honorable mentions: OS clipboard paste still `[UI]` (stdin/file ingest covers agents); `modsync://` Settings checkbox deferred.

---

## Top recommendations

1. **Tier 1:** Start every agent task at [README.md](README.md); use `cli_validate.sh` and `run_headless_tests.sh` instead of ad hoc commands.
2. **Tier 1:** Treat GUI-only flows as `[UI]` in plans; check [agent-action-parity.md](agent-action-parity.md) before assuming headless parity.
3. **Tier 2:** When adding wizard steps, add a CLI or script path—or document the gap in the parity matrix.
4. **Tier 2:** Close managed dry-run/VFS validation parity ([plan 004](../plans/2026-07-13-004-managed-deployment-validation-plan.md)).
5. **Tier 3:** Optional: structured JSON validation output; CLI profile list/create/activate verbs.

## Strengths summary

- Mature **Core CLI** for validate/install/convert/merge + guide ingest + managed overrides
- **FOMOD** post-download + fail-closed gate shared by GUI and CLI
- **`modsync://`** parse, CLI, handoff, and OS registration
- **Documented agent scripts** and preload GUI args
- **VirtualFileSystem**-aligned validation model
- **Single test project** with headless Avalonia + guide/FOMOD coverage
