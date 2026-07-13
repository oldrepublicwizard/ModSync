# Residual review findings — paste / FOMOD security (2026-07-13)

Branch: `fix/paste-draft-path-sandbox` (from `feat/guide-paste-ingestion`)  
Review focus: path sandboxing for NL draft instructions (`DraftInstructionService`) and GUI/CLI review-flag parity after paste / `--parse-directions`

## Finding (medium) — closed

**Issue:** `DraftInstructionService.IsSandboxedPath` / `TrySanitizeInstruction` only checked `StartsWith("<<modDirectory>>")` / `StartsWith("<<kotorDirectory>>")` and did **not** reject `..` segments (or glued forms like `<<modDirectory>>../outside`). Paste + `convert --parse-directions` could therefore draft instructions that escape the placeholder roots after `Path.GetFullPath`.

**Fix:**

1. `IsSandboxedPath` now requires a separator after the placeholder (when a relative path follows) and rejects `..`, drive-letter segments (`:`), and rooted segments — same rejection posture as `FomodToComponentMapper.NormalizeRelativePath`.
2. `GuideIngestionTests` cover `<<modDirectory>>/../outside`, glued traversal, drive-after-placeholder, and prose cases that must not retain escaping paths.
3. GUI/CLI parity: successful drafts call `ApplyReviewFlag`, which sets `InstallationWarning` to `ReviewFlagMessage` (same text CLI writes as `# VALIDATION ISSUES:`). Paste path re-applies the flag and logs that message; `InstallStartPage` surfaces `InstallationWarning` on the pre-install review list.

## Residual Review Findings

| ID | Severity | Status | Notes |
|----|----------|--------|-------|
| PASTE-PATH-001 | Medium | **Closed** | `..` / rooted / drive escapes after placeholder rejected; tests in `GuideIngestionTests` |
| PASTE-REVIEW-001 | Medium (UX/parity) | **Closed** | Draft review flag on `InstallationWarning` + InstallStartPage display; CLI still emits `ReviewFlagMessage` via `ComponentValidationContext` |

**Residual actionable work from this finding: none.**

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName~GuideIngestionTests" \
  --configuration Debug
```
