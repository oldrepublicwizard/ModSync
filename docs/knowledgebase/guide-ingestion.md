# Guide ingestion (paste + draft instructions)

`[REPO]` How community install guides become instruction files with optional draft executable steps. Vision context: [product-vision.md](product-vision.md). CLI flags: [core-cli-reference.md](core-cli-reference.md).

## What shipped

| Capability | Surface | Agent path |
|------------|---------|------------|
| Format-sniff import (TOML → Markdown → YAML → XML/JSON) | GUI paste / file open; Core deserialize | `convert --stdin` or `convert -i` |
| Markdown guide → components (`Directions` preserved) | `MarkdownParser` | Same convert path |
| Prose → draft `Instruction` objects | `DraftInstructionService` + `NaturalLanguageInstructionParser` | `convert --parse-directions` |
| GUI clipboard paste | `ImportFromClipboardButton` → `FileLoadingService.ImportFromTextAsync` | Headless smoke: `GuiSmokeHeadlessTests`; real OS clipboard still `[UI]` |

Draft instructions are **review-flagged** in output (never auto-trusted). Paths must use `<<modDirectory>>` / `<<kotorDirectory>>` only.

## Agent workflow

```bash
# Ingest a guide file to review-flagged TOML
dotnet run --project src/ModSync.Core/ModSync.Core.csproj -f net9.0 -- \
  convert -i ./path/to/guide.md --parse-directions -f toml -o ./tmp/draft.toml

# Pipe pasted text (Linux example)
xclip -o | dotnet run --project src/ModSync.Core/ModSync.Core.csproj -f net9.0 -- \
  convert --stdin --parse-directions -f toml -o ./tmp/draft.toml
# Or: cat clipboard.txt | …
```

Then review drafts, fix TOML as needed, and use `validate` / `install` as usual.

## Tests

```bash
dotnet test src/ModSync.Tests/ModSync.Tests.csproj --filter "FullyQualifiedName~GuideIngestionTests"
./scripts/agents/run_headless_tests.sh --filter "FullyQualifiedName~GuiSmokeHeadlessTests"
```

## Related

- [agent-action-parity.md](agent-action-parity.md)
- [instruction-format.md](instruction-format.md)
- Requirements: [docs/brainstorms/2026-07-13-guide-paste-ingestion-requirements.md](../brainstorms/2026-07-13-guide-paste-ingestion-requirements.md)
- Plan: [docs/plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md](../plans/2026-07-13-001-feat-guide-paste-ingestion-plan.md)
