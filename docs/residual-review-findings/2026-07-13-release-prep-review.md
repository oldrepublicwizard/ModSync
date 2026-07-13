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

## Residual risks
- R1 (P2): Gate trusts `configured` without proving archive-scoped instructions.
- R2 (P2): `warned` still skips Fetch re-prompt (CLI warn-not-repeated).
- R3 (P2): Missing on-disk archives skipped by `GetPaths` (FOMOD gate fail-open until present).

## Testing gaps
- No e2e dismiss→Fetch→configure→gate pass.
- No headless Mod Management Configure coverage.

```json
{"reviewer":"adversarial","findings":[{"severity":"P0","title":"Cascade: dismiss FOMOD then documented recovery never clears gate","confidence":90,"status":"fixed"},{"severity":"P1","title":"Composition: nested Files keys make MarkConfigured silently no-op","confidence":85,"status":"fixed"},{"severity":"P1","title":"Composition: backslash FOMOD sources survive reconfigure merge","confidence":80,"status":"fixed"}],"residual_risks":["R1","R2","R3"],"testing_gaps":["dismiss-fetch-configure e2e","Mod Management UI"]}
```
