---
title: "FOMOD archive discovery after download"
status: completed
date: 2026-06-14
completed: 2026-07-13
origin: docs/plans/vortex-mo2-feature-parity-living-plan.md
---

# FOMOD archive discovery after download

## Summary

After GUI downloads finish, detect FOMOD installers inside downloaded archives and
optionally prompt the user to run the existing FOMOD configuration wizard. Skipping
is allowed and remembered per archive file.

## Problem Frame

FOMOD parser and installer dialog exist, but discovery still requires the user to
manually pick an extracted folder via **Configure FOMOD Mod**. Downloads already land
archives in the mod workspace without surfacing FOMOD packages automatically.

## Requirements

**Download detection and prompt**

- R1: After a GUI download session completes, scan selected components' downloaded
  archive files in the mod directory using archive entry listing (no full extract for
  detection).
- R2: When `fomod/ModuleConfig.xml` is found, show an optional post-download prompt to
  configure installer options now.
- R3: Choosing **No** dismisses the prompt for that archive on that component; the user
  can still use **Configure FOMOD Mod** manually later.
- R4: Choosing **Yes** extracts the archive to the mod workspace, runs the existing
  FOMOD wizard, and merges generated options/instructions into the existing component.

**Archive enumeration**

- R5: When building the mod file tree, mark archive nodes that contain a FOMOD
  installer so the UI can distinguish them later.

**Persistence**

- R6: Dismissed and configured outcomes are stored per archive file name on the
  component's resource metadata so prompts do not repeat unnecessarily.

## Success Criteria

- A downloaded archive containing `fomod/ModuleConfig.xml` triggers the post-download
  prompt once per archive until dismissed or configured.
- Accepting the prompt runs the existing FOMOD wizard without requiring a manual folder
  picker first.
- Archive enumeration marks FOMOD archives without requiring extraction.

## Scope Boundaries

**In scope**

- GUI download orchestration path used by **Fetch Downloads** (wizard and Getting
  Started).
- Reuse of `FomodDetector`, `FomodInstallerDialog`, and existing mapper/presenter stack.

**Deferred (completed separately)**

- CLI download/install parity → [2026-06-14-fomod-cli-download-prompts-requirements.md](2026-06-14-fomod-cli-download-prompts-requirements.md)
- Validation blocking when FOMOD choices are unset → `FomodConfigurationGate`

**Still deferred**

- Plugin images and advanced conditional file-install runtime beyond current mapper.

## Key Decisions

- Detection uses archive entry listing; extraction happens only when the user accepts
  the prompt.
- Prompt state is tracked per archive file name in resource handler metadata.
- Wizard output merges into the existing instruction-file component rather than
  creating a separate standalone component.

## Implemented

Shipped in PR #169 and follow-ups.

| Area | Path |
|------|------|
| Archive probe | `src/ModSync.Core/Services/Fomod/FomodArchiveProbe.cs` |
| Prompt state | `src/ModSync.Core/Services/Fomod/FomodDownloadPromptState.cs` |
| GUI post-download | `src/ModSync.GUI/Services/FomodPostDownloadPromptService.cs` |
| GUI dialog host | `src/ModSync.GUI/Services/FomodGuiPostDownloadHost.cs` |
| File-tree mark | `src/ModSync.GUI/Services/ArchiveEnumerationService.cs` |
| KB | `docs/knowledgebase/fomod-support.md`, `docs/knowledgebase/download-system.md` |

## Outstanding Questions

- None.
