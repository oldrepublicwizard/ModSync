# FOMOD installer support

Status: Core parser and mapping layer only (Phase 6, slice 1). GUI wizard and
archive detection hook are deferred to a follow-up slice.

## What exists `[REPO]`

`src/ModSync.Core/Services/Fomod/` contains:

| File | Purpose |
|------|---------|
| `FomodInfo.cs` | Model for `fomod/info.xml` (Name, Author, Version, Website, Description) |
| `FomodModuleConfig.cs` | Models for `fomod/ModuleConfig.xml`: install steps, groups, plugins, file installs, condition flags, dependency trees, conditional install patterns |
| `FomodParser.cs` | `ParseInfoXml` / `ParseModuleConfigXml` (string/stream/file overloads); case-insensitive element lookup; `FormatException` on invalid XML |
| `FomodDetector.cs` | Finds `fomod/ModuleConfig.xml` in an archive entry listing (case-insensitive, any root prefix) |
| `FomodToComponentMapper.cs` | Translates a parsed FOMOD into a native `ModComponent` |

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

## Deferred (follow-up slice) `[OPEN]`

- `FomodInstallerDialog` GUI step wizard with images and live flag evaluation.
- Detection hook in `DownloadCacheService`/`ArchiveEnumerationService` that offers
  the guided flow when `FomodDetector` matches an archive.

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~Fomod"
```

Plan: [docs/plans/2026-06-12-115-fomod-parser-plan.md](../plans/2026-06-12-115-fomod-parser-plan.md)
