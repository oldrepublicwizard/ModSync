#!/usr/bin/env bash
# PR #110 — wizard validation parity (presenter + dialog mapper).
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
configuration="${CONFIGURATION:-Debug}"
project="${repo_root}/src/KOTORModSync.Tests/KOTORModSync.Tests.csproj"

echo "PR #110 validation tests (WizardValidationStagePresenter)..."
dotnet test "$project" --configuration "$configuration" \
  --filter "FullyQualifiedName~WizardValidationStagePresenter" "$@"

echo "PR #110 validation tests (ValidationPipelineDialogMapper)..."
dotnet test "$project" --configuration "$configuration" \
  --filter "FullyQualifiedName~ValidationPipelineDialogMapper" "$@"
