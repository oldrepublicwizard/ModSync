---
title: "feat: guide paste ingestion + NaturalLanguageInstructionParser wiring"
status: completed
date: 2026-07-13
completed: 2026-07-13
origin: docs/brainstorms/2026-07-13-guide-paste-ingestion-requirements.md
related_strategy: STRATEGY.md + docs/knowledgebase/product-vision.md
---

# Plan 2026-07-13-001 — Guide paste ingestion

## Summary

Implement the first vision slice from `STRATEGY.md` (guide-ingestion track): a paste/clipboard import entry point in the GUI, wiring of the currently dead `NaturalLanguageInstructionParser` so imported guide prose produces draft `Instruction` objects, CLI parity for the same ingestion, and tests. See [docs/knowledgebase/product-vision.md](../knowledgebase/product-vision.md) for the vision-vs-current-state analysis this plan closes.

## Problem Frame

`[REPO]` Markdown guide import already works and round-trips: `src/ModSync.Core/Parsing/MarkdownParser.cs` parses Deadly Stream-style guides into components (prose preserved in `Directions`), verified by `DocumentationRoundTripTests`. Guide emission exists via `GenerateModDocumentation` in `src/ModSync.Core/Services/ModComponentSerializationService.cs`. Two gaps remain:

1. Ingestion is file/drag-drop only — no clipboard/paste entry point ("paste → it just loads").
2. `src/ModSync.Core/Parsing/NaturalLanguageInstructionParser.cs` (prose → typed `Instruction` objects) has zero references — imported guides get no executable instructions from their prose.

## Requirements

| ID | Requirement | Plan coverage |
|----|-------------|---------------|
| R1 | GUI paste/clipboard import routed through existing format-sniffing cascade | U1 |
| R2 | Imported guide prose produces draft, review-flagged instructions | U2 |
| R3 | Draft instruction paths obey `<<modDirectory>>`/`<<kotorDirectory>>` sandboxing | U2, U4 |
| R4 | Unparseable prose degrades gracefully to today's behavior | U2 |
| R5 | CLI parity: ingest guide file/stdin → TOML with draft instructions | U3 |
| R6 | Tests headless in `ModSync.Tests`; paste flow exercised in a real desktop session | U4 |

## Implementation Units

### U1. Paste/clipboard import entry point

**Goal:** GUI "Import from text/clipboard" action that routes pasted text through the existing content-sniffing cascade (`DeserializeModComponentFromString`, TOML → Markdown → YAML → XML/JSON) and, for markdown in editor mode, the existing `RegexImportDialog`.

**Requirements:** R1

**Files:**

- `src/ModSync.GUI/MainWindow.axaml.cs`
- `src/ModSync.GUI/Services/FileLoadingService.cs`

**Constraints:** No inline font/style on new XAML elements per `.cursorrules`.

---

### U2. Wire NaturalLanguageInstructionParser into import

**Goal:** After markdown import, run each component's `Directions` prose through the parser to produce draft `Instruction` objects — placeholder-prefixed paths per the path-sandboxing rules, flagged for review, never auto-trusted. Unparseable prose degrades gracefully to today's behavior.

**Requirements:** R2, R3, R4

**Dependencies:** U1

**Files:**

- `src/ModSync.Core/Parsing/NaturalLanguageInstructionParser.cs`
- `src/ModSync.Core/Parsing/MarkdownParser.cs` or the import service seam

---

### U3. CLI parity

**Goal:** Extend the Core CLI (`convert` or a new flag) to ingest a guide file/stdin and emit TOML with parsed draft instructions — agent-action-parity per repo rules.

**Requirements:** R5

**Dependencies:** U2

**Files:**

- `src/ModSync.Core/CLI/ModBuildConverter.cs`
- `docs/knowledgebase/core-cli-reference.md`, `docs/knowledgebase/agent-action-parity.md` (doc updates)

---

### U4. Tests

**Goal:** Coverage for the new ingestion paths, all in `src/ModSync.Tests`.

**Requirements:** R3, R6

**Dependencies:** U1–U3

**Test scenarios:**

- NL-parser cases against real guide snippets from `./mod-builds` markdown.
- Paste-cascade format sniffing (TOML/Markdown/YAML/XML/JSON inputs).
- Round-trip preservation (`DocumentationRoundTripTests`) still green.
- Sandboxed-path assertions on parsed instructions (`<<modDirectory>>`/`<<kotorDirectory>>` prefixes only).

## Verification Strategy

```bash
dotnet build ModSync.sln --configuration Debug
dotnet test src/ModSync.Tests/ModSync.Tests.csproj \
  --filter "FullyQualifiedName!~LongRunning" --configuration Debug
```

Manual (desktop, per `AGENTS.md`): launch via `./scripts/agents/launch_gui_desktop.sh`, exercise the paste flow in a real desktop session.

## Out of scope

- `modsync://` protocol scheme (separate entry-points track in `STRATEGY.md`).
- Publish/share flows for multi-author builds.
- Vision/strategy docs (landed 2026-07-13: `STRATEGY.md`, `docs/knowledgebase/product-vision.md`).
