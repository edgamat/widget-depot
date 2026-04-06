# CLAUDE.md

This file contains standing instructions for Claude Code when working in this repository.

---

## Plan Mode

Do not explore the codebase during planning unless the task requires it or the user explicitly asks. If the user's request is self-contained (e.g., breaking down a story, writing a plan from a spec), go straight to planning.

---

## The Local Workflow:

- You create an issue in GitHub (via the web UI or `gh issue create`)
- You tell Claude Code: "Let's work on issue #12"
- Claude Code reads the issue with `gh issue view 12`, understands the task, and starts working
- As work progresses, it (or you) can post progress updates as issue comments with `gh issue comment`
- When done, Claude Code opens a PR linked to the issue with `gh pr create`
- You review, merge, and the issue closes automatically

## GitHub CLI Usage

- Always use the GitHub CLI (`gh`) to interact with GitHub — never GitHub Actions
- When starting work on an issue, run `gh issue view <number>` to read the full details before doing anything
- Create a branch named `issue-<number>-short-description` before making any changes
- Post a comment on the issue when starting work: "Starting work on this issue"
- Post progress comments on the issue as significant steps are completed
- Check off acceptance criteria checkboxes in the issue as each one is completed using `gh issue edit`
- When work is complete, open a PR using `gh pr create` with "Closes #<number>" in the body
- Never commit directly to main

---

## Useful `gh` Commands Reference

```bash
gh issue list                              # See all open issues
gh issue create                            # Create a new issue interactively
gh issue view 12                           # Read issue #12
gh issue comment 12 --body "..."           # Add a comment to issue #12
gh issue close 12                          # Close issue #12
gh pr create --title "..." --body "Closes #12"  # Open a PR linked to an issue
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

---

## General Coding Conventions

> refer to the [architecture](./docs/standards/architecture.md) for the solution structure and technologies to use.
