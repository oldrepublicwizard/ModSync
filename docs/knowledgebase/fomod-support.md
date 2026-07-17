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
- In-validate **Configure FOMOD** action / `fomod configure` CLI verb (recovery hints only today).

## Post-download hook + CLI parity `[REPO]`

**Shipped.** Requirements (completed): [archive discovery](../brainstorms/2026-06-14-fomod-archive-discovery-requirements.md), [CLI prompts](../brainstorms/2026-06-14-fomod-cli-download-prompts-requirements.md). Plan 123: [shipped](../plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md).

- `FomodArchiveProbe` detects `fomod/ModuleConfig.xml` inside downloaded archives via entry listing.
- `FomodPostDownloadOrchestrator` + `IFomodPostDownloadHost` adapters unify GUI and CLI after download.
- GUI: `FomodPostDownloadPromptService` / `FomodGuiPostDownloadHost` after **Fetch Downloads**.
- CLI `install -d` / `convert -d` / `merge -d`: TTY wizard, warn-continue, `--fomod-skip`, `--fomod-choices` / `MODSYNC_FOMOD_CHOICES` (also `--interactive` / `--non-interactive`, env `MODSYNC_FOMOD_POST_DOWNLOAD_MODE`).
- `FomodDownloadPromptState` stores dismissed/configured/warned outcomes in resource handler metadata.
  - **`configured`**: permanently skips the Fetch Downloads / CLI post-download prompt; required to pass `FomodConfigurationGate`.
  - **`dismissed`**: does **not** pass the gate; Fetch Downloads / CLI will **re-prompt** (documented recovery path).
  - **`warned`**: CLI warn-continue / non-TTY default; does **not** pass the gate, and **does not re-prompt** on later Fetch/CLI runs (avoids spam). Clear by configuring the archive (wizard / `--fomod-choices`) or resetting prompt metadata.
- `FomodConfigurationGate` blocks validate and install unless every detected FOMOD archive on selected mods (plus hard dependencies) is `configured`; dismiss/skip/warned do not pass the gate. Unreadable downloaded archives fail closed. Missing mod directory fails closed.
  - **R1 soft check:** `configured` without any archive-scoped install instructions (`<<modDirectory>>/<archive-folder>/...`) emits a **warning** (does not fail the gate). Re-run Configure FOMOD / Fetch Downloads so merger output is applied.
  - **R3 missing archives:** registered archive paths missing on disk are **fail-closed** when the archive already has FOMOD prompt state (configured/dismissed/warned). Otherwise they emit a soft warning (generic missing downloads stay with component archive validation).
- `ArchiveEnumerationService` sets `FileTreeNode.IsFomodInstaller` when an archive contains FOMOD metadata.

Non-TTY **warn-continue** / `--fomod-skip` print recovery hints (`FomodConfigurationGate.RecoveryHint`); they do **not** satisfy the configured-only gate.

## Verification

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~Fomod"
```

Plans:

- [docs/plans/2026-06-12-115-fomod-parser-plan.md](../plans/2026-06-12-115-fomod-parser-plan.md)
- [docs/plans/2026-06-14-121-fomod-installer-dialog-plan.md](../plans/2026-06-14-121-fomod-installer-dialog-plan.md)
- [docs/plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md](../plans/2026-06-14-123-feat-fomod-cli-download-prompts-plan.md) (shipped; residual TTY polish / in-validate recovery are deferred)
