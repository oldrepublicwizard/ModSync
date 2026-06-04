#!/usr/bin/env bash
# Print or run the recommended merge sequence for open PRs #110 and #111.
# Default is dry-run. --execute merges #110; --execute-all completes #110 and #111.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
agents_dir="$(dirname "${BASH_SOURCE[0]}")"
default_branch="${DEFAULT_BRANCH:-master}"
execute=false
execute_all=false

usage() {
  cat <<EOF
Usage: merge_open_prs.sh [--execute | --execute-all]

  Dry-run (default): print recommended merge steps for #110 then #111.

  --execute      On feat/wizard-archive-validation-parity: verify + gh pr merge 110 --merge
  --execute-all  Full handoff: merge #110, rebase #111 onto origin/${default_branch},
                 verify, push --force-with-lease, merge #111

See: docs/solutions/parallel-pr-merge-handoff-2026-06-03.md
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --execute) execute=true; shift ;;
    --execute-all) execute_all=true; execute=true; shift ;;
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
git -C "$repo_root" checkout feat/wizard-archive-validation-parity
"${agents_dir}/verify_open_pr_ready.sh"
gh pr merge 110 --merge
echo "PR #110 merged."

if [[ "$execute_all" != true ]]; then
  echo "Complete steps 2–3 above for PR #111, or re-run with --execute-all."
  exit 0
fi

echo "== Rebase and merge PR #111 =="
git -C "$repo_root" fetch origin
git -C "$repo_root" checkout feat/holocron-erf-nested-open
git -C "$repo_root" rebase "origin/${default_branch}"
"${agents_dir}/verify_open_pr_ready.sh"
git -C "$repo_root" push --force-with-lease origin feat/holocron-erf-nested-open
echo "Waiting for PR #111 CI..."
set +e
gh pr checks 111 --watch
checks_exit=$?
set -e
if [[ "$checks_exit" -ne 0 ]]; then
  echo "PR #111 CI not green (exit ${checks_exit}). Fix before: gh pr merge 111 --merge" >&2
  exit 2
fi
gh pr merge 111 --merge
echo "PR #111 merged. Handoff complete."
