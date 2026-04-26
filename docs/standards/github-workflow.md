# GitHub Workflow

## The Local Workflow

- You create an issue in GitHub (via the web UI or `gh issue create`)
- You tell Claude Code: "Let's work on issue #12"
- Claude Code checks out the main branch and pulls down the latest commits.
- Claude Code creates a branch named `issue-<number>-short-description` **before any Edit or Write tool call** (see [Before editing any file](#before-editing-any-file)).
- Claude Code reads the issue with `gh issue view 12`, understands the task, and starts working
- As work progresses, it (or you) can post progress updates as issue comments with `gh issue comment`
- When done, Claude Code pushes the commits to the remote repo and then Claude Code opens a PR linked to the issue with `gh pr create`
- You review, merge, and the issue closes automatically
- Once the PR is merged, you say **"cleanup"** and Claude Code runs the [Post-merge cleanup](#post-merge-cleanup) sequence.

---

## Post-merge cleanup

After a PR has merged on GitHub, the local repo is still sitting on the task branch. Use the trigger phrase **"cleanup"** (variants like "clean up" or "cleanup #12" are accepted) to run this sequence:

1. Run `git branch --show-current` to identify the current branch. If it is already `main`, note that and skip to step 4.
2. `git checkout main`
3. `git pull`
4. `git branch -D <previous-branch>` — force-delete is required because squash-merges leave the local branch looking unmerged to git even though the work is on `main`.
5. `git fetch --prune` to drop stale remote-tracking refs.
6. Report the final state: branch deleted, now on `main` at the latest commit.

Example sequence, for a task branch named `issue-12-add-password-reset`:

```bash
git branch --show-current          # -> issue-12-add-password-reset
git checkout main
git pull
git branch -D issue-12-add-password-reset
git fetch --prune
```

---

## GitHub CLI Usage

- Always use the GitHub CLI (`gh`) to interact with GitHub — never GitHub Actions
- When starting work on an issue, run `gh issue view <number>` to read the full details before doing anything
- Create a branch named `issue-<number>-short-description` **before any Edit or Write tool call** (see [Before editing any file](#before-editing-any-file))
- Post a comment on the issue when starting work: "Starting work on this issue"
- Post progress comments on the issue as significant steps are completed
- Check off acceptance criteria checkboxes in the issue as each one is completed using `gh issue edit`
- When work is complete, push the commits to the remote repo and open a PR using `gh pr create` with "Closes #<number>" in the body
- Never commit directly to main, and never edit files while checked out on `main`

---

## Useful `gh` and `git` Commands Reference

```bash
# GitHub
gh issue list                              # See all open issues
gh issue create                            # Create a new issue interactively
gh issue view 12                           # Read issue #12
gh issue comment 12 --body "..."           # Add a comment to issue #12
gh issue close 12                          # Close issue #12
gh pr create --title "..." --body "Closes #12"  # Open a PR linked to an issue

# Branch pre-flight (run before any Edit/Write)
git branch --show-current                  # Confirm current branch
git checkout main                          # Switch to main
git pull                                   # Pull latest commits
git checkout -b issue-12-short-description # Create the task branch

# Post-merge cleanup
git checkout main
git pull
git branch -D issue-12-short-description   # Force-delete local branch
git fetch --prune                          # Drop stale remote-tracking refs
```

---

## Issue Format

Issues should be written with enough detail for Claude Code to act on them without ambiguity.
Use the following structure when creating issues:

```markdown
**Title:** Short, action-oriented description

**What to do:**
A clear description of the feature, bug fix, or task.

**Acceptance criteria:**
- [ ] Specific, testable condition 1
- [ ] Specific, testable condition 2
- [ ] Specific, testable condition 3

**Notes:**
- Reference any relevant files, utilities, or patterns to follow
- Call out anything that should NOT be changed
```
