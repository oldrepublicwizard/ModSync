# File-level conflict analysis

`[REPO]` Phase 5 slice 1 (plan
[2026-06-12-118](../plans/2026-06-12-118-file-conflict-analyzer-plan.md)): detects game-directory
paths written by more than one selected component during a dry-run install simulation.

## Components

| Piece | Location | Role |
|-------|----------|------|
| `FileConflictAnalyzer` | `src/ModSync.Core/Services/Conflicts/FileConflictAnalyzer.cs` | Executes selected components in install order against a shared `VirtualFileSystemProvider` and attributes every game-directory write to the component/instruction that performed it. |
| `FileConflict` / `ConflictAnalysisResult` | `src/ModSync.Core/Services/Conflicts/` | Models conflicting paths, writers in install order, and per-component conflict counts. |
| `VirtualFileSystemProvider.FileWritten` | `src/ModSync.Core/Services/FileSystem/VirtualFileSystemProvider.cs` | Additive `Action<string>` event raised outside the internal lock on copy/move/rename/write/extract writes so overwrites are observable (snapshot diff alone cannot detect them). |

## Semantics

- Only writes under `MainConfig.DestinationPath` (game directory) count; mod-workspace staging is ignored.
- Path keys compare case-insensitively; display paths use `<<kotorDirectory>>/` prefix with forward slashes.
- The last writer in install order is the winner, matching real install behavior.
- Deselected components are skipped; dependency-gated components that would not install are skipped.

## GUI (Plan 120)

`[REPO]` `ConflictsDialog` shows analyzer results: destination path, writers in install order, and `(wins)` on the last writer. Entry points: mod list context menu **Analyze File Conflicts** (`MenuBuilderService`) and Mod Management → Validation Operations.

`[UI]` Desktop validation recommended for progress dialog + conflict list layout.

## Still deferred

- Per-pair conflict rules in profiles
- Mod list conflict badges
- ValidatePage integration
- `DependencyResolverService` ordering feed-in

## Tests

`FileConflictAnalyzerTests`, `ConflictsDialogPresenterTests`

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~FileConflictAnalyzer"
dotnet test src/ModSync.Tests/ModSync.Tests.csproj -f net9.0 --filter "FullyQualifiedName~ConflictsDialogPresenter"
```
