# Residual review findings — feat/fomod-configuration-gate (2026-07-13)

**Scope:** `master..HEAD` on `feat/fomod-configuration-gate` (merge-base `c6fbbb51`). Tip at fix time included FOMOD gate/fail-closed/smoke/UTC serialization. Guide-paste commits were not on tip when finalized.

**Depth:** Deep (validate/install gate + post-download recovery).

## Fixed

### P0 — Dismiss → permanent gate deadlock
`ShouldPrompt` skipped all non-empty statuses (including `dismissed`), so Fetch Downloads could not recover. Mod Management Configure FOMOD claimed save without `MergeInto`/`MarkConfigured`.

**Fix:** Re-prompt `dismissed`; skip only `configured`/`warned`. Mod Management merges into the single selected mod and marks configured.

### P1 — Nested Files key vs basename status
`MarkConfigured` used exact `ContainsKey`, missing `nested/mod.7z` when status used basename.

**Fix:** Basename-aware lookup; status keys use file-name segment.

### P1 — Backslash sources survive reconfigure
Merger prefix match was `/`-only.

**Fix:** Normalize `\\`→`/` before compare.

### P2 — R1 configured without archive-scoped instructions
Gate trusted `configured` even when merger output was never applied.

**Fix:** Soft warning when status is `configured` but no `<<modDirectory>>/<archive-folder>/` instructions exist. Does not fail the gate.

### P2 — R2 warned skip behavior undocumented
**Fix:** Documented in [fomod-support.md](../knowledgebase/fomod-support.md): `warned` skips re-prompt (CLI warn-continue) but still fails the gate until configured.

### P2 — R3 missing on-disk archives skipped by GetPaths
Registered archives missing on disk were invisible to the gate (fail-open).

**Fix:** Enumerate registered archive paths including missing ones. Fail-closed when prior FOMOD prompt status exists; otherwise soft-warn (generic missing downloads stay with component archive validation).

## Testing gaps (addressed)

- Unit: dismiss → `ShouldPrompt` true → `MarkConfigured` → gate passes (`DismissThenConfigure_RePromptsThenGatePasses`).
- Merger nested basename prefix + backslash reconfigure covered in `FomodConfiguredComponentMergerTests`.
- Nested registry Files key + MarkConfigured covered in gate + prompt-state tests.

```json
{"reviewer":"adversarial","findings":[{"severity":"P0","title":"Cascade: dismiss FOMOD then documented recovery never clears gate","confidence":90,"status":"fixed"},{"severity":"P1","title":"Composition: nested Files keys make MarkConfigured silently no-op","confidence":85,"status":"fixed"},{"severity":"P1","title":"Composition: backslash FOMOD sources survive reconfigure merge","confidence":80,"status":"fixed"},{"severity":"P2","title":"configured without archive-scoped instructions only soft-warns","confidence":70,"status":"fixed"},{"severity":"P2","title":"missing on-disk archives with prior FOMOD status fail closed","confidence":75,"status":"fixed"}],"residual_risks":[],"testing_gaps":["Mod Management UI headless"]}
```
