# Residual review findings — parity U3 CLI managed overrides

Branch: `feat/parity-cli-managed-u3`  
PR: https://github.com/oldrepublicwizard/ModSync/pull/177  
Plan: `docs/plans/2026-07-16-001-three-project-parity-architecture-plan.md` (U3)  
Review: `ce-code-review` run `20260717-013743-46fc067d` (2026-07-17)

## Verdict

**Merge-ready once CI is green.** No P0/P1 correctness bugs found. Fail-closed managed-without-profile, process-local overrides (no `SaveManagedDeploymentFields` on the CLI/install path), single-mod vs install-all session wiring, and net48 (`LangVersion` 7.3) compile all check out.

## Focus checklist (review)

| Focus | Result |
|-------|--------|
| Fail-closed managed without profile | OK — `ManagedInstallCliOverrides.Apply` + `ManagedInstallSession.TryCreate` |
| Process-local overrides (no settings.json persist) | OK — in-memory mutate only; `SaveManagedDeploymentFields` not called from CLI/`InstallationService` |
| Single-mod vs install-all session parity | OK — both forward `profileOverride` / `managedDeploymentOverride` into `RunWithManagedInstallSessionAsync` |
| net48 compatibility | OK — `dotnet build -f net48` 0 errors; override helper uses C# 7.3-safe syntax |
| Unit tests | OK — `ManagedInstallCliOverrides` 6/6 passed |

## Residual (not merge-blocking)

| ID | Severity | Notes |
|----|----------|-------|
| RESID-U3-001 | Medium | Unit tests cover helper `Apply`/`Resolve` only. Missing: settings already `managedDeploymentEnabled` without profile (no `--managed`); InstallationService override wiring; assert overrides never call save |
| RESID-U3-002 | Medium | `docs/knowledgebase/core-cli-reference.md` `install` section does not yet list `--managed` / `--no-managed` / `--profile` (HelpText is present in code) |
| RESID-U3-003 | Low | Theoretical TOCTOU: CLI `Apply` loads settings, then `InstallationService` reloads from disk before session create. Irrelevant for single-process CLI; only matters if something else rewrites `settings.json` mid-run |
| RESID-U3-004 | Low | Fail-closed early return in CLI does not clear `InstallationService.LastManagedInstallResult` from a prior in-process success (one-shot CLI unaffected) |

## Resolves from prior residual

From `feat-three-project-parity-foundation.md` **RESID-002** (CLI `--profile` / `--managed` deferred): **addressed** by this PR for install overrides. Purge CLI verbs remain deferred (U5).

## Recommended follow-ups (non-blocking)

1. Document the three install flags in `docs/knowledgebase/core-cli-reference.md` (CLI override > settings.json; process-local).
2. Add one helper test for “settings already managed, blank profile, no CLI flags → error”.
3. Optional thin test that `InstallSingleComponentAsync` / `InstallAllSelectedComponentsAsync` accept and forward overrides (mock/session seam).
