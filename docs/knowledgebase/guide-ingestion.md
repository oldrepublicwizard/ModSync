# Guide ingestion (paste + draft instructions)

`[REPO]` How community install guides become instruction files with optional draft executable steps. Vision context: [product-vision.md](product-vision.md). CLI flags: [core-cli-reference.md](core-cli-reference.md).

## What shipped

| Capability | Surface | Agent path |
|------------|---------|------------|
| Format-sniff import (TOML → Markdown → YAML → XML/JSON) | GUI paste / file open; Core deserialize | `convert --stdin` or `convert -i` |
| Markdown guide → components (`Directions` preserved) | `MarkdownParser` | Same convert path |
| Prose → draft `Instruction` objects | `DraftInstructionService` + `NaturalLanguageInstructionParser` | `convert --parse-directions` |
| GUI clipboard paste | `ImportFromClipboardButton` → `FileLoadingService.LoadInstructionTextAsync` | Headless smoke: `GuiSmokeHeadlessTests`; real OS clipboard still `[UI]` |
| Guide emission (components → markdown) | `GenerateModDocumentation` | Round-trip covered by `GuideIngestionTests` + documentation tests |

Draft instructions are **review-flagged** in output (never auto-trusted). Paths must use `<<modDirectory>>` / `<<kotorDirectory>>` only.

## When drafts are generated (decision)

**Drafts from natural-language Directions are opt-in except for paste.**

| Path | Drafts from prose? | Why |
|------|--------------------|-----|
| GUI **paste / clipboard** | Yes (always) | Paste is an explicit “ingest this guide” action; users expect draft instructions |
| CLI `convert --parse-directions` | Yes (flag required) | Agent/parity path; keeps plain `convert` non-destructive |
| GUI **file open / drag-drop** of `.md` | No | Opening an authored guide or TOML companion must not rewrite empty `Instructions` with heuristic drafts |

File-based markdown import can still produce drafts by converting with `--parse-directions` (or pasting the file contents). There is no separate GUI feature flag for file open; paste + CLI is the intentional surface.

## Agent workflow

```bash
# Ingest a guide file to review-flagged TOML (opt-in drafts)
dotnet run --project src/ModSync.Core/ModSync.Core.csproj -f net9.0 -- \
  convert -i ./path/to/guide.md --parse-directions -f toml -o ./tmp/draft.toml

# Or pipe pasted text
cat clipboard.txt | dotnet run --project src/ModSync.Core/ModSync.Core.csproj -f net9.0 -- \
  convert --stdin --parse-directions -f toml -o ./tmp/draft.toml
```

Then review drafts, fix TOML as needed, and use `validate` / `install` as usual.

## Tests

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~GuideIngestionTests"
./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~GuiSmokeHeadlessTests"
```

## Related

- Requirements (completed): [docs/brainstorms/2026-07-13-guide-paste-ingestion-requirements.md](../brainstorms/2026-07-13-guide-paste-ingestion-requirements.md)
- Plan (completed): [docs/plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md)
- [agent-action-parity.md](agent-action-parity.md)
- [product-vision.md](product-vision.md)
