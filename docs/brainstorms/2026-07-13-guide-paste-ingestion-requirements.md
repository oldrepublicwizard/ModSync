---
title: "Guide paste ingestion + draft instructions"
status: completed
date: 2026-07-13
completed: 2026-07-13
strategy_track: Guide ingestion
plan: docs/plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md
origin: STRATEGY.md + docs/knowledgebase/product-vision.md
---

# Guide paste ingestion + draft instructions

## Summary

Players and authors can paste (or pipe) an existing community install guide into ModSync and get components plus review-flagged draft `Instruction` objects from prose — no re-entry of what they already wrote.

## Problem Frame

Markdown guide import and guide emission already existed, but ingestion was file/drag-drop only, and `NaturalLanguageInstructionParser` was dead code. The STRATEGY guide-ingestion track needed a clipboard/stdin entry point and wired draft-instruction generation.

## Requirements

- R1: GUI paste/clipboard import routes text through the existing format-sniffing cascade (`DeserializeModComponentFromString`: TOML → Markdown → YAML → XML/JSON).
- R2: After import, components with `Directions` prose and no authored instructions get draft instructions from `NaturalLanguageInstructionParser` via `DraftInstructionService`.
- R3: Draft paths obey `<<modDirectory>>` / `<<kotorDirectory>>` sandboxing; drafts are never auto-trusted (review flag / log warning).
- R4: Unparseable prose degrades gracefully (no drafts; components still load).
- R5: CLI parity: `convert --stdin --parse-directions` (and file input) emit TOML with draft instructions.
- R6: Tests in `ModSync.Tests`; paste control smoke via Avalonia headless; real OS clipboard remains a desktop check.

## Success Criteria

- Getting Started **Import from Clipboard** loads a pasted Deadly Stream-style guide into components.
- Draft instructions appear for prose-only components and are flagged for review.
- `convert --stdin --parse-directions` produces equivalent TOML for agents.
- Round-trip / documentation tests stay green.

## Scope Boundaries

**In scope:** Paste entry point, NL parser wiring, CLI stdin/parse-directions, tests, KB updates.

**Out of scope:** `modsync://` deep links; publish/share multi-author flows; auto-trusting drafts at install time.

## Implemented

Shipped in `a37efbaf` (feat) + headless smoke (`653c7771`).

| Area | Path |
|------|------|
| Draft wiring | `src/ModSync.Core/Parsing/DraftInstructionService.cs` |
| NL parser | `src/ModSync.Core/Parsing/NaturalLanguageInstructionParser.cs` |
| GUI import | `FileLoadingService.ImportFromTextAsync`, `ImportFromClipboardButton` |
| CLI | `ModBuildConverter` `--stdin` / `--parse-directions` |
| Tests | `GuideIngestionTests`, `GuiSmokeHeadlessTests` |
| Plan | [docs/plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md) → **completed** |
| KB | [docs/knowledgebase/guide-ingestion.md](../knowledgebase/guide-ingestion.md) |

## Strategy alignment

Closes STRATEGY **Guide ingestion** entry-point + prose→instruction gaps called out in `docs/knowledgebase/product-vision.md` rows 1b/1c.
