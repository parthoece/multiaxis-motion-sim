#!/usr/bin/env bash
set -euo pipefail

repo_name="${1:-multiaxis-motion-sim}"

if ! command -v gh >/dev/null 2>&1; then
  echo "GitHub CLI is required."
  exit 1
fi

owner="$(gh api user --jq .login)"
python scripts/replace_owner.py "$owner"

git init
git branch -M main
git add .
git commit -m "Create simulation-first motion control platform"
gh repo create "$repo_name" --public --source=. --remote=origin --push

echo "Published https://github.com/$owner/$repo_name"
