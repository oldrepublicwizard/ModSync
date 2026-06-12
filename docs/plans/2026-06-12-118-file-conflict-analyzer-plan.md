# Plan 118: File-level conflict analyzer over the dry-run VFS

First sub-slice of Phase 5 of the Vortex/MO2 feature-parity roadmap (MO2-style
"which mod wins for this file" detection). Core-only: analyzer + result models + tests.

## Goal

Given the selected components in install order, determine every game-directory
destination path that more than one component writes, attribute each write to the
component (and instruction) that performed it, and classify the winner as the last
writer in install order.

## Chosen approach: dry-run execution with VFS write notifications

The analyzer executes each selected component's instructions through the existing
unified dry-run pipeline (`ModComponent.ExecuteSingleInstructionAsync` against a shared
`VirtualFileSystemProvider`), exactly like `DryRunValidator.ValidateInstallationAsync`
does. This was preferred over the static destination-enumeration fallback because:

- It reuses the simulation that already models Extract/Move/Copy/Rename/Delete
  semantics (including archive content enumeration and wildcard expansion), so the
  set of written paths matches what a real install would produce.
- A snapshot/diff of `GetTrackedFiles()` between components cannot detect overwrites:
  a second component writing an already-tracked path does not change the file set,
  and overwrites are precisely the conflicts we need. Write notifications observe
  every write, including overwrites.

To support attribution, one purely additive member is added to
`VirtualFileSystemProvider`: a `FileWritten` event (`Action<string>`) raised with the
normalized absolute destination path whenever a dry-run write records a file
(Copy/Move/Rename destinations, `WriteFileAsync`, and each virtual archive-extract
entry). No existing signatures or behavior change; the event is raised outside the
provider's internal lock.

## Scope

### ModSync.Core (new folder `src/ModSync.Core/Services/Conflicts/`)

- `FileConflict.cs` — one conflicting destination path:
  - `DestinationPath` — relative to the game dir with the `<<kotorDirectory>>` prefix
    preserved (forward slashes, first-seen casing).
  - `Writers` — ordered list (install order) of `FileConflictWriter` entries:
    `ComponentGuid`, `ComponentName`, `InstructionIndex` (zero-based index into
    `ModComponent.Instructions` of the last instruction of that component that wrote
    the path).
  - `WinnerComponentGuid` — the last writer in install order.
- `ConflictAnalysisResult.cs` — `Conflicts`, `ConflictCountsByComponent`
  (per-component number of conflicting paths the component participates in), and
  `AnalyzedComponentCount` (selected components actually simulated).
- `FileConflictAnalyzer.cs` — entry point
  `AnalyzeAsync(IReadOnlyList<ModComponent> componentsInInstallOrder, CancellationToken)`.
  Mirrors `DryRunValidator`: builds a fresh `VirtualFileSystemProvider`, initializes it
  from `MainConfig.SourcePath` / `MainConfig.DestinationPath` for the selected
  components, then executes each selected component's instructions in order with
  per-instruction write attribution. Only writes that land under the game directory
  are considered (mod-workspace staging writes such as archive extraction into
  `<<modDirectory>>` are not user-facing conflicts). Path keys compare
  case-insensitively (`OrdinalIgnoreCase`), matching the VFS and KOTOR's
  case-insensitive pathing expectations.

### VirtualFileSystemProvider (additive only)

- `public event Action<string> FileWritten` raised on every recorded dry-run write.

### Tests (`src/ModSync.Tests/FileConflictAnalyzerTests.cs`)

- Two components copying to the same destination produce exactly one conflict with
  both writers in install order and the second component as winner.
- Components writing distinct destinations produce no conflicts.
- Destination paths differing only by case collide (case-insensitive detection).
- Three writers preserve ordering and the last one wins.
- Deselected components are excluded from the analysis.
- A cancelled token throws `OperationCanceledException`.

All tests use temp directories plus the same `MainConfig`/instruction setup pattern as
`VirtualFileSystemDryRunValidationTests`. No network, no LongRunning tests.

## Out of scope (follow-up slices of Phase 5)

- Per-pair conflict rule overrides ("X before Y") persisted in profiles — profiles are
  being built in parallel; the rules model will land once profile storage exists.
- `ConflictsDialog` GUI (file tree, winner highlighting, reordering).
- Conflict badges on `ModListItem`.
- `ValidatePage` integration of conflict warnings.
- Any `DependencyResolverService` changes (rule-driven ordering feed-in).
- Any edit to `src/ModSync.GUI`.

## Verification

- `dotnet build ModSync.sln --configuration Debug`
- `dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~FileConflictAnalyzer"`
