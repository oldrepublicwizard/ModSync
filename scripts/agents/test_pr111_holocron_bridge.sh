#!/usr/bin/env bash
# PR #111 — Godot Holocron PyKotor bridge CLI tests (skips when PyKotor unavailable).
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
configuration="${CONFIGURATION:-Debug}"
project="${repo_root}/src/KOTORModSync.Tests/KOTORModSync.Tests.csproj"

echo "PR #111 Holocron bridge tests (KotorFormatBridgeCliTests)..."
dotnet test "$project" --configuration "$configuration" \
  --filter "FullyQualifiedName~KotorFormatBridgeCliTests" "$@"
