# Install profiles

`[REPO]` Phase 3 of the Vortex/MO2 feature-parity roadmap (plan
[2026-06-12-114](../plans/2026-06-12-114-install-profiles-plan.md)). A profile is a
named loadout: KOTOR directory, mod directory, instruction file path, and per-component
selection state (component `IsSelected` plus the set of selected option GUIDs).

## Model and service

- `src/ModSync.Core/Services/Profiles/Profile.cs` - `Profile` and
  `ProfileComponentSelection` (keyed by component GUID).
- `src/ModSync.Core/Services/Profiles/ProfileService.cs` - `ListProfiles`,
  `CreateProfile`, `CloneProfile`, `RenameProfile`, `DeleteProfile`, `SaveProfile`,
  `LoadProfile`, `CaptureFromCurrentState`, `ApplyProfile`.

## Storage

One JSON file per profile (Newtonsoft.Json, indented) under
`{settingsDir}/profiles/`, where the GUI passes the same settings directory that
`SettingsManager` uses for `settings.json` (`%APPDATA%/ModSync` on Windows,
`~/.config/ModSync` on Linux). Filenames are sanitized profile names (path separators
and invalid filename characters become underscores). Writes are atomic: temp file in
the same directory, then move.

## Activation semantics (minimal blast radius)

`ApplyProfile` does not introduce a new configuration source. It copies the profile's
directories into the existing static `MainConfig` via its instance accessors
(`sourcePath`, `destinationPath`) and flips `IsSelected` on the live
`ModComponent`/`Option` instances. Components without an entry in the profile are left
untouched. The stored `InstructionFilePath` is informational; activation does not
auto-load the instruction file.

## GUI

`src/ModSync.GUI/Dialogs/ProfileManagerDialog.axaml(.cs)` lists profiles with
Save Current As / Activate / Clone / Rename / Delete. Entry point: the "Profiles..."
item that `MenuBuilderService.AddCommonMenuItems` adds to the global actions flyout and
the global context menu.

## Out of scope (future slices)

- Save-game isolation (per-profile `saves/` swap).
- Profile selector in the MainWindow title area.
- Per-profile persistence hooks in the install wizard's `ModSelectionPage`.

## Tests

`src/ModSync.Tests/ProfileServiceTests.cs` (NUnit): CRUD, persistence round-trip,
capture/apply against real `ModComponent` instances and `MainConfig` (statics saved and
restored per test), filename sanitization, corrupt-file skipping.

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~ProfileService"
```
