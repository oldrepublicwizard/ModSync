# Plan 120 — ConflictsDialog GUI (Phase 5 slice 2)

**Status:** shipped (PR pending)  
**Branch:** `feat/conflicts-dialog` (stacks on `feat/file-conflict-analyzer`)  
**Depends on:** PR #160

## Goals

1. `ConflictsDialog` shows `FileConflictAnalyzer` results (path, writers, winner).
2. Entry: mod list menu **Analyze File Conflicts** + Mod Management validation tab.
3. Progress dialog during dry-run simulation.

## Out of scope

- Profile conflict rules, mod list badges, ValidatePage integration, reordering

## Verification

```bash
dotnet build ModSync.sln -f net9.0
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~FileConflictAnalyzer"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~ConflictsDialogPresenter"
```
