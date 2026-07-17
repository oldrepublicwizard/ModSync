# Residual review findings — three-project parity foundation

Branch: `feat/three-project-parity-foundation`  
PR: https://github.com/oldrepublicwizard/ModSync/pull/176  
Plan: `docs/plans/2026-07-16-001-three-project-parity-architecture-plan.md`  
Review mode: `ce-code-review` (autofix applied for P0/P1)

## Verdict

**Not merge-blocked on the P0/P1 items below** after the autofix commit on this branch. Remaining items are deferred product/architecture follow-ups, not silent correctness holes in the landed foundation.

## Fixed in review autofix

| ID | Severity | Finding | Fix |
|---|---|---|---|
| P0-1 | Critical | Managed staging remapped destinations only; Copy→Override then Rename still targeted the live game tree | `ApplyStagingRedirect` remaps `RealSourcePaths` under the game dir; `Instruction.RedirectResolvedSources` |
| P0-2 | Critical | `modsync://` fetch had no user confirm, private-host guard, or size cap (SSRF / unbounded body) | Confirm dialog before download; `InstructionUrlSafety` + 5 MiB cap + final-URI recheck in `ModSyncInstructionFetcher` |
| P0-3 | Critical | Purge could run against a second `DeploymentService` while a managed install is live | `ManagedDeploymentActions.PurgeAsync` / uninstall refuse when `ManagedInstallSession.Current != null` |
| P1-1 | High | Handoff URLs enqueued while `_processing` were stranded | `ProcessPendingAsync` re-drains until empty + race re-entry |
| P1-2 | High | Status masked managed misconfig as classic | `ManagedDeploymentStatus.ResolveError` + `GetStatusOrClassic` surfaces config errors |
| P1-3 | High | Deleting the active profile left `ActiveProfileName` set | Profile delete clears active setting and warns about unmanaged hardlinks |
| P1-4 | High | Manifest `RelativePath` with `..` could escape the game dir on uninstall | Confined path resolve before delete/restore |
| P1-5 | Medium | Uninstall always reported success | Propagate `DeploymentService` false / exceptions through GUI actions |

## Tests added / extended

- `ManagedInstallSessionTests.ApplyStagingRedirect_RemapsSourcesUnderGameDirectory`
- `ModSyncInstructionFetcherTests` — loopback reject, oversized body, `IsBlockedAddress`
- `DeploymentServiceTests.Uninstall_SkipsPathTraversalRelativePaths`

## Residual (not fixed here)

| ID | Severity | Notes |
|---|---|---|
| RESID-001 | High | Managed deploy path still lacks FOMOD fail-closed composition until #170 is merged into this tip |
| RESID-002 | Medium | CLI `--profile` / `--managed` / purge verbs still deferred (plan U3+) |
| RESID-003 | Medium | Settings UI scheme toggle for `modsync://` OS registration still deferred |
| RESID-004 | Low | Managed VFS dry-run parity with classic still deferred |
| RESID-005 | Low | Purge does not report per-component partial failures from `DeploymentService` (only hard failures / lock) |

## Recommended next CE step

1. Land/merge **#170** (`feat/fomod-configuration-gate`) into the managed install path, then re-review FOMOD × managed composition.  
2. Or `/ce-work` remaining plan units (CLI managed/profile/purge) once #176 is green CI.
