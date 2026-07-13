# Product vision

`[SYNTH]` Where ModSync came from, where it is headed, and how far the repo has gotten. For what the product *is* today, read [product-overview.md](product-overview.md) first; this page covers intent and gaps.

## Origin

`[SYNTH]` ModSync began as a response to the classic KOTOR mod-build experience: a long, order-sensitive manual install chain where one failed step means restarting the whole build from scratch. It was the author's first C# program, built to make that chain executable instead of manual.

`[REPO]` The foundation is HoloPatcher's CLI (bundled per-platform under `Resources/`, see [holopatcher-resources.md](holopatcher-resources.md)): install guides become machine-executable **instruction files** (TOML and related formats) that drive extract/patch/move/delete steps against a game directory.

## Shipped user experience

`[REPO]` The core loop works today: load (or drag-drop) an instruction file → select mods → download → validate → install. Both the install wizard and the legacy Getting Started tab drive this flow (see [install-lifecycle.md](install-lifecycle.md)), and the Core CLI mirrors it headlessly ([core-cli-reference.md](core-cli-reference.md)).

## The full vision

`[SYNTH]` The end state is a two-way bridge between human guides and executable installs, open to any author:

1. **Guide ingestion** — paste or import an existing community install guide and get a working instruction file, including draft executable instructions parsed from the prose. No re-entry by the author.
2. **"Install with ModSync"** — one-click entry points from wherever mods live: `nxm://` links from Nexus Mods, and eventually `modsync://` links for sharing whole builds.
3. **Guide emission** — emit a readable install guide back out of any instruction file, so the instruction file is the source of truth and the guide is a build artifact.
4. **Democratized multi-author builds** — any author's build can be encoded, merged with others, compatibility-fixed, and shared; the community is not limited to one curator's canonical build.

## Vision vs. current state

Row numbers map each capability from [The full vision](#the-full-vision) above; capability 1 splits into three parts (1a–1c).

| Vision capability | Current state | Evidence |
|-------------------|---------------|----------|
| 1a. Guide import (markdown → components) | **Shipped.** `MarkdownParser` parses Deadly Stream-style guides into components, prose preserved in `Directions`; verified round-trippable | `[REPO]` `src/ModSync.Core/Parsing/MarkdownParser.cs`, `DocumentationRoundTripTests` |
| 1b. Paste-a-guide ingestion | **Missing.** Import is file/drag-drop only; no clipboard/paste entry point into the format-sniffing cascade (`DeserializeModComponentFromString`) | `[REPO]` `FileLoadingService.cs` (Core + GUI) |
| 1c. Prose → executable instructions | **Dead code.** `NaturalLanguageInstructionParser` exists but has zero references; imported guide prose yields no draft `Instruction` objects | `[REPO]` `src/ModSync.Core/Parsing/NaturalLanguageInstructionParser.cs` |
| 2. "Install with ModSync" entry points | **Partial.** `nxm://` protocol handler shipped ([nxm-protocol-handler.md](nxm-protocol-handler.md)); `modsync://` scheme not present anywhere in the codebase | `[REPO]` `src/ModSync.GUI/Services/NxmProtocolRegistrationService.cs`; codebase search for `modsync://` |
| 3. Guide emission (components → guide) | **Shipped.** `GenerateModDocumentation` in `ModComponentSerializationService` | `[REPO]` `src/ModSync.Core/Services/ModComponentSerializationService.cs` |
| 4. Multi-author builds | **Partial.** Merge tooling (`merge` CLI verb) and install profiles ([install-profiles.md](install-profiles.md)) exist; publish/share flows do not | `[REPO]` merge/profiles; `[SYNTH]` publish/share gap |

`[SYNTH]` Net: the round-trip machinery (import + emit) is closer to done than the repo's WIP framing suggests. The active gaps are the ingestion entry point (paste), wiring the natural-language parser into import, and the sharing/distribution layer.

## Where the work is tracked

- Strategy and tracks: [`STRATEGY.md`](../../STRATEGY.md) (repo root)
- First implementation slice (paste ingestion + NL parser wiring): [docs/plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md)

## Related

- [product-overview.md](product-overview.md) — what ModSync is today
- [nxm-protocol-handler.md](nxm-protocol-handler.md) — shipped entry-point track
- [install-profiles.md](install-profiles.md) — multi-build loadouts
- [agent-action-parity.md](agent-action-parity.md) — GUI vs CLI coverage of these flows
