#!/usr/bin/env bash
# Run wizard validation regression tests (PR #110 scope).
set -euo pipefail

agents_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "${agents_dir}/test_pr110_validation.sh" "$@"
