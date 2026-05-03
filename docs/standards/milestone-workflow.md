# Milestone Workflow

A milestone moves from PRD to implementation through three skills. This doc is
a map: it shows how the pieces fit together. The prescriptive detail lives in
each skill file and the related standards docs.

## Pipeline

```
/docs/prd.md  →  /refine-milestone N  →  /docs/milestone-NN/*.md
                                                  ↓ (mark ready)
                                          /share-milestone N
                                                  ↓
                                          GitHub: milestone parent issue
                                                  + story sub-issues
                                                  ↓
                                          /refine-story N S  (per story)
                                                  ↓
                                          GitHub: task sub-issues
                                                  under the story
                                                  ↓
                                          implementation begins
```

## The three skills

| Skill | Input | Output | Standards consulted |
|---|---|---|---|
| [`/refine-milestone N`](../../.claude/skills/refine-milestone/SKILL.md) | `prd.md` milestone section | `/docs/milestone-NN/SS-slug.md` (status: draft) | `user-stories.md` |
| [`/share-milestone N`](../../.claude/skills/share-milestone/SKILL.md) | story files (status: ready) | GitHub milestone parent + story sub-issues; story files updated to status: shared | `user-stories.md` (milestone marker format) |
| [`/refine-story N S`](../../.claude/skills/refine-story/SKILL.md) | story file (status: shared) + GitHub story issue | GitHub task sub-issues under the story; story file updated to status: split (or unchanged on no-split) | `issue-splitting.md` |

## Frontmatter state machine

Every story file carries this frontmatter:

```yaml
---
status: draft | ready | shared | split
milestone: N            # integer, not zero-padded
github_issue:           # set by /share-milestone
task_issues: []         # set by /refine-story (split case)
---
```

Transitions:

- `/refine-milestone` writes the file with `status: draft`.
- The user manually flips it to `status: ready` when satisfied with the
  draft. This is the human checkpoint — no skill does it automatically.
- `/share-milestone` flips it to `status: shared` and fills in
  `github_issue`.
- `/refine-story` (split case) flips it to `status: split` and fills in
  `task_issues`. (No-split case leaves both alone.)

## Operating principles

These are the load-bearing decisions baked into the pipeline.

### 1. The markdown file is user-focused; GitHub holds the task split.

A story stays as one user-facing markdown file even when it's implemented as
multiple GitHub issues. The split lives only in the GitHub issue tree
(story → tasks). This keeps the docs readable as product narrative and lets
GitHub be the place where execution is broken down.

### 2. No drift between markdown and GitHub.

`/share-milestone` posts story bodies **verbatim**, then fetches the issue
back and byte-compares. On mismatch the skill stops; it does not "fix" the
difference. This is the contract that makes the markdown trustworthy as
source of truth.

### 3. Standards docs hold judgment; skills are thin orchestration.

When a skill has to make a judgment call — story shape, splitting rules,
milestone marker format — it defers to a standards doc:

- Story shape and acceptance-criteria rules: [`user-stories.md`](./user-stories.md)
- Splitting rules and worked examples: [`issue-splitting.md`](./issue-splitting.md)
- Milestone marker issue format: [`user-stories.md`](./user-stories.md) (Milestone Marker Issue Format)

If a skill is getting long and prescriptive, the prescription belongs in a
standards doc instead.

### 4. Refuse to overwrite in v1.

Each skill detects re-runs and stops rather than clobbering existing state.
A future `--update` flag will support re-running; until then, the user
unwinds state manually if they need to redo something.

## When something goes wrong

- **Plan or interview was bad** → delete the affected files in
  `/docs/milestone-NN/` and re-run `/refine-milestone N`. The skill detects
  partial state on Step 0.
- **Drift check failed in `/share-milestone`** → the skill aborted mid-run.
  Some story files are already `status: shared` with `github_issue` set,
  some aren't. Fix the source markdown (or the GitHub issue if it was
  edited there), reconcile the partially-updated frontmatter by hand, then
  re-run on what's left.
- **Coverage check failed in `/refine-story`** → the proposed split missed
  an acceptance criterion. Revise the proposal so every AC is owned by
  exactly one task, then re-run.
- **A story shouldn't have been split** → close the task issues on GitHub,
  clear `task_issues` and reset `status: shared` in the frontmatter.

## What's not in scope yet

- `--update` flags on any skill (re-share, re-split).
- Automatic resume mid-run after a failure.
- Closing the loop back from PR merges to story status (no `status: done`
  yet).

These are deliberate v1 omissions. The skills surface where the gap is so
the user can either drive around it manually or open a follow-up.
