# Exit immediately on any error (PowerShell equivalent of set -e)
$ErrorActionPreference = 'Stop'

# Capture the current branch
$currentBranch = git branch --show-current

if ($currentBranch -eq 'main') {
    Write-Host "Already on main. Skipping checkout and branch deletion."
    git pull
    git fetch --prune
    exit 0
}

$branchToDelete = $currentBranch

git checkout main
git pull

# Force-delete required: squash-merges leave the local branch looking
# unmerged to git even though the work is already on main.
git branch -D $branchToDelete

git fetch --prune

Write-Host "Done. Cleaned up branch: $branchToDelete"