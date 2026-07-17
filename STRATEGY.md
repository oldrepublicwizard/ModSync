---
name: ModSync
last_updated: 2026-07-13
---

# ModSync Strategy

## Target problem

Mod-build installs for KOTOR are long, order-sensitive manual chains — dozens of downloads, patches, and file moves that must happen in exactly the right order. One failed step restarts everything, and the barrier is so high that effectively one curator publishes builds for the whole community.

## Our approach

Encode install guides as machine-executable instruction files built on HoloPatcher's CLI. Ingest existing human-written guides without re-entry, emit readable guides back out, and make any author's build encodable, compatibility-fixed, and shareable — not just the one canonical build.

## Who it's for

**Primary:** Players - They're hiring ModSync to install a curated mod build without hand-copying files or running TSLPatcher manually for every mod.

**Secondary:** Mod-build authors - They're hiring ModSync to encode their install guide once as an instruction file with dependency and compatibility rules.

**Secondary:** Maintainers / agents - They're hiring ModSync to validate full builds, run CI, and ship releases headlessly via the Core CLI.

## Key metrics

- **Guide import fidelity** - Fraction of a real community guide's components imported as components without hand-editing; measured against `./mod-builds` markdown guides.
- **Full-build pass rate** - Validate/install pass rate for full builds (`KOTOR1_Full.toml` / `KOTOR2_Full.toml`); measured via the validation pipeline and full-build test flows.
- **Distinct encoded authors** - Count of distinct authors' builds encoded as instruction files, beyond the single canonical curator; tracked manually until instruction files carry author metadata.

## Tracks

### Guide ingestion

Turning existing human-written install guides (markdown, pasted text) into instruction files with draft executable instructions.

_Why it serves the approach:_ Authors never re-enter what they already wrote; the encoded-build supply grows from guides that already exist.

### Guide emission

Generating readable install documentation back out of instruction files (`GenerateModDocumentation`), keeping guides and instruction files round-trippable.

_Why it serves the approach:_ The instruction file stays the single source of truth while humans still get a guide they can read and publish.

### "Install with ModSync" entry points

One-click handoff from where mods live into the installer: `nxm://` protocol handling is shipped; a `modsync://` scheme for build links is future work.

_Why it serves the approach:_ Removes the manual download-and-place chain that causes most order-sensitive failures.

### Multi-author builds

Merge tooling, install profiles, and (future) publish/share flows so any author's build can be combined, compatibility-fixed, and distributed.

_Why it serves the approach:_ Breaks the single-curator bottleneck — the crux of the target problem.

## Milestones

- **Undated validation target** - Expanded Ending Overhaul + M4-78EP compatibility encoded in a community build. Named validation target only; no work unit scheduled against it yet.
