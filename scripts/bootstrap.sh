#!/usr/bin/env bash
set -euo pipefail

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET 10 SDK is required."
  exit 1
fi

dotnet --version
dotnet restore

echo
echo "Repository restored."
echo "Run: ./scripts/check.sh"
