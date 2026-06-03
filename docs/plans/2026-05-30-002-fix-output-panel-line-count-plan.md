---
title: Fix embedded output panel line count
type: fix
status: completed
date: 2026-05-30
origin: GUI investigation session (EmbeddedLogPanel shows "0 lines" while log list has content)
---

# Fix embedded output panel line count

## Summary

Fix the main window `EmbeddedLogPanel` header so the line count matches visible log entries. Add a headless UI test that asserts the count updates when log lines are appended.

---

## Problem Frame

During desktop GUI validation, the Output panel header stayed at **"0 lines"** while the `ListBox` showed many log lines after loading `KOTOR1_Full.toml`. `UpdateLineCount()` exists but `LogLines` mutations may occur off the UI thread (via `Logger.Logged`), which is unsafe for Avalonia `ObservableCollection` and can desync manual header updates from bound list content.

---

## Requirements

- R1. Line count in the panel header reflects `LogLines.Count` after append, bulk load, and clear.
- R2. All `LogLines` / `_logBuilder` mutations run on the Avalonia UI thread.
- R3. Headless test covers append → count update without requiring X11 automation.
- R4. No theme/font changes in XAML (per `.cursorrules`).

---

## Scope Boundaries

- **In scope:** `EmbeddedLogPanel`, `OutputViewModel`, one headless test.
- **Deferred:** Extracting `AddPipelineStageIssuesToDialog` to shared mapper (separate refactor).
- **Deferred:** MainWindow decomposition, duplicate wizard hosts.

---

## Key Technical Decisions

- Prefer `ObservableCollection.CollectionChanged` → update header on UI thread rather than only manual `UpdateLineCount()` calls at call sites.
- Keep `LineCountText` as code-behind target (no new XAML font/style); update from a single handler.
- Test via Avalonia headless `UserControl` host pattern used elsewhere in `KOTORModSync.Tests`.

---

## Implementation Units

### U1. UI-thread log mutations and line-count sync

**Goal:** Header count always matches bound list.

**Files:**
- Modify: `src/KOTORModSync.GUI/Controls/EmbeddedLogPanel.axaml.cs`
- Modify: `src/KOTORModSync.GUI/OutputLogViewModel.cs` (optional `LineCount` property if binding simplifies)

**Approach:** Marshal `AppendLogLine` / `ClearLog_Click` body mutations through `Dispatcher.UIThread`; subscribe to `LogLines.CollectionChanged` to refresh header text; call initial count after bulk load in `EnsureLoggerAttached`.

**Test expectation:** Covered in U2.

**Verification:** Manual desktop run shows non-zero line count after file load; header resets to "0 lines" after Clear.

### U2. Headless regression test

**Goal:** Prevent regression without desktop automation.

**Files:**
- Create or modify: `src/KOTORModSync.Tests/HeadlessUITests/EmbeddedLogPanelHeadlessTests.cs`

**Approach:** Instantiate `EmbeddedLogPanel`, append lines via public/test hook or Logger simulation, assert `LineCountText.Text` matches count.

**Test scenarios:**
- **Happy path:** After three appends, header reads `"3 lines"`.
- **Edge case:** After clear, header reads `"0 lines"`.
- **Edge case:** Single line reads `"1 line"`.

**Verification:** `dotnet test` with filter `FullyQualifiedName~EmbeddedLogPanelHeadlessTests` passes under net9.0 without `RunSettingsFilePath` category filter blocking.

---

## Success Criteria

- Output header line count matches list content during normal app startup logging.
- Headless test passes in CI.
