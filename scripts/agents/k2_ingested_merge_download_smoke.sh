#!/usr/bin/env bash
# Merge neocities K2 Full + golden TOML, then download one mod via install --download --select mod:NAME.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

FIXTURE="${FIXTURE:-$ROOT/src/ModSync.Tests/Fixtures/k2_full_guide.md}"
GOLDEN="${GOLDEN:-$ROOT/mod-builds/TOMLs/KOTOR2_Full.toml}"
OUT_DIR="${OUT_DIR:-$ROOT/tmp/k2_merge_download_smoke}"
MERGED="${MERGED:-$OUT_DIR/k2_merged.toml}"
KOTOR_DIR="${KOTOR_DIR:-$OUT_DIR/kotor}"
MOD_DIR="${MOD_DIR:-$OUT_DIR/mods}"
DOWNLOAD_MOD="${DOWNLOAD_MOD:-Silent Sion Restoration}"
SKIP_DOWNLOAD=false

usage() {
  cat <<'EOF'
Usage: k2_ingested_merge_download_smoke.sh [options]

Merges the neocities K2 Full markdown fixture with golden KOTOR2_Full.toml, then
downloads one mod with install --download --select mod:NAME (network required).

Options:
  --fixture PATH       Neocities markdown fixture (default: k2_full_guide.md)
  --golden PATH        Golden TOML (default: ./mod-builds/TOMLs/KOTOR2_Full.toml)
  --out-dir PATH       Output directory (default: ./tmp/k2_merge_download_smoke)
  --download-mod NAME  Mod name for --select mod:NAME (default: Silent Sion Restoration)
  --skip-download      Merge + validate dry-run only; do not download
  -h, --help           Show this help

Environment:
  FIXTURE, GOLDEN, OUT_DIR, MOD_DIR, KOTOR_DIR, MERGED, DOWNLOAD_MOD
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --fixture) FIXTURE="$2"; shift 2 ;;
    --golden) GOLDEN="$2"; shift 2 ;;
    --out-dir) OUT_DIR="$2"; MERGED="$OUT_DIR/k2_merged.toml"; KOTOR_DIR="$OUT_DIR/kotor"; MOD_DIR="$OUT_DIR/mods"; shift 2 ;;
    --download-mod) DOWNLOAD_MOD="$2"; shift 2 ;;
    --skip-download) SKIP_DOWNLOAD=true; shift ;;
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

mkdir -p "$OUT_DIR" "$MOD_DIR"
"$ROOT/scripts/agents/create_template_kotor_install.sh" "$KOTOR_DIR" "$MOD_DIR" >/dev/null

strip_dependencies_for_mods() {
  local toml_path="$1"
  shift
  python3 - "$toml_path" "$@" <<'PY'
import re, sys
path = sys.argv[1]
names = sys.argv[2:]
text = open(path, encoding="utf-8").read()
for name in names:
    pattern = rf'(\[\[thisMod\]\]\nGuid = "[^"]+"\nName = "{re.escape(name)}"[\s\S]*?)(?=\n\[\[thisMod\]\]|\Z)'
    match = re.search(pattern, text)
    if not match:
        print(f"warning: mod block not found for {name!r}", file=sys.stderr)
        continue
    block = match.group(1)
    block = re.sub(r'\nDependencies = \[.*?\]\n', '\n', block)
    block = re.sub(r'\nRestrictions = \[.*?\]\n', '\n', block)
    text = text.replace(match.group(1), block)
open(path, "w", encoding="utf-8").write(text)
PY
}

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

if [[ "$SKIP_DOWNLOAD" == true ]]; then
  echo "Merge download smoke complete (merge + validate). Skipping network download."
  echo "  Merged TOML: $MERGED"
  exit 0
fi

echo "==> install --download --select mod:$DOWNLOAD_MOD"
strip_dependencies_for_mods "$MERGED" "$DOWNLOAD_MOD"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  install --input "$MERGED" --game-dir "$KOTOR_DIR" --source-dir "$MOD_DIR" \
  --select "mod:$DOWNLOAD_MOD" --download --skip-validation --best-effort -y

echo "Download smoke complete."
echo "  Merged TOML: $MERGED"
echo "  Mod workspace: $MOD_DIR"
ls -la "$MOD_DIR"/*.zip 2>/dev/null || echo "  (no .zip files at mod workspace root yet)"
