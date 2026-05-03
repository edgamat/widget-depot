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

## Step 2 - Create each story issue (one at a time)

For each story file, in filename order:

### 2a. Extract title and body

- Drop the YAML frontmatter (everything from the first `---` line through the
  matching closing `---` line, plus the blank line that follows).
- The next `# H1 line` is the issue **title**. Drop that line from the body.
- The remainder is the issue **body**, byte-for-byte. Do not reformat,
  paraphrase, summarize, or "improve" anything.
- Normalize line endings to `\n` (LF). Save the body to a temp file for the
  next step. This is the canonical body that will be compared against
  GitHub's response.

### 2b. Create the issue

```
gh issue create \
  --title "{title from H1}" \
  --body-file <tempfile> \
  --label story
```

Capture the new issue's number from the output.

### 2c. Fetch back and verify (drift check)

```
gh issue view {number} --json body --jq .body
```

Normalize line endings on the response to `\n`, then compare to the canonical
body byte-for-byte.

- **Mismatch:** stop the entire skill. Print a unified diff between the
  source body and the returned body. Do **not** continue with subsequent
  stories. Do **not** edit the GitHub issue to "fix" the difference. Do
  **not** edit the source markdown. Tell the user the issue number that
  failed verification and stop.
- **Match:** continue.

### 2d. Link as a sub-issue of the milestone parent

Fetch the new issue's GraphQL node ID:

```
gh issue view {number} --json id --jq .id
```

Save as `CHILD_NODE_ID`. Then run the sub-issue mutation:

```
gh api graphql \
  -f query='mutation($issueId: ID!, $subIssueId: ID!) { addSubIssue(input: {issueId: $issueId, subIssueId: $subIssueId}) { issue { number } subIssue { number } } }' \
  -f issueId="$PARENT_NODE_ID" \
  -f subIssueId="$CHILD_NODE_ID"
```

This is the **only** supported way to create native sub-issues in `gh`
2.92.x - there is no first-class flag. Do not improvise an alternative.

### 2e. Write the issue number back to the story file

Update the story's frontmatter:

- `status: shared`
- `github_issue: {number}`

Leave `task_issues: []` alone - that's `/refine-story`'s job.

Edit the file in place. Preserve the rest of the document exactly.

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
