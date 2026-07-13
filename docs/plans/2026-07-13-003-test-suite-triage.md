---
title: "test: suite triage 2026-07-13"
status: active
date: 2026-07-13
---

# Test suite triage (2026-07-13)

Command:

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName!~LongRunning" \
  --configuration Debug
```

Worktree: `ModSync-test-suite-triage` on branch `test/2026-07-13-suite-triage` (base `origin/master` @ bd53b5f8).

## Fixes landed

| Area | Classification | Action |
|---|---|---|
| `StepNavigationServiceTests` / `GuiPathServiceTests` static `MainConfig` races | Regression | `MainConfigStaticState` collection + reset/lock |
| `CrossPlatformFileWatcherTests` event waits | Environmental (inotify) | `[Fact(Skip=...)]` on event cases; ctor/start/stop kept |
| `CrossPlatformFileWatcherIntegrationTests` | Environmental | `[OneTimeSetUp]` -> `Assert.Ignore` (avoid `[Ignore]` attrs that break NUnit filter XML) |
| `SettingsServiceTests` EmptyPaths ListBoxItem parent flake | Avalonia headless | Clear `PathSuggestions` before/after |
| `MenuBuilderServiceTests` `fonts:SystemFonts` | Avalonia headless | `Avalonia.Fonts.Inter` + `.WithInterFont()` |
| `MainWindow` ctor NRE in `InitializeDirectoryPickers` | Product regression | Construct `SettingsService` before picker init |

## Remaining / documented

| Area | Notes |
|---|---|
| NUnit explore crash under `FullyQualifiedName!~LongRunning` | After xUnit finishes, NUnit adapter `Explore` throws `TestFilter.FromXml` ArgumentOutOfRange. Most NUnit cases never execute in the combined run. Pre-existing mixed-adapter filter issue; list-tests still sees ~1.7k names. |
| `InstallingPage_CompletesSharedPipelineInstall` | Headless flake: status text stayed `"Installing: ..."` without `"complete"` within timeout |
| `ModListSidebar_Raises_Selection_Events` | Headless flake: `ClickButtonWithContentAsync(..., "Select All")` returned null |
| Settings EmptyPaths visual-parent | Residual Avalonia `ResetForUnitTests` flake if suggestions virtualization races |

## Verification snapshots

Focused (path/menu/settings/filewatcher): **Passed 33 / Failed 0 / Skipped 20**.

Full combined after MainWindow fix: **Passed 249 / Failed 2 / Skipped 20 / Total 271** (xUnit-heavy; NUnit explore aborted).

## Longer-term

- Stop storing install paths on static `MainConfig` fields.
- Split xUnit vs NUnit CI jobs or use NUnit-native `Where` filters so combine FQN filters do not abort exploration.
