#!/usr/bin/env bash
# Smoke-test neocities K2 Full guide round-trip: convert → validate dry-run → optional install.
#
# The site-scraped fixture has no download URLs; this script validates structure and can
# install mods when you pre-stage archives under the mod workspace (see --install-mod).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

FIXTURE="${FIXTURE:-$ROOT/src/ModSync.Tests/Fixtures/k2_full_guide.md}"
OUT_DIR="${OUT_DIR:-$ROOT/tmp/k2_ingested_smoke}"
KOTOR_DIR="${KOTOR_DIR:-$OUT_DIR/kotor}"
MOD_DIR="${MOD_DIR:-$OUT_DIR/mods}"
TOML="${TOML:-$OUT_DIR/k2_full_ingested.toml}"
INSTALL_MOD="${INSTALL_MOD:-}"
SKIP_INSTALL=false

usage() {
  cat <<'EOF'
Usage: k2_ingested_roundtrip_smoke.sh [options]

Runs convert (--parse-directions) on the neocities K2 Full fixture, validate --dry-run-only,
and optionally install one mod when files are staged in the mod workspace.

Options:
  --fixture PATH       Markdown input (default: site-scraped k2_full_guide.md fixture)
  --out-dir PATH       Working directory (default: ./tmp/k2_ingested_smoke)
  --install-mod NAME   After validate, run install --select mod:NAME (requires staged files)
  --skip-install       Do not run install even if INSTALL_MOD is set
  -h, --help           Show this help

Environment:
  FIXTURE, OUT_DIR, KOTOR_DIR, MOD_DIR, TOML, INSTALL_MOD

Example (Silent Sion loose-file smoke):
  mkdir -p tmp/k2_ingested_smoke/mods
  printf 'dlg\n' > tmp/k2_ingested_smoke/mods/153sion.dlg
  ./scripts/agents/k2_ingested_roundtrip_smoke.sh --install-mod "Silent Sion Restoration"
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --fixture) FIXTURE="$2"; shift 2 ;;
    --out-dir) OUT_DIR="$2"; KOTOR_DIR="$OUT_DIR/kotor"; MOD_DIR="$OUT_DIR/mods"; TOML="$OUT_DIR/k2_full_ingested.toml"; shift 2 ;;
    --install-mod) INSTALL_MOD="$2"; shift 2 ;;
    --skip-install) SKIP_INSTALL=true; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
  esac
done

if [[ ! -f "$FIXTURE" ]]; then
  echo "Fixture not found: $FIXTURE" >&2
  exit 1
fi

mkdir -p "$OUT_DIR" "$MOD_DIR"
"$ROOT/scripts/agents/create_template_kotor_install.sh" "$KOTOR_DIR" "$MOD_DIR" >/dev/null

# shellcheck source=common.sh
source "$(dirname "${BASH_SOURCE[0]}")/common.sh"
ensure_core_resources_symlink "$ROOT"
if [[ -x "$ROOT/scripts/agents/ensure_linux_holopatcher.sh" ]]; then
  "$ROOT/scripts/agents/ensure_linux_holopatcher.sh" || true
fi

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

echo "==> convert --parse-directions"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  convert --input "$FIXTURE" -f toml --parse-directions -o "$TOML" --plaintext

echo "==> validate --dry-run-only"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  validate --input "$TOML" --game-dir "$KOTOR_DIR" --source-dir "$MOD_DIR" \
  --dry-run-only --errors-only

if [[ "$SKIP_INSTALL" == true || -z "$INSTALL_MOD" ]]; then
  echo "Round-trip smoke complete (convert + validate). Skipping install."
  echo "  TOML: $TOML"
  exit 0
fi

echo "==> install --select mod:$INSTALL_MOD"
strip_dependencies_for_mods "$TOML" "$INSTALL_MOD"
dotnet run --project "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -f net9.0 --no-build -- \
  install --input "$TOML" --game-dir "$KOTOR_DIR" --source-dir "$MOD_DIR" \
  --select "mod:$INSTALL_MOD" --skip-validation --best-effort -y

echo "Install complete for mod:$INSTALL_MOD"
echo "  KOTOR Override: $KOTOR_DIR/Override"
echo "  TOML: $TOML"
