---
name: refine-story
description: Decide whether a shared story should be split into task sub-issues before implementation. Walks the splitting checklist, proposes a split (or confirms no split), creates task issues under the story, and verifies acceptance-criteria coverage.
---

# Refine a story before implementation

Run this **after** a story has been shared to GitHub (Phase C) and **before**
implementation starts. The skill consults
`/docs/standards/issue-splitting.md`, walks the decision checklist with the
user, and either:

- confirms the story is fine as a single issue, or
- creates one task **sub-issue per split task** under the story issue, with
  each task owning a distinct subset of the story's acceptance criteria.

The story markdown file always stays as a single user-facing story. The split
lives only in GitHub.

`$ARGUMENTS` is `M S` — the milestone number and the story number, both
unpadded (e.g., `1 4`). Normalize to two-digit zero-padded form and find
`/docs/milestone-MM/SS-*.md` (a single matching file).

## Step 0 — Pre-flight checks

1. Parse `$ARGUMENTS` as `M S`. Normalize to `MM` and `SS`.
2. Glob `/docs/milestone-MM/SS-*.md`. There must be exactly one match. If
   zero or multiple, stop and report.
3. Parse the story's YAML frontmatter:
   - `status` must be `shared`. If `draft` or `ready`: stop, tell the user
     to run `/share-milestone M` first. If already `split`: stop and refuse
     (see "Refuse to overwrite" below).
   - `github_issue` must be set. If empty: same — story hasn't been shared.
   - `task_issues` must be `[]`. If non-empty:
     "This story already has task issues: [list]. v1 of this skill does
     not re-split. A future `--update` flag will support this. To redo,
     close those task issues, clear the `task_issues` list and reset
     `status: shared` manually, then re-run."
4. Read `/docs/standards/issue-splitting.md` — this is where the judgment
   lives.
5. Read the story file in full. Extract the **acceptance criteria** as an
   ordered list of `- [ ]` items. These are the units the coverage check
   will operate on.
6. Fetch the corresponding GitHub issue for context and the URL:
   ```
   gh issue view {github_issue} --json title,url,body --jq '.'
   ```
   Save the URL for the summary at the end. Do not re-do the drift check
   from Phase C — the source of truth for splitting is the markdown file.

## Step 1 — Walk the decision checklist

Walk the rules from `/docs/standards/issue-splitting.md` in order. For each
rule, state briefly:

- the rule (one line)
- whether it fires for this story
- the reason (one line)

Keep this to roughly 4–6 lines total — the user wants to see the reasoning,
not a thesis. Example shape:

```
1. Schema/migration change?      No  — story does not touch the database.
2. CI/CD pipeline change?        No  — no pipeline files in scope.
3. Backend + frontend slice?     Yes — endpoint + Blazor page are each
                                       independently verifiable.
4. >15 files?                    Likely — endpoint, query, page, layout,
                                          tests on both sides.
5. Default                       N/A — earlier rule fired.
```

## Step 2 — Propose a split (or no-split)

### If no rule fires → propose no split

State: "No split rule fires. Recommend keeping this as a single issue."
Skip to **Step 5** (no GitHub changes; print summary).

### If at least one rule fires → propose a split

For each proposed task, give:

- **Title** (short, imperative)
- **Description** (2–3 sentences — what this task covers and where its
  boundary is)
- **Acceptance criteria covered** — verbatim copies of the AC items from
  the story that this task owns

Then present the **AC coverage map**: every AC from the story, and which
task owns it. Every AC must appear under exactly one task. If any AC is
unassigned, the proposal is incomplete — fix it before showing the user.

Example coverage map:

```
AC 1: ✓ Backend
AC 2: ✓ Backend
AC 3: ✓ Frontend
AC 4: ✓ Frontend
AC 5: ✓ Frontend
```

Wait for user approval. The user may edit titles, descriptions, the AC
mapping, or reject the split entirely (in which case go to no-split).

## Step 3 — Coverage check (gate before creating any GitHub issues)

Programmatically verify that the union of task ACs equals the source story's
AC list:

- Each story AC appears in **exactly one** task's "Acceptance criteria
  covered" list.
- Each task has **at least one** AC.
- The text of each AC in a task matches its source verbatim (this is the
  drift guard for the split).

If the check fails: stop, list which ACs are missing or duplicated, do not
proceed. Do not auto-fix — surface it to the user.

## Step 4 — Create the task sub-issues

Only after the coverage check passes.

For each approved task, in the order the user approved them:

### 4a. Compose the task issue body

```markdown
> Sub-task of #{story_issue_number}

{description — 2–3 sentences from the proposal}

## Acceptance Criteria

{verbatim AC items from the story that this task owns, as `- [ ]` checkboxes}
```

The `> Sub-task of #N` line at the top makes the parent visible in the
GitHub UI even before the sub-issue link is rendered.

### 4b. Create the issue

```
gh issue create \
  --title "{task title}" \
  --body-file <tempfile> \
  --label task
```

Capture the new issue's number.

### 4c. Link as a sub-issue of the **story** issue

(Not the milestone — the story.)

Fetch the new task issue's GraphQL node ID:

```
gh issue view {task_number} --json id --jq .id
```

Fetch the story issue's GraphQL node ID once at the start of Step 4 and
reuse it:

```
gh issue view {story_issue_number} --json id --jq .id
```

Run the sub-issue mutation:

```
gh api graphql \
  -f query='mutation($issueId: ID!, $subIssueId: ID!) { addSubIssue(input: {issueId: $issueId, subIssueId: $subIssueId}) { issue { number } subIssue { number } } }' \
  -f issueId="$STORY_NODE_ID" \
  -f subIssueId="$TASK_NODE_ID"
```

This is the same mutation `/share-milestone` uses — see that skill for
notes. There is no first-class `gh` flag for sub-issues in 2.92.x.

## Step 5 — Update the story frontmatter

After all task issues are created and linked (or after a no-split
confirmation):

- **Split case:**
  - `status: split`
  - `task_issues: [N1, N2, ...]` — the new issue numbers in order
- **No-split case:** leave the file alone. `status` stays `shared`,
  `task_issues` stays `[]`. Print the no-split decision in the summary so
  the user has a record.

Edit the file in place. Preserve the rest of the document exactly.

## Step 6 — Final summary

Print:

- **Story:** title, story issue number, URL.
- **Decision:** "no split" or "split into N tasks".
- **Tasks (split case):** numbered list of `{number} - {title} - {URL}`.
- **AC coverage:** confirm every AC from the story is owned by exactly one
  task (split case) or by the story itself (no-split case).
- **Frontmatter updates:** the diff applied (split case).
- **Suggested next step:** "Open the first task issue and start
  implementation."

## Cross-cutting reminders

- **Source of truth is the markdown file.** If the GitHub issue body has
  drifted from the markdown since `/share-milestone` ran (someone edited
  it on GitHub), surface that as a warning but use the markdown for the
  splitting decision.
- **Never invent acceptance criteria.** Every AC quoted in a task issue
  must be a verbatim copy of an AC from the source story. The coverage
  check verifies this.
- **Refuse to overwrite.** If `task_issues` is already populated, stop.
  See Step 0 for the up-front check.
- **One sub-issue layer at a time.** Tasks are sub-issues of the **story**
  issue, not the milestone. The hierarchy is:
  `milestone parent → story → tasks`. Don't flatten this.
- **The `task` label is reused from the user-stories standard.** That doc
  defines `task` as "operational or non-code work" — split tasks here
  are code work. If the project decides this is a meaningful conflict,
  the standards doc should be updated and this skill should follow.
  Surface the conflict if the user hasn't seen it; don't decide
  unilaterally.
