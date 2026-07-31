#!/usr/bin/env bash
set -euo pipefail

python scripts/check_docs.py
python scripts/check_architecture.py

dotnet restore

dotnet build \
  src/MotionControl.OperatorConsole \
  --configuration Release \
  --no-restore

dotnet test \
  tests/MotionControl.Domain.Tests \
  --configuration Release \
  --no-restore

dotnet test \
  tests/MotionControl.Application.Tests \
  --configuration Release \
  --no-restore

dotnet test \
  tests/MotionControl.IntegrationTests \
  --configuration Release \
  --no-restore

dotnet run \
  --project src/MotionControl.OperatorConsole \
  --configuration Release \
  --no-restore \
  -- normal

dotnet run \
  --project src/MotionControl.OperatorConsole \
  --configuration Release \
  --no-restore \
  -- operator-stop

set +e
dotnet run \
  --project src/MotionControl.OperatorConsole \
  --configuration Release \
  --no-restore \
  -- probe-timeout
fault_exit=$?
set -e

if [[ "$fault_exit" -eq 0 ]]; then
  echo "Expected probe-timeout scenario to return non-zero."
  exit 1
fi

echo "All repository checks passed."
