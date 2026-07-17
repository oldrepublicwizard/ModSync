#!/usr/bin/env bash
# Merge neocities K2 Full fixture with golden mod-builds/KOTOR2_Full.toml for download URLs.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

FIXTURE="${FIXTURE:-$ROOT/src/ModSync.Tests/Fixtures/k2_full_guide.md}"
GOLDEN="${GOLDEN:-$ROOT/mod-builds/TOMLs/KOTOR2_Full.toml}"
OUT_DIR="${OUT_DIR:-$ROOT/tmp/k2_merge_golden_smoke}"
MERGED="${MERGED:-$OUT_DIR/k2_merged.toml}"
KOTOR_DIR="${KOTOR_DIR:-$OUT_DIR/kotor}"
MOD_DIR="${MOD_DIR:-$OUT_DIR/mods}"

usage() {
  cat <<'EOF'
Usage: k2_ingested_merge_golden_smoke.sh [options]

Merges the neocities K2 Full markdown fixture with golden KOTOR2_Full.toml
(existing order; keep golden instructions, options, and download URLs).

Options:
  --fixture PATH    Neocities markdown fixture (default: k2_full_guide.md)
  --golden PATH     Golden TOML (default: ./mod-builds/TOMLs/KOTOR2_Full.toml)
  --out-dir PATH    Output directory (default: ./tmp/k2_merge_golden_smoke)
  -h, --help        Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --fixture) FIXTURE="$2"; shift 2 ;;
    --golden) GOLDEN="$2"; shift 2 ;;
    --out-dir) OUT_DIR="$2"; MERGED="$OUT_DIR/k2_merged.toml"; KOTOR_DIR="$OUT_DIR/kotor"; MOD_DIR="$OUT_DIR/mods"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
  esac
done

if [[ ! -f "$FIXTURE" ]]; then
  echo "Fixture not found: $FIXTURE" >&2
  exit 1
fi

if [[ ! -f "$GOLDEN" ]]; then
  echo "Golden TOML not found: $GOLDEN" >&2
  echo "Clone mod-builds: git clone https://github.com/th3w1zard1/mod-builds ./mod-builds" >&2
  exit 1
fi

mkdir -p "$OUT_DIR"
"$ROOT/scripts/agents/create_template_kotor_install.sh" "$KOTOR_DIR" "$MOD_DIR" >/dev/null

dotnet build "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -c Debug -f net9.0 -v q

echo "==> merge (golden existing + neocities incoming)"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  merge --existing "$GOLDEN" --incoming "$FIXTURE" \
  --use-existing-order --prefer-existing-instructions --prefer-existing-options --prefer-existing-modlinks \
  -f toml -o "$MERGED" --plaintext

echo "==> validate --dry-run-only (structure + VFS; archives may be missing locally)"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  validate --input "$MERGED" --game-dir "$KOTOR_DIR" --source-dir "$MOD_DIR" \
  --dry-run-only --errors-only --best-effort || true

echo "Merge smoke complete."
echo "  Merged TOML: $MERGED"
echo "  Next: stage/download mods, then install with --select mod:NAME"
