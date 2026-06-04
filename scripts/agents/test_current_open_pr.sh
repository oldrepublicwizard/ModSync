#!/usr/bin/env bash
# Run PR-targeted tests for the current open feature branch (#110 or #111).
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
agents_dir="$(dirname "${BASH_SOURCE[0]}")"
branch="$(git -C "$repo_root" rev-parse --abbrev-ref HEAD 2>/dev/null || true)"

case "$branch" in
  feat/wizard-archive-validation-parity)
    exec "${agents_dir}/test_pr110_validation.sh" "$@"
    ;;
  feat/holocron-erf-nested-open)
    exec "${agents_dir}/test_pr111_holocron_bridge.sh" "$@"
    ;;
  *)
    echo "test_current_open_pr: unknown branch '${branch}'." >&2
    echo "Use test_pr110_validation.sh (#110) or test_pr111_holocron_bridge.sh (#111)." >&2
    exit 1
    ;;
esac
