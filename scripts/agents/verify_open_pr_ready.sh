#!/usr/bin/env bash
# Pre-merge verifier: wizard validation tests + GitHub CI for the current branch PR (if any).
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
agents_dir="$(dirname "${BASH_SOURCE[0]}")"

echo "== Local tests (wizard validation) =="
"${agents_dir}/test_pr110_validation.sh" "$@"

if ! command -v gh >/dev/null 2>&1; then
  echo "== GitHub CI: skipped (gh not installed) =="
  exit 0
fi

pr_number="$(gh pr view --json number -q .number 2>/dev/null || true)"
if [ -z "$pr_number" ]; then
  echo "== GitHub CI: skipped (no open PR for current branch) =="
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
