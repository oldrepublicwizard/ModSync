# Product vision

`[SYNTH]` Where ModSync came from, where it is headed, and how far the repo has gotten. For what the product *is* today, read [product-overview.md](product-overview.md) first; this page covers intent and gaps.

## Origin

`[SYNTH]` ModSync began as a response to the classic KOTOR mod-build experience: a long, order-sensitive manual install chain where one failed step means restarting the whole build from scratch. It was the author's first C# program, built to make that chain executable instead of manual.

`[REPO]` The foundation is HoloPatcher's CLI (bundled per-platform under `Resources/`, see [holopatcher-resources.md](holopatcher-resources.md)): install guides become machine-executable **instruction files** (TOML and related formats) that drive extract/patch/move/delete steps against a game directory.

## Shipped user experience

`[REPO]` The core loop works today: load (or drag-drop / paste) an instruction file or guide → select mods → download → validate → install. Both the install wizard and the legacy Getting Started tab drive this flow (see [install-lifecycle.md](install-lifecycle.md)), and the Core CLI mirrors it headlessly ([core-cli-reference.md](core-cli-reference.md)). Avalonia headless GUI smoke (`GuiSmokeHeadlessTests`) covers paste-import and page-0 layout without a desktop session.

## The full vision

`[SYNTH]` The end state is a two-way bridge between human guides and executable installs, open to any author:

1. **Guide ingestion** — paste or import an existing community install guide and get a working instruction file, including draft executable instructions parsed from the prose. No re-entry by the author.
2. **"Install with ModSync"** — one-click entry points from wherever mods live: `nxm://` links from Nexus Mods, and `modsync://` links for sharing whole builds.
3. **Guide emission** — emit a readable install guide back out of any instruction file, so the instruction file is the source of truth and the guide is a build artifact.
4. **Democratized multi-author builds** — any author's build can be encoded, merged with others, compatibility-fixed, and shared; the community is not limited to one curator's canonical build.

## Vision vs. current state

Row numbers map each capability from [The full vision](#the-full-vision) above; capability 1 splits into three parts (1a–1c).

| Vision capability | Current state | Evidence |
|-------------------|---------------|----------|
| 1a. Guide import (markdown → components) | **Shipped.** `MarkdownParser` parses Deadly Stream-style guides into components, prose preserved in `Directions`; verified round-trippable | `[REPO]` `src/ModSync.Core/Parsing/MarkdownParser.cs`, `DocumentationRoundTripTests` |
| 1b. Paste-a-guide ingestion | **Shipped.** Getting Started **Import from Clipboard** + `FileLoadingService.ImportFromTextAsync`; CLI `convert --stdin` | `[REPO]` [guide-ingestion.md](guide-ingestion.md), `GuideIngestionTests`, `GuiSmokeHeadlessTests` |
| 1c. Prose → executable instructions | **Shipped.** `DraftInstructionService` wires `NaturalLanguageInstructionParser`; drafts review-flagged, never auto-trusted | `[REPO]` `DraftInstructionService.cs`, `convert --parse-directions`, [guide-ingestion.md](guide-ingestion.md) |
| 2. "Install with ModSync" entry points | **Shipped (Settings toggle deferred).** `nxm://` + `modsync://` parse/CLI/handoff/consume + OS registration | `[REPO]` [modsync-protocol-handler.md](modsync-protocol-handler.md); `ModSyncHandoffService`, `ModSyncProtocolRegistrationService` |
| 3. Guide emission (components → guide) | **Shipped.** `GenerateModDocumentation` in `ModComponentSerializationService` | `[REPO]` `src/ModSync.Core/Services/ModComponentSerializationService.cs` |
| 4. Multi-author builds | **Partial.** Merge tooling (`merge` CLI) and install profiles exist; publish/share flows do not | `[REPO]` merge/profiles; [plan stub](../plans/2026-07-13-003-feat-multi-author-publish-share-plan.md) |

`[SYNTH]` Net: guide round-trip (import + paste + draft instructions + emit) and `modsync://` entry points are shipped. Active gap is multi-author publish/share (and optional Settings toggle for protocol registration).

## Where the work is tracked

- Strategy and tracks: [`STRATEGY.md`](../../STRATEGY.md) (repo root)
- Guide paste (done): [docs/brainstorms/2026-07-13-guide-paste-ingestion-requirements.md](../brainstorms/2026-07-13-guide-paste-ingestion-requirements.md), [plan 001](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md)
- `modsync://` (shipped; Settings toggle deferred): [modsync-protocol-handler.md](modsync-protocol-handler.md), [plan 006](../plans/2026-07-13-006-feat-modsync-protocol-os-registration-plan.md)
- Multi-author publish/share (open): [plan stub 003](../plans/2026-07-13-003-feat-multi-author-publish-share-plan.md)
- Release readiness quadruple-check: [plan 005](../plans/2026-07-13-005-release-readiness-checklist.md)

## Related

- [product-overview.md](product-overview.md) — what ModSync is today
- [guide-ingestion.md](guide-ingestion.md) — paste/draft CLI and GUI paths
- [nxm-protocol-handler.md](nxm-protocol-handler.md) — shipped Nexus entry-point track
- [modsync-protocol-handler.md](modsync-protocol-handler.md) — build deep links (consume + OS registration shipped)
- [install-profiles.md](install-profiles.md) — multi-build loadouts
- [fomod-support.md](fomod-support.md) — FOMOD discovery, CLI prompts, configured-only gate
- [agent-action-parity.md](agent-action-parity.md) — GUI vs CLI coverage of these flows
