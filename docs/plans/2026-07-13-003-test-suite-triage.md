---
title: "test: suite triage 2026-07-13"
status: active
date: 2026-07-13
---

# Test suite triage (2026-07-13)

Non-interactive run (worktree `ModSync-test-suite-triage`, branch `test/2026-07-13-suite-triage`, base `origin/master`):

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName!~LongRunning" \
  --configuration Debug
```

## Latest results (after fixes)

| Adapter | Passed | Failed | Skipped | Notes |
|---|---:|---:|---:|---|
| **xUnit** (mixed run) | **248** | **0** | **22** | FileWatcher event suites + 2 AvaloniaFact skips |
| **NUnit** (mixed run) | — | — | — | **Explore crash residual** (see below) |

Targeted confirmation after FontManager + ValidationService follow-ups:

- `ModSync.Tests.ValidationServiceTests` — **6/6 passed**
- `ModSync.Tests.MenuBuilderServiceTests` — **6/6 passed**
- `ComponentState_InstallState_TransitionsCorrectly` — **passed** (fonts fixed)

## Failures observed → actions

| Area | Classification | Action |
|---|---|---|
| `StepNavigationServiceTests` / `GuiPathServiceTests` | Static `MainConfig` race | **Fixed** — `MainConfigStaticState` collection + lock/reset |
| `ValidationServiceTests` dependency details | Same static `allComponents` race | **Fixed** — same collection; instructions seeded so dependency is primary error |
| `CrossPlatformFileWatcherTests` (events) | Environmental inotify | **Skipped** — reason `FileWatcherEventsUnreliable` (no XML punctuation) |
| `CrossPlatformFileWatcherIntegrationTests` | Same | **Class `[Ignore("FileWatcherEventsUnreliable")]`** |
| `SettingsServiceTests` EmptyPaths | Avalonia headless flake | **Hardened** — clear `PathSuggestions` |
| `MenuBuilderServiceTests` / MainWindow headless | Missing Inter / SettingsService ctor order | **Fixed** — `Avalonia.Fonts.Inter` + `WithInterFont` + `FontManagerOptions{DefaultFamilyName=Inter}`; construct `SettingsService` before pickers |
| NUnit explore mid-suite | Infra: `TestFilter.FromXml` ArgumentOutOfRange when VSTest feeds `FullyQualifiedName!~LongRunning` (+ runsettings Slow exclude) into NUnit3TestAdapter Explore | **Residual** — xUnit green; NUnit suites still pass when targeted alone |

## Residuals (not fixed this pass)

1. **NUnit3TestAdapter Explore crash** on full `FullyQualifiedName!~LongRunning` mixed-assembly runs (`TNode.FirstChild` empty). Class Ignore punctuation was one contributor; crash persists with the `!~` filter shape. Follow-up: split adapters, upgrade adapter, or Category-based LongRunning exclusion for NUnit.
2. **FileWatcher event suites** remain skipped (AGENTS.md cloud/inotify expectation).
3. **Static `MainConfig` paths/components** — longer-term: stop storing install paths on process-wide statics so collections aren't required.
4. Concurrent rebuilds can delete `ModSync.Tests.dll` under a running testhost and hang/abort the suite.

## Commits on this branch

- `36839166` — initial triage fix set + plan doc
- follow-up — FontManagerOptions + ValidationServiceTests serialize/seed
