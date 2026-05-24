---
name: share-milestone
description: Push a refined milestone's story files to GitHub as issues, linked as native sub-issues of a milestone parent issue. Use when a milestone's stories are marked ready and need to be created on GitHub.
---

# Share a milestone to GitHub

Mechanical pipeline that creates a milestone parent issue plus one GitHub issue
per story file in `/docs/milestone-NN/`, then links each story issue as a
**native sub-issue** of the parent.

The whole point of this skill is **drift prevention**. The body of each GitHub
issue must match the source markdown exactly. If it doesn't, the skill stops -
it does **not** "fix" the mismatch.

The user's milestone number is in `$ARGUMENTS` (e.g., `1`, `02`). Normalize to
two-digit zero-padded form (`01`, `02`, ...). The directory is
`/docs/milestone-NN/`.

## Step 0 - Pre-flight checks

1. List all `*.md` files in `/docs/milestone-NN/` (excluding any leading
   dotfiles). Sort by filename so processing order matches the `SS-` prefix.
2. For each story file, parse the YAML frontmatter and validate:
   - `status` must equal `ready`. If any file has `status: draft`, stop and
     list them: "These stories are still drafts. Edit them and set
     `status: ready`, then re-run." Do not proceed.
   - `github_issue` must be empty. If any file has it set, stop and list
     them: "These stories already have GitHub issues. v1 of this skill does
     not re-share. A future `--update` flag will support this. Remove the
     `github_issue:` field manually if you really want to re-share."
3. Read `/docs/standards/user-stories.md` so the milestone parent issue body
   follows the **Milestone Marker Issue Format** specified there.
4. Read the milestone's section in `/docs/prd.md` for the title and summary
   that go on the milestone parent issue.

If any check fails, stop. Do not partially share.

## Step 1 - Create the milestone parent issue

This is the parent that all story issues will hang under as sub-issues. Its
body uses the Review Checklist format from `/docs/standards/user-stories.md`.

1. Compose the title: `Milestone N - {milestone-title-from-prd}` (use the
   non-zero-padded number; matches the PRD's `### Milestone N - ...`
   heading).
2. Compose the body following the Milestone Marker format (Review Checklist +
   Smoke Test). Fill in the per-screen/per-flow lines from the milestone's
   stories.
3. Create the issue:
   ```
   gh issue create \
     --title "Milestone N - {title}" \
     --body-file <tempfile> \
     --label milestone
   ```
   Capture the URL/number from the output.
4. Fetch the milestone parent's GraphQL node ID - needed for sub-issue linking:
   ```
   gh issue view {parent_number} --json id --jq .id
   ```
   Save this as `PARENT_NODE_ID`.

## Step 2 - Run share-stories.sh

Run the helper script to process all story files:

```bash
bash .claude/skills/share-milestone/share-stories.sh \
  docs/milestone-NN \
  "$PARENT_NODE_ID"
```

Replace `milestone-NN` with the zero-padded milestone directory (e.g. `milestone-03`).

The script handles each story file in filename order:
- Extracts the H1 title and body (strips frontmatter)
- Creates the GitHub issue with `--label story`
- Drift-checks the returned body byte-for-byte against the source (trailing
  newline normalized to absorb GitHub's API artifact)
- Links the issue as a native sub-issue via the GraphQL `addSubIssue` mutation
- Flips the story file's frontmatter to `status: shared` with `github_issue: N`

If the script exits non-zero, it will have reported which issues were created
before the failure and which story files remain at `status: ready`. Do not
continue — relay the script's output to the user.

## Step 3 - Summary

After all stories are shared (or the skill aborted), print:

- **Milestone parent:** URL of the parent issue.
- **Story issues:** numbered list with title, number, and URL.
- **Frontmatter updates:** confirm each file flipped to `status: shared` with
  `github_issue` set.
- **Suggested next step:** when implementation begins, run
  `/refine-story N S` to decide whether each story should be split into task
  sub-issues before coding starts.

## Cross-cutting reminders

- **Verbatim posting.** The body that lands on GitHub must match the source
  markdown exactly (modulo line-ending normalization). No paraphrasing,
  summarizing, or "fixing typos." If you spot a typo while running this
  skill, stop - fix it in the markdown, then re-run from scratch.
- **Stop on mismatch.** Drift detection is the whole point. Make the
  failure loud: print the diff, name the failing issue, and exit. The user
  decides what to do next.
- **No partial state recovery in v1.** If the skill aborts mid-run, some
  story files will have updated frontmatter and some won't. Surface this
  clearly in the abort message: "Issues N through M were created and
  marked shared. Issue X failed the drift check. Story files Y, Z still
  have `status: ready` and no `github_issue` set." A future `--resume`
  flag will handle this; v1 leaves cleanup to the user.
- **Repo selection.** The `gh` commands above run against the current
  working directory's git repo. Do not pass `--repo` unless the user
  explicitly tells you to operate on a different repo (and if you do,
  follow the CLAUDE.md rule: fetch the repo name with
  `gh repo view --json nameWithOwner -q .nameWithOwner` first).
- **Authentication.** Assume `gh auth status` is already valid. If a
  command fails with an auth error, surface it and stop - do not attempt
  to re-auth on the user's behalf.
