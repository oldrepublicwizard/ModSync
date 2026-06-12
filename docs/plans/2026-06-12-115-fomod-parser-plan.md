# Plan 115: FOMOD ModuleConfig parser + mapping to native Option/Choose model

Phase 6 of the Vortex/MO2 feature-parity roadmap, first slice (Core only).

## Goal

Archives that ship a `fomod/ModuleConfig.xml` guided installer can be parsed into
ModSync's native `ModComponent`/`Option`/`Choose` model, so a later GUI slice can
present the FOMOD wizard and the existing instruction pipeline can execute the result.

## Scope (this slice)

### ModSync.Core (`src/ModSync.Core/Services/Fomod/`)

- `FomodInfo.cs` - model for `fomod/info.xml` (Name, Author, Version, Website, Description).
- `FomodModuleConfig.cs` - models for `fomod/ModuleConfig.xml`:
  `FomodModuleConfig`, `FomodInstallStep`, `FomodGroup`, `FomodPlugin`,
  `FomodFileInstall`, `FomodConditionFlag`, `FomodDependency`,
  `FomodConditionalInstallPattern`, plus enums `FomodGroupType`, `FomodPluginType`,
  `FomodDependencyOperator`, `FomodDependencyType`.
- `FomodParser.cs` - `ParseInfoXml` / `ParseModuleConfigXml` (string, stream, and file
  overloads) built on System.Xml.Linq. Tolerant of missing optional elements;
  throws `FormatException` on structurally invalid XML. Element lookup is
  case-insensitive so the camelCase ModuleConfig schema and the PascalCase info.xml
  schema are both handled.
- `FomodDetector.cs` - static helper that scans archive entry paths for
  `fomod/ModuleConfig.xml` (case-insensitive, any root prefix, both slash directions).
- `FomodToComponentMapper.cs` - maps parsed FOMOD + archive name into a `ModComponent`:
  - `requiredInstallFiles` become Copy instructions on the component.
  - Each installStep group becomes one `Option` per plugin; the group selection
    semantics (SelectExactlyOne, SelectAny, ...) are recorded on the Option's
    existing fields (`InstallationMethod` holds the group type, `Heading` holds
    "step / group" names) - no new model fields.
  - One `Choose` instruction per group whose Source lists the option GUIDs
    (consumed by `Instruction.GetChosenOptions()`).
  - Per-plugin files become Copy instructions inside the Option using
    `<<modDirectory>>/<archive-folder>/...` sources and `<<kotorDirectory>>/...`
    destinations (path sandboxing rule; Choose is the documented exception whose
    Source lists option GUIDs).
  - `conditionalFileInstalls`: patterns whose flag dependencies are all set by a
    single plugin are appended to that plugin's Option; gameDependency and
    fileDependency are treated as always-true with a logged warning; anything
    else (Or operators, nested composites, flags spanning plugins) is skipped
    with a logged warning.

### Tests (`src/ModSync.Tests`, single project)

- `FomodParserTests.cs` - realistic inline ModuleConfig.xml (steps, groups, plugins,
  files, flags, conditional installs), info.xml parsing, invalid XML throws
  `FormatException`, detector case-insensitivity.
- `FomodToComponentMapperTests.cs` - mapped component has expected Options, Choose
  wiring (GUIDs match, `GetChosenOptions` resolves), and every generated
  Source/Destination starts with `<<modDirectory>>` or `<<kotorDirectory>>`.

### Docs

- This plan doc plus a short `docs/knowledgebase/fomod-support.md` note and an index
  entry in `docs/knowledgebase/README.md`.

## Out of scope (follow-up slice)

- GUI `FomodInstallerDialog` (step wizard with images and live flag evaluation) -
  needs a real desktop validation session.
- Detection hook in `DownloadCacheService`/`ArchiveEnumerationService` offering the
  guided flow - deferred to avoid touching files owned by parallel work.
- Full dependency-typed plugin descriptors (`dependencyType` patterns beyond the
  default type) and real gameDependency/fileDependency evaluation.

## Verification

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~Fomod"
```
