---
name: address-pr
description: Fetch and address review feedback on a GitHub pull request. Use when the user asks to respond to PR comments, address review feedback, handle reviewer suggestions, or work through code review on an open PR.
---

# Address PR feedback

Fetch all review feedback on a pull request, present a plan, and after confirmation make the changes locally.

## Step 1 - Identify the PR

If the user provided a PR number in `$ARGUMENTS`, use it. Otherwise, run `gh pr status` to find the current branch's PR, or ask the user which PR.

## Step 2 - Gather feedback from all sources

GitHub stores conversation comments and inline review comments in different places. Fetch both:

- `gh pr view <number> --comments` - conversation comments
- `gh api repos/{owner}/{repo}/pulls/<number>/comments` - inline review comments tied to specific lines (these are NOT fully included in `pr view`)
- `gh pr view <number> --json reviews,headRefName,baseRefName` - review state and branch info

## Step 3 - Make sure the PR branch is checked out locally

If not on the PR branch, check it out before editing. Pull changes from the repo

## Step 4 - Present a plan, then stop

Group feedback by file. For each comment, give the user:
- File and line (if inline)
- The gist of the comment
- Proposed fix, OR a note that it's ambiguous, OR a note that it's worth pushing back on rather than implementing

Wait for the user to confirm the plan before making any edits. They will use the phrase "Proceed with plan".

## Step 5 - Make the changes

After confirmation:
- Edit the files
- Run all tests in the solution (`dotnet test`)
- Commit with a message referencing the feedback addressed (e.g. "Address review feedback on auth handler")
- Do NOT push until the user explicitly says to. They will use the phrase "Push feedback".
- Do NOT resolve review threads - leave that to the reviewer so they can verify
