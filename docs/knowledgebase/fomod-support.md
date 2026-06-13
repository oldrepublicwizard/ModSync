# FOMOD installer support

Status: Core parser/mapping (Phase 6 slice 1) plus GUI installer dialog (slice 2).

## What exists `[REPO]`

`src/ModSync.Core/Services/Fomod/` contains:

| File | Purpose |
|------|---------|
| `FomodInfo.cs` | Model for `fomod/info.xml` (Name, Author, Version, Website, Description) |
| `FomodModuleConfig.cs` | Models for `fomod/ModuleConfig.xml`: install steps, groups, plugins, file installs, condition flags, dependency trees, conditional install patterns |
| `FomodParser.cs` | `ParseInfoXml` / `ParseModuleConfigXml` (string/stream/file overloads); case-insensitive element lookup; `FormatException` on invalid XML |
| `FomodDetector.cs` | Finds `fomod/ModuleConfig.xml` in an archive entry listing (case-insensitive, any root prefix) |
| `FomodArchiveDiscovery.cs` | Finds `fomod/ModuleConfig.xml` and `fomod/info.xml` on disk inside an extracted archive folder |
| `FomodToComponentMapper.cs` | Translates a parsed FOMOD into a native `ModComponent` |

GUI (`src/ModSync.GUI/`):

| File | Purpose |
|------|---------|
| `Services/FomodInstallerPresenter.cs` | Headless wizard session: step visibility, group validation, apply `Option.IsSelected` |
| `Dialogs/FomodInstallerDialog.axaml` | Step wizard UI (checkbox/radio semantics per group type) |
| Mod Management → Validation Operations → **Configure FOMOD Mod** | Folder picker for an extracted archive with `fomod/ModuleConfig.xml` |

## Mapping to the native model `[REPO]`

- `requiredInstallFiles` -> Copy instructions on the component.
- Each group in each install step -> one `Option` per plugin plus one `Choose`
  instruction whose Source lists the option GUIDs (`Instruction.GetChosenOptions()`
  resolves the user's picks at install time).
- Group selection semantics are recorded on the Option: `InstallationMethod` holds
  the `FomodGroupType` name, `Heading` holds "step / group".
- All generated paths are sandboxed: sources start with
  `<<modDirectory>>/<archive-folder>/`, destinations with `<<kotorDirectory>>/`.
  `Choose` is the documented exception (Source lists GUIDs, per the repo rule).
- `conditionalFileInstalls`: a pattern whose flag dependencies are all set by one
  plugin attaches to that plugin's Option. gameDependency/fileDependency are
  treated as always-true with a logged warning. Unsupported shapes (Or, nested
  composites, flags spanning plugins) are skipped with a logged warning.

## Known limitations `[REPO]`

- File renames (`file` destination with a different file name) fall back to the
  source file name because the Copy action preserves names; a warning is logged.
- Folder installs map to a wildcard Copy (`<source>/*`) of the folder contents.
- `dependencyType` type descriptors only honor `defaultType`.
- Installer dialog does not render plugin images from `image path`.

## Deferred `[OPEN]`

- Plugin images from `image path`.

## Planned: CLI post-download parity `[REPO]`

Requirements: [docs/brainstorms/2026-06-14-fomod-cli-download-prompts-requirements.md](../brainstorms/2026-06-14-fomod-cli-download-prompts-requirements.md)

Plan: [docs/plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md](../plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md)

- Core `FomodPostDownloadOrchestrator` + CLI console host + `--fomod-skip` / `--fomod-choices`
- Full TTY wizard; non-TTY default warn-continue; convert output persists FOMOD state

## Post-download hook `[REPO]`

- `FomodArchiveProbe` detects `fomod/ModuleConfig.xml` inside downloaded archives via entry listing.
- `FomodPostDownloadPromptService` runs after GUI **Fetch Downloads** completes; optional prompt per archive.
- `FomodDownloadPromptState` stores dismissed/configured outcomes in resource handler metadata.
- `ArchiveEnumerationService` sets `FileTreeNode.IsFomodInstaller` when an archive contains FOMOD metadata.

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~Fomod"
```

Plans:

- [docs/plans/2026-06-12-115-fomod-parser-plan.md](../plans/2026-06-12-115-fomod-parser-plan.md)
- [docs/plans/2026-06-14-121-fomod-installer-dialog-plan.md](../plans/2026-06-14-121-fomod-installer-dialog-plan.md)
