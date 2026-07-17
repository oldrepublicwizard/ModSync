---
title: "Three-project parity architecture — expansion-first blueprint"
type: architecture
status: active
date: 2026-07-16
origin: User ask for complete-system parity with important capabilities of all three reference projects; grounded in STRATEGY.md, vortex-mo2 living plan, product-vision
depends_on: docs/plans/vortex-mo2-feature-parity-living-plan.md; STRATEGY.md (vision branch); docs/knowledgebase/product-vision.md (vision branch)
branch: feat/three-project-parity-foundation
---

# Plan 2026-07-16-001 — Three-project parity architecture

## Summary

ModSync’s “complete system” is not a greenfield rewrite of a generic mod manager.
It is the intersection of **three reference systems** already named in repo strategy
and the Vortex/MO2 living plan: modern Nexus-manager UX (**Vortex**), power-user
virtualization/profiles/conflicts (**MO2**), and KOTOR order-sensitive patch
execution (**HoloPatcher** / TSLPatcher-compatible). This plan identifies those
three, lists **important** capabilities only, proposes expansion-first Core seams
(ports/adapters), maps what already exists vs gaps, and defines executable units
`U1..U10`.

**Design bias:** Prefer future expansion over short-term simplicity — explicit
ports in `ModSync.Core`, thin GUI/CLI adapters, no second install pipeline.

## 1. The three projects (explicit)

| # | Project | Role in ModSync docs | Why it is in scope |
|---|---------|----------------------|--------------------|
| 1 | **Vortex** | Named competitor in `NxmHandlerProbe`; primary Nexus-ecosystem parity target in `docs/plans/vortex-mo2-feature-parity-living-plan.md` | Soft deploy (hardlink), `nxm://`, Nexus updates, FOMOD, one-click “Mod Manager Download” |
| 2 | **Mod Organizer 2 (MO2)** | Named competitor in `NxmHandlerProbe`; co-equal living-plan target; purge/conflicts language is MO2-style in KB | Profiles/loadouts, purge newest-first, file conflict attribution, VFS-like non-destructive installs |
| 3 | **HoloPatcher** | STRATEGY.md approach (“built on HoloPatcher’s CLI”); product-vision foundation; `docs/ModSync_Master.md` contrasts Vortex expectations with TSLPatcher/KOTOR reality | Order-sensitive Patch instructions, namespaces, Mac/Linux without Wine, instruction-file fidelity Vortex/MO2 do not provide for KOTOR |

**Not a third competitor:** There is no documented third *external mod manager*
(Steam Workshop appears once in Master as user expectation only). Treating
HoloPatcher as the third **reference system** is the reading that unifies
STRATEGY (KOTOR patch chain) with the living plan (Vortex/MO2 manager features).

**Not in scope as “the three”:** `ModSync.Core` / `ModSync.GUI` / `ModSync.Tests`
(solution layout in `AGENTS.md`) — those are implementation assemblies, not
parity references.

### Sources

- Living tracker: `docs/plans/vortex-mo2-feature-parity-living-plan.md` (phases 1–7)
- Strategy: `STRATEGY.md` (guide ingest/emit, entry points, multi-author; HoloPatcher foundation)
- Vision: `docs/knowledgebase/product-vision.md` (vision vs current state)
- Competitor probe: `docs/knowledgebase/nxm-protocol-handler.md` (MO2 / Vortex / ModSync)

### Related PRs (refreshed 2026-07-17 / U7)

| PR | State | Relevance |
|----|-------|-----------|
| [#168](https://github.com/oldrepublicwizard/ModSync/pull/168) | CLOSED | Superseded by #176 managed wiring |
| [#169](https://github.com/oldrepublicwizard/ModSync/pull/169) | MERGED | FOMOD post-download GUI + CLI |
| [#170](https://github.com/oldrepublicwizard/ModSync/pull/170) | MERGED | Fail-closed FOMOD validate/install gate (U7 code) |
| [#171](https://github.com/oldrepublicwizard/ModSync/pull/171) | MERGED | Paste guide + draft instructions (U9) |
| [#172](https://github.com/oldrepublicwizard/ModSync/pull/172) | MERGED | `modsync://` Phase 1 parse/CLI/handoff |
| [#176](https://github.com/oldrepublicwizard/ModSync/pull/176) | MERGED | Parity ports foundation + managed install wiring + `modsync://` Phase 2 |
| [#177](https://github.com/oldrepublicwizard/ModSync/pull/177)–[#181](https://github.com/oldrepublicwizard/ModSync/pull/181) | MERGED | U3 CLI, KB audit, U5 uninstall, U4 decision B, U6 patcher provenance |

## 2. Capability matrix (important only)

Important = living-plan phases + STRATEGY differentiators. Deliberately omits
endorsements UI, plugin images, save-game isolation, Steam Workshop, etc.

| Capability | Vortex | MO2 | HoloPatcher | ModSync today | Gap severity |
|------------|:------:|:---:|:-----------:|---------------|--------------|
| `nxm://` Mod Manager Download | ● | ● | — | Shipped (#155–#164) | Low (macOS `.app` release bundling residual) |
| Nexus update checks / badges | ● | ○ | — | Shipped core + badges (#156/#167) | Low (persist cache; endorse deferred) |
| Install profiles / loadouts | ○ | ● | — | Shipped (#157); no save isolation | Medium (wizard hooks) |
| Non-destructive deploy (hardlink/copy) | ● | ● | — | Engine (#158) + install wiring (#176) + CLI (#177) | Low (managed VFS DryRun option A deferred) |
| Per-mod uninstall / purge | ● | ● | — | GUI + Deployed badge (#179) | Low |
| File conflict analysis | ○ | ● | — | Analyzer + dialog shipped | Medium (badges / ValidatePage) |
| FOMOD configure | ● | ● | — | Parser + dialog + post-download (#169); **fail-closed gate Merged (#170)** | Low |
| Instruction / patch execute (TSLPatcher) | — | — | ● | Shipped via Patch + Resources | Low (U6 live-hash provenance #181; CAS restore deferred) |
| Guide ingest → instructions | — | — | — | Paste/draft on master (#171); guide ports (#176) | Low |
| Guide emit | — | — | — | `GenerateModDocumentation` shipped | Low |
| Share-a-build deep link | ○ | ○ | — | Phase 1 (#172) + Phase 2 consume/OS reg (#176); Settings toggle deferred | Low |
| Multi-author publish/share | — | — | — | Merge/profiles only | Medium (stub plan 003) |
| Dry-run / VFS validate | ○ | ● | ○ | Classic VFS shipped; managed decision B (#180) | Medium (option A deferred) |
| Agent/CLI parity for above | — | — | — | Strong validate/install/download + `--managed`/`--profile` (#177) | Medium (U10 polish) |

Legend: ● = core to that project’s important UX; ○ = partial/adjacent; — = N/A.

## 3. Clean architecture (expansion-first)

### Principles

1. **Core owns domain ports; GUI/CLI are adapters.** New capabilities land as
   interfaces + services under `src/ModSync.Core`, with Avalonia and `Program` CLI
   as hosts (already the pattern for `IDownloadHandler`, `IFileSystemProvider`,
   `IFomodPostDownloadHost`).
2. **One install pipeline.** Classic vs managed is an install-backend strategy,
   not a fork of `InstallationService` / validation.
3. **VFS remains the truth for prediction.** Managed mode must eventually stage
   into a virtual tree the same way classic writes to `<<kotorDirectory>>`
   (`VirtualFileSystemProvider` only for dry-run/analysis).
4. **Path sandboxing unchanged.** TOML still requires `<<modDirectory>>` /
   `<<kotorDirectory>>`; absolute paths stay internal to services.
5. **Expansion seams before features.** Introduce ports even when only one
   adapter exists today, so Vortex-like / MO2-like / patcher-like backends can
   grow without rewriting callers.

### Proposed Core seams (ports)

Place under `src/ModSync.Core/` (suggested folders; names illustrative):

| Port | Responsibility | Existing concrete seed |
|------|----------------|------------------------|
| `IDownloadProvider` / handler chain | Resolve URL → archive (HTTP, Nexus, nxm, Mega, …) | `IDownloadHandler`, `DownloadHandlerFactory` |
| `IProtocolHandler` | Parse + register OS schemes (`nxm`, `modsync`) | `NxmUrl`; GUI `NxmProtocolRegistrationService`; upcoming `ModSyncUrl` |
| `IInstallBackend` | Classic direct-to-game vs managed stage+deploy | `InstallationService` + `DeploymentService` (+ #176 `ManagedInstallSession`) |
| `IDeploymentStore` | Manifests, uninstall, purge | `DeploymentService` |
| `IProfileStore` | Named loadouts | `ProfileService` |
| `IConflictAnalyzer` | Multi-writer attribution | `FileConflictAnalyzer` |
| `IFomodConfigurator` | Detect / present / apply FOMOD choices | `FomodPostDownloadOrchestrator` + `IFomodPostDownloadHost` |
| `IPatcherEngine` | HoloPatcher / KPatcher invocation | `PatcherEngines` / TSLPatcher instruction path |
| `IGuideIngestor` / `IGuideEmitter` | Prose ↔ instruction file | `DraftInstructionService`, `MarkdownParser`, `GenerateModDocumentation` |
| `IUpdateChecker` | Provider version checks | `ModUpdateCheckService` + `NexusApiClient` |

```
                    ┌──────────── GUI (Avalonia) ────────────┐
                    │  Wizard / Settings / Dialogs / Menus   │
                    └───────────────┬────────────────────────┘
                                    │ adapters
┌───────────────────────────────────▼───────────────────────────────────┐
│                         ModSync.Core (domain)                          │
│  Ports: Download | Protocol | InstallBackend | Deployment | Profiles  │
│         Conflicts | Fomod | Patcher | Guide | Updates | FileSystem    │
│  Shared: Instruction model, VFS, ValidationPipeline, MainConfig        │
└───────┬─────────────────┬──────────────────┬──────────────────────────┘
        │                 │                  │
   CLI adapter      Real FS / VFS      External: Nexus API,
   (Program)        providers          HoloPatcher binary, OS MIME
```

### Non-goals for this architecture pass

- Replacing Avalonia or introducing a second UI stack.
- Genericizing beyond KOTOR/TSL until HoloPatcher + instruction fidelity are
  stable behind `IPatcherEngine`.
- Full MO2 virtualized filesystem (USVFS); ModSync’s hardlink deploy + VFS dry-run
  is the chosen expansion path.

## 4. What already exists vs gaps

### Exists in-tree (do not re-implement)

| Area | Evidence |
|------|----------|
| nxm end-to-end | `NxmUrl`, handler, IPC, Settings, conflict probe, progress UI |
| Nexus updates | `NexusApiClient`, `ModUpdateCheckService`, badges, menu action |
| Profiles | `ProfileService`, `ProfileManagerDialog` |
| Deployment engine | `Services/Deployment/*`, tests |
| Conflicts | `FileConflictAnalyzer`, `ConflictsDialog` |
| FOMOD stack | Parser, dialog, post-download orchestrator (#169 merged) |
| Classic install + validation | `InstallationService`, `InstallationValidationPipeline`, VFS dry-run |
| Patcher | HoloPatcher Resources + Patch instruction |
| Download multi-provider | `IDownloadHandler` family |
| Guide round-trip | Paste (#171), `--parse-directions`, `GenerateModDocumentation`, guide ports (#176) |
| Managed install + CLI | `ManagedInstallSession` (#176); `--managed`/`--profile` (#177) |
| FOMOD fail-closed gate | `FomodConfigurationGate` (#170); see `fomod-support.md` |
| `modsync://` Phase 1–2 | Parse/CLI/handoff (#172); consume + OS registration (#176); Settings toggle deferred |
| Patcher live-file provenance | Manifest hash snapshot (#181); CAS restore deferred |

### Top gaps (ordered) — residual only

1. **U6 CAS restore** — pre-image restore when patchers overwrite existing game files in place.
2. **U8 residual** — Settings UI toggle for `modsync://` OS registration + conflict probe polish.
3. **NexusApiClient migration for download handler** — living-plan “Next” / U10.
4. **Publish/share multi-author** — STRATEGY track; stub only.
5. **Conflict badges / ValidatePage integration** — analyzer exists; UX incomplete.
6. **Managed VFS DryRun option A** — decision B (#180) documents classic DryRun caveat; option A deferred.

## 5. Phased implementation units

Units are independently shippable; prefer landing open PRs over rewrites.

### U1. Architecture seams skeleton (this branch)

**Goal:** Add Core port interfaces + no-op/wrapper adapters around existing
services without behavior change; document mapping in KB index.

**Deliverables:** Port interfaces listed in §3; `DeploymentService` /
`ProfileService` / download factory implement or wrap ports; living-plan link from
this file; refresh Phase 4/6 status notes.

**Verify:** Build + existing `DeploymentService` / `ProfileService` /
`Nxm` / `Fomod` filters still green.

---

### U2. Land managed install wiring (#168)

**Goal:** Opt-in managed mode stages Extract/Move/Copy/Rename, deploys via
`DeploymentService`; classic remains default.

**Depends on:** U1 optional; engine #158 done.

**Verify:** `ManagedInstallSession` / related filters; desktop smoke optional.

---

### U3. CLI + single-mod managed parity

**Goal:** `--profile`, Core-readable `managedDeploymentEnabled`, shared session
for wizard and single-mod install.

**Status:** **Done** (#177).

**Depends on:** U2.

---

### U4. Managed validation decision

**Goal:** Either (A) VFS staging redirect under managed settings, or (B) document
install-only managed validation in `validation-pipeline.md` /
`managed-deployment.md`.

**Status:** **Done** — decision B (#180); option A deferred.

**Depends on:** U2.

---

### U5. Uninstall / purge GUI

**Goal:** Per-component uninstall, purge-all, deployment-state indicator on mod
list; CLI verbs if agent-parity requires.

**Status:** **Done** (#179).

**Depends on:** U2 (manifests written).

---

### U6. Patcher provenance (ImmutableCheckpoint)

**Goal:** Record patcher outputs into deployment manifests so managed uninstall
does not orphan HoloPatcher writes.

**Status:** **Done** — live hash → manifests (#181); CAS restore deferred.

**Depends on:** U2; exercises `IPatcherEngine` seam.

---

### U7. FOMOD gate + living-plan sync

**Goal:** Merge #170 (or equivalent); update
`vortex-mo2-feature-parity-living-plan.md` Phase 4/6; mark #169 merged.

**Status:** **Done** — gate Merged (#170); living-plan sync (this docs update).

**Depends on:** none (can parallel U2).

---

### U8. Protocol ports — `modsync://` Phase 2

**Goal:** OS registration + MainWindow consume behind `IProtocolHandler`; keep
nxm adapter unchanged.

**Status:** **Done** per KB (#176); Settings toggle deferred (residual).

**Depends on:** #172 Phase 1.

---

### U9. Guide ports on master

**Goal:** Ensure ingest/emit sit behind `IGuideIngestor` / `IGuideEmitter`; land
#171 residuals if still open; agent-action-parity rows current.

**Status:** **Done** — ports (#176); paste/draft (#171).

**Depends on:** vision-branch content on master.

---

### U10. Expansion polish (deferrable)

**Goal:** `NexusModsDownloadHandler` → `NexusApiClient`; conflict badges;
ValidatePage conflict cards; update-check persistence; publish/share stub → real
plan when `modsync://` consume works.

**Depends on:** U2–U8 as needed per item.

## 6. Suggested execution order

```
U1 (seams) ──► U2 (#168) ──► U3 ──► U4
                 │              └────► U5, U6
U7 (#170 / docs) ─────────────────────► (parallel)
U8 (modsync Phase 2) ─────────────────► after #172
U9 (guides on master) ────────────────► after #171
U10 (polish) ─────────────────────────► last
```

## 7. Verification (architecture + early units)

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~DeploymentService|FullyQualifiedName~ProfileService|FullyQualifiedName~Nxm|FullyQualifiedName~Fomod|FullyQualifiedName~FileConflictAnalyzer"
```

After U2+: add `ManagedInstallSession` / validation parity filters per plan
`2026-07-13-004` (on managed-wiring branch).

## 8. Doc updates required when implementing

| When | Update |
|------|--------|
| U1 lands | This plan status; KB README link; optional `docs/knowledgebase/architecture-ports.md` |
| U2 lands | `managed-deployment.md` `[OPEN]` → wiring `[REPO]`; living plan Phase 4 |
| U7 lands | Living plan Phase 6; `fomod-support.md` if needed |
| U8 lands | `modsync-protocol-handler.md`; product-vision entry-point row |
| Any unit | Prefer updating living plan over creating parallel trackers |

## 9. Success criteria

- Three reference systems named and agreed (Vortex, MO2, HoloPatcher).
- Important capability matrix drives backlog; living plan remains status tracker.
- Core ports exist so new providers/backends do not require GUI rewrites.
- Phase 4 wiring + uninstall close the largest Vortex/MO2 gaps.
- HoloPatcher remains first-class via `IPatcherEngine` + provenance, not an
  afterthought bolted onto managed deploy.
- STRATEGY vision tracks (guide, share link, multi-author) stay compatible with
  the same ports rather than parallel stacks.

## 10. Landed on `feat/three-project-parity-foundation` (2026-07-16)

### U1 seams + highest-ROI slices behind those ports (no classic-install behavior change)

| Seam | Location | Notes |
|------|----------|-------|
| `IDownloadProviderRegistry` | `Ports/Download/` | Implemented by `DownloadManager`; `DownloadHandlerFactory.CreateProviderRegistry()` |
| `IInstallBackend` + selector | `Ports/Installation/` | Classic default; managed wraps `DeploymentService` |
| `IProtocolHandler` + registry | `Ports/Protocol/` | `NxmProtocolHandler` + `ModSyncProtocolHandler`; `ModSyncUrl` in Core |
| `IProfileStore` | `Ports/Profiles/` | Implemented by `ProfileService`; GUI dialog typed to port; artifact dir helper |
| `IGuideIngestService` / `IGuideEmitService` | `Ports/Guides/` | Serialization + `DraftInstructionService` |
| `IConflictAnalyzer` | `Ports/Conflicts/` | Implemented by `FileConflictAnalyzer` |
| `IUpdateCheckResultStore` | `Ports/Updates/` | JSON snapshot; wired from Nexus update menu action |

Tests: `ParityPortsTests`, `ModSyncUrlTests`.

### U2 managed install wiring (shipped on this branch)

| Piece | Location | Notes |
|-------|----------|-------|
| Settings flag | `ModSyncSettings` / `AppSettings.managedDeploymentEnabled` | Classic default (`false`) |
| Active profile | `activeProfileName` via Profile Manager Activate | Required when managed on (fail closed) |
| Session | `ManagedInstallSession` | Staging redirect + deploy via `IInstallBackend` selector |
| Install path | `InstallationService` (+ wizard / single-mod) | `RunWithManagedInstallSessionAsync`; classic unchanged when flag off |
| Settings UI | Deployment checkbox | Disabled without active profile |
| Finish messaging | Wizard complete pages + single-mod dialog | R7 patcher warning summary |

Tests: `ManagedInstallSessionTests`, `ModSyncSettingsTests`, `DeploymentService` filters.

**FOMOD gate:** Fail-closed `FomodConfigurationGate` is **Merged (#170)** on both classic and managed install paths. U7 docs sync updates the [living plan](vortex-mo2-feature-parity-living-plan.md); KB `fomod-support.md` already documents gate behavior.

### Unit status (2026-07-17 / U7)

| Unit | Status |
|------|--------|
| U3 CLI `--managed` / `--no-managed` / `--profile` | **Done** ([#177](https://github.com/oldrepublicwizard/ModSync/pull/177)) |
| U4 Managed VFS / validation parity | **Done** (decision B — [#180](https://github.com/oldrepublicwizard/ModSync/pull/180); option A deferred) |
| U5 Uninstall / purge GUI | **Done** ([#179](https://github.com/oldrepublicwizard/ModSync/pull/179)) |
| U6 Patcher provenance (live hash → manifests) | **Done** ([#181](https://github.com/oldrepublicwizard/ModSync/pull/181); CAS restore for in-place overwrites deferred) |
| U7 FOMOD gate + living-plan sync | **Done** (gate [#170](https://github.com/oldrepublicwizard/ModSync/pull/170); living-plan / this docs sync) |
| U8 `modsync://` Phase 2 OS consume | **Done** ([#176](https://github.com/oldrepublicwizard/ModSync/pull/176); Settings toggle deferred — residual) |
| U9 Guide ports on master | **Done** (ports [#176](https://github.com/oldrepublicwizard/ModSync/pull/176); paste/draft [#171](https://github.com/oldrepublicwizard/ModSync/pull/171)) |
| U10 Expansion polish | Deferred (residual) |
