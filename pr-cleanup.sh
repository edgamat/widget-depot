#!/usr/bin/env bash
set -euo pipefail

# Capture the current branch
current_branch=$(git branch --show-current)

if [[ "$current_branch" == "main" ]]; then
    echo "Already on main. Skipping checkout and branch deletion."
    git pull
    git fetch --prune
    exit 0
fi

# Store the branch name before switching away
branch_to_delete="$current_branch"

git checkout main
git pull

# Force-delete required: squash-merges leave the local branch looking
# unmerged to git even though the work is already on main.
git branch -D "$branch_to_delete"

git fetch --prune

echo "Done. Cleaned up branch: $branch_to_delete"