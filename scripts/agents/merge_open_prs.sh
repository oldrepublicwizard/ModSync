#!/usr/bin/env bash
# Print or run the recommended merge sequence for open PRs #110 and #111.
# Default is dry-run. Pass --execute to merge #110 after verify (maintainer only).
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
agents_dir="$(dirname "${BASH_SOURCE[0]}")"
default_branch="${DEFAULT_BRANCH:-master}"
execute=false

usage() {
  cat <<EOF
Usage: merge_open_prs.sh [--execute]

  Dry-run (default): print recommended merge steps for #110 then #111.

  --execute  On feat/wizard-archive-validation-parity, run verify_open_pr_ready.sh
             then: gh pr merge 110 --merge
             (#111 still requires rebase onto origin/${default_branch} before merge)

See: docs/solutions/parallel-pr-merge-handoff-2026-06-03.md
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --execute) execute=true; shift ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage; exit 1 ;;
  esac
done

cat <<EOF
== Parallel PR merge sequence (dry-run) ==

1. On feat/wizard-archive-validation-parity:
   ./scripts/agents/verify_open_pr_ready.sh
   gh pr merge 110 --merge

2. Rebase Holocron branch onto origin/${default_branch}:
   git fetch origin
   git checkout feat/holocron-erf-nested-open
   git rebase origin/${default_branch}
   ./scripts/agents/verify_open_pr_ready.sh
   git push --force-with-lease origin feat/holocron-erf-nested-open

3. Merge Holocron:
   gh pr merge 111 --merge

Handoff: docs/solutions/parallel-pr-merge-handoff-2026-06-03.md
EOF

if [[ "$execute" != true ]]; then
  echo ""
  echo "Dry-run only. Re-run with --execute to merge PR #110 after verify (wizard branch required)."
  exit 0
fi

branch="$(git -C "$repo_root" rev-parse --abbrev-ref HEAD)"
if [[ "$branch" != "feat/wizard-archive-validation-parity" ]]; then
  echo "merge_open_prs --execute: checkout feat/wizard-archive-validation-parity first (on '${branch}')." >&2
  exit 1
fi

echo "== Executing PR #110 merge =="
"${agents_dir}/verify_open_pr_ready.sh"
gh pr merge 110 --merge
echo "PR #110 merged. Complete steps 2–3 above for PR #111."
