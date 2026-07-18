#!/usr/bin/env bash
# Full K2 neocities round-trip smoke: golden URLs + ingested NLP → download → extract → Override install.
# Wraps the LongRunning integration test (network required; mod-builds at repo root).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

GOLDEN="${GOLDEN:-$ROOT/mod-builds/TOMLs/KOTOR2_Full.toml}"
FILTER="${FILTER:-FullyQualifiedName~K2FullGuideFixture_RoundTripSilentSion_DownloadAndInstalls_LongRunning}"
MOD="${MOD:-Silent Sion Restoration}"

usage() {
  cat <<'EOF'
Usage: k2_merged_roundtrip_download_install_smoke.sh [options]

Runs a K2 Full round-trip LongRunning test: merge neocities fixture with golden TOML,
overlay ingested NLP instructions, download the archive, extract, and install to Override.

Requires:
  - mod-builds cloned at ./mod-builds (KOTOR2_Full.toml)
  - network access to Deadly Stream download URLs

Options:
  --golden PATH   Golden TOML path (default: ./mod-builds/TOMLs/KOTOR2_Full.toml)
  --mod NAME      Mod name for round-trip overlay (default: Silent Sion Restoration)
  -h, --help      Show this help

Environment:
  GOLDEN, FILTER, MOD (MOD selects default FILTER when FILTER unset)

Examples:
  ./scripts/agents/k2_merged_roundtrip_download_install_smoke.sh
  MOD="Prestige Class Saving Throw Fixes" \\
    FILTER='FullyQualifiedName~K2FullGuideFixture_RoundTripPrestige_DownloadAndInstalls_LongRunning' \\
    ./scripts/agents/k2_merged_roundtrip_download_install_smoke.sh
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --golden) GOLDEN="$2"; shift 2 ;;
    --mod) MOD="$2"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
  esac
done

if [[ "$MOD" == "Prestige Class Saving Throw Fixes" && "$FILTER" == "FullyQualifiedName~K2FullGuideFixture_RoundTripSilentSion_DownloadAndInstalls_LongRunning" ]]; then
  FILTER="FullyQualifiedName~K2FullGuideFixture_RoundTripPrestige_DownloadAndInstalls_LongRunning"
fi

if [[ ! -f "$GOLDEN" ]]; then
  echo "Golden TOML not found: $GOLDEN" >&2
  echo "Clone mod-builds: git clone https://github.com/th3w1zard1/mod-builds ./mod-builds" >&2
  exit 1
fi

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$PATH"

echo "==> dotnet test (round-trip download + install)"
dotnet test "$ROOT/src/ModSync.Tests/ModSync.Tests.csproj" -c Debug -f net9.0 \
  --filter "$FILTER"

echo "Round-trip download+install smoke passed."
