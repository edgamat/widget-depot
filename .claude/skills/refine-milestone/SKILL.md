---
name: refine-milestone
description: Refine a milestone from PRD into a set of drafted user story markdown files. Use when the user wants to plan a milestone's stories, interview through scope and acceptance criteria, and draft story files under /docs/milestone-NN/.
---

# Refine a milestone

Take a milestone from `/docs/prd.md` and produce a set of refined user story
markdown files under `/docs/milestone-NN/`. Runs in three sequential phases:
**Plan**, **Interview**, **Draft**. Each phase has an explicit handoff back to
the user.

The user's milestone number is in `$ARGUMENTS` (e.g., `1`, `02`). Normalize it
to a two-digit zero-padded form (`01`, `02`, ...). The directory is
`/docs/milestone-NN/`. Story files inside it are named `SS-slug.md` where `SS`
is also zero-padded.

This skill is **thin orchestration**. The shape of a story lives in
`/docs/standards/user-stories.md` — read that and follow it; do not paraphrase
it here.

## Step 0 — Pre-flight checks

1. Read `/docs/prd.md` and locate the `### Milestone N - …` heading. If not
   found, stop and ask the user.
2. Read `/docs/standards/user-stories.md`. The story file format must match
   what that doc specifies.
3. Check if `/docs/milestone-NN/` already exists.
   - **Does not exist or is empty:** fresh run. Proceed.
   - **Has story files:** list them. Refuse to overwrite. Tell the user:
     "Existing story files found in `/docs/milestone-NN/`. v1 of this skill
     does not overwrite. A future `--update` flag will support re-running.
     For now, delete the files you want to redo and run again, or treat this
     run as additive — tell me to plan only stories not already on disk."
   - Wait for the user's choice before continuing.

## Step 1 — Plan phase

1. Re-read the PRD's milestone section. Re-read `/docs/business-requirements.md` if
   helpful for context. Don't read every other story file in the repo unless
   you have a reason to.
2. Propose a list of stories for the milestone. Aim for fewer than 12 — most
   milestones in this project sit comfortably under that. For each story:
   - **Title** (short, descriptive)
   - **Summary** (2–3 sentences — what it covers, where the boundary is)
   - **User value** (one sentence — why the user cares)
3. Order the stories so each builds on the previous one logically.
4. **Stop and present the plan.** Wait for the user's approval. The user may
   add, remove, edit, reorder, or merge stories. Iterate until they confirm.
5. Once approved, restate the final list back to the user as a numbered list
   with proposed slugs (e.g., `01-public-catalog-browsing`). Slugs become the
   filenames; get them right now.

**Do not write files in this phase.** The plan lives in conversation until
interviews start.

## Step 2 — Interview phase

For each approved story, in order, walk the checklist below. Move through
**one section at a time**. Don't dump all questions at once.

Order is fixed:

1. **Scope boundaries.** What is in. What is explicitly out. The "out" list
   matters more — it prevents scope creep at implementation time.
2. **Acceptance criteria.** Specific, testable, behavior-focused. Cover happy
   path, key error cases, and edge cases. Each criterion will become a
   checkbox in the story file. Refer to `/docs/standards/user-stories.md`
   for the criteria rules.
3. **Edge cases and error handling.** Empty inputs, validation failures,
   concurrent or duplicate actions, permission boundaries, anything else
   the user wants to call out.

### Interview rules

- **Be ruthless about cutting questions that don't surface anything useful.**
  If a section has nothing interesting to ask, summarize the obvious answer
  back to the user and move on. Don't make the interview a chore.
- **The user can skip ahead at any time.** If they say "move on", "skip",
  "next", or anything similar, end the current section (or the current
  story's interview entirely, if at the end) and proceed.
- **One question or short cluster at a time.** Not a wall of bullets.
- **Track decisions, not transcripts.** The Refinement Notes section in the
  story file captures the *why* behind decisions, not every back-and-forth.

### Write the story file as soon as its interview ends

After finishing the interview for a story, immediately write
`/docs/milestone-NN/SS-slug.md` (Step 3 below). This makes the work durable
across sessions — if the conversation ends, the completed stories are on
disk and the user can resume by re-running the skill.

## Step 3 — Draft phase (per story, immediately after its interview)

Write the story file using this structure:

```markdown
---
status: draft
milestone: N
github_issue:
task_issues: []
---

# {{Title}}

## User Story

> As a **{{role}}**,
> I need to {{behavior}},
> in order to {{benefit}}.

## Background

{{context the developer needs that isn't obvious from code}}

## Scope

**In scope:**
- {{...}}

**Out of scope:**
- {{...}}

## Developer Notes

- {{notes from the interview that guide implementation}}

## Acceptance Criteria

- [ ] {{...}}

## Refinement Notes

{{Key decisions from the interview — the *why*. Not a transcript.
Examples: "Chose ILIKE search over full-text because the catalog stays
small in this milestone." or "Pagination explicitly deferred to a later
milestone — confirmed with user."}}
```

Frontmatter rules:

- `status: draft` — author the file with `draft`. The user marks it `ready`
  when they're satisfied. `/share-milestone` (Phase C) will refuse to push
  any story that isn't `ready`.
- `milestone: N` — the integer milestone number, **not** zero-padded.
- `github_issue:` — leave empty. `/share-milestone` fills it in.
- `task_issues: []` — leave empty. `/refine-story` fills it in.

Match the section structure to `/docs/standards/user-stories.md` exactly.
If that doc and this template ever diverge, the standards doc wins.

## Step 4 — Final summary

After all approved stories are drafted, print:

- **What changed:** the list of files written (full paths).
- **Current state:** all stories at `status: draft`. The user must edit and
  flip them to `status: ready` when satisfied.
- **Suggested next step:** review each draft, mark them `ready`, then run
  `/share-milestone N` to push them to GitHub.

## Cross-cutting reminders

- **No files written before user approves the plan.** The plan phase is
  conversation only.
- **One story file per approved story.** Splitting into multiple GitHub
  issues happens later, in `/refine-story`. The markdown file always stays
  one user-facing story.
- **Refuse to overwrite.** If a story file at the target path already
  exists when it's time to write it, stop and surface the conflict; do
  not silently overwrite. (See Step 0 for the up-front check.)
- **Trust the standards docs for judgment.** Story shape lives in
  `/docs/standards/user-stories.md`. This skill orchestrates; it does not
  re-decide what a story should look like.
