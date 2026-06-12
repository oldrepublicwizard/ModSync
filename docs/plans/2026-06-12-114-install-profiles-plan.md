# Plan 114: Install profiles (MO2-style loadouts)

Phase 3 of the Vortex/MO2 feature-parity roadmap.

## Goal

Multiple named loadouts. Each profile stores its own KOTOR directory, mod directory,
instruction file path, and per-component selection state (component selected flag plus
the set of selected option GUIDs). Activating a profile copies its values into the
existing static `MainConfig` and flips `IsSelected` on the loaded components/options,
so the rest of the pipeline keeps working unchanged (minimal-blast-radius approach).

## Scope

### ModSync.Core

- `Services/Profiles/Profile.cs` - model: `Name`, `KotorDirectory`, `ModDirectory`,
  `InstructionFilePath`, `ComponentSelections` (`Dictionary<Guid, ProfileComponentSelection>`
  with `IsSelected` + `SelectedOptionGuids`), `CreatedUtc`, `LastUsedUtc`.
- `Services/Profiles/ProfileService.cs` - constructor takes the storage directory
  (GUI passes the ModSync settings dir; profiles live in a `profiles/` subdirectory,
  one JSON file per profile). Operations:
  - `ListProfiles`, `CreateProfile`, `CloneProfile`, `RenameProfile`, `DeleteProfile`,
    `SaveProfile`, `LoadProfile`.
  - `CaptureFromCurrentState(name, components, instructionFilePath)` - reads
    `MainConfig.SourcePath`/`DestinationPath` and component/option selection state.
  - `ApplyProfile(profile, components)` - writes `MainConfig` paths and component/option
    `IsSelected` flags; updates `LastUsedUtc` and persists.
  - Newtonsoft.Json persistence (matches what Core already uses), sanitized filenames,
    atomic writes (temp file + move).

### ModSync.GUI

- `Dialogs/ProfileManagerDialog.axaml(.cs)` - lists profiles, buttons for
  Create (capture current state), Clone, Rename, Delete, Activate. No font/style/color
  attributes in XAML (implicit theme defaults).
- `Services/MenuBuilderService.cs` - "Profiles..." menu item in the common menu items
  so both the global actions flyout and the global context menu can open the dialog.
  MainWindow startup files are not touched.

### Tests (`src/ModSync.Tests`, single project)

- `ProfileServiceTests.cs` (NUnit) - create/list/clone/rename/delete, persistence
  round-trip, capture/apply against real `ModComponent` instances and `MainConfig`
  (statics saved in SetUp and restored in TearDown), filename sanitization, temp
  directory per test.

### Docs

- `docs/knowledgebase/install-profiles.md` plus an index entry in
  `docs/knowledgebase/README.md`.

## Out of scope

- Save-game isolation (per-profile `saves/` swap) - deferred to a later slice.
- Wizard/`ModSelectionPage` per-profile persistence hooks.
- Profile selector in the MainWindow title area (avoids touching GUI startup files
  that other parallel work edits).
- Applying `InstructionFilePath` does not auto-load the instruction file; the path is
  stored so the GUI can offer to load it later.

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~ProfileService"
```

GUI desktop validation of the dialog is deferred (no desktop session in this run);
noted in the PR body.
