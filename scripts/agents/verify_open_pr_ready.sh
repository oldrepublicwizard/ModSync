#!/usr/bin/env bash
# Pre-merge verifier: PR-targeted tests + GitHub CI status for #110 or #111.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
agents_dir="$(dirname "${BASH_SOURCE[0]}")"
branch="$(git -C "$repo_root" rev-parse --abbrev-ref HEAD 2>/dev/null || true)"

pr_number=""
case "$branch" in
  feat/wizard-archive-validation-parity) pr_number=110 ;;
  feat/holocron-erf-nested-open) pr_number=111 ;;
  *)
    echo "verify_open_pr_ready: checkout feat/wizard-archive-validation-parity or feat/holocron-erf-nested-open." >&2
    exit 1
    ;;
esac

echo "== Local tests (PR #${pr_number}) =="
"${agents_dir}/test_current_open_pr.sh" "$@"

if ! command -v gh >/dev/null 2>&1; then
  echo "== GitHub CI: skipped (gh not installed) =="
  exit 0
fi

echo "== GitHub CI (PR #${pr_number}) =="
set +e
gh pr checks "$pr_number" 2>&1
exit_code=$?
set -e

case "$exit_code" in
  0) echo "CI checks passed on PR #${pr_number}." ;;
  8) echo "CI still pending on PR #${pr_number} — re-run after checks complete." >&2; exit 2 ;;
  *) echo "CI failed on PR #${pr_number} — fix before merge." >&2; exit 1 ;;
esac
