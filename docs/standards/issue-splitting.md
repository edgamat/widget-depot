# Issue Splitting

## Purpose

This standard describes when a single user story should become more than one GitHub
issue, and how to draw the split. It is the document `/refine-story` consults when
deciding whether to split a story before implementation begins.

The story markdown file always remains a single user-facing story. The split lives
only in GitHub: the story issue stays as-is, and the implementation work is broken
into native sub-issues underneath it.

## Default

One story → one GitHub issue. Split only when the checklist below fires.

Splitting has a real cost: more PRs, more reviews, more coordination overhead. The
rules below identify the cases where that cost is paid back; everywhere else, keep
the story as one issue.

## Decision checklist

Walk these checks in order. The first matching rule decides the split.

### 1. Schema or data migration change? (hard rule, with one exception)

If the story requires a database schema change or a data migration, split the
migration into its own task issue.

**Why:** migrations need to be reviewed and merged ahead of dependent code so the
rest of the team can pull schema changes early. Bundling them with feature code
delays availability and mixes two very different review concerns.

**Exception:** a single-column or otherwise small additive schema change that is
only meaningful when paired with a small UI/API change in the same story. The
overhead of splitting outweighs the benefit. Keep together.

### 2. CI/CD pipeline change? (hard rule)

If the story requires changes to build, test, or deploy pipeline configuration,
split that into its own task issue.

**Why:** pipeline changes are operational and have a different blast radius than
application code. They need their own review.

(There is no CI/CD pipeline in this project yet. This rule is here for when one
is added.)

### 3. Substantive work on both backend and frontend? (heuristic)

If the story has a meaningful slice of work on each side of the API boundary —
where each side is independently verifiable — split at the API boundary into a
backend task and a frontend task.

"Substantive" means each side stands on its own:

- The backend task ends with an endpoint that can be exercised and verified
  against its acceptance criteria without the UI.
- The frontend task has enough logic, state, or UX behavior to be reviewed on its
  own merits, not just as the form that calls the new endpoint.

**Counter-signal:** if the frontend is a thin form whose only purpose is to call
one new endpoint and render the response, the two sides are not independent. Keep
them together.

### 4. More than ~15 files changed? (heuristic)

If the implementation is likely to touch more than ~15 files, look for a natural
split — usually the API boundary again, or a feature sub-slice (e.g., create flow
vs. edit flow).

This is a candidate, not a hard rule. Some stories are genuinely cohesive at this
size and should stay one issue.

### 5. Otherwise → single issue.

## Worked examples

### Worked example 1: split backend/frontend

**Story:** `milestone-01/01-public-catalog-browsing.md` — visitor browses the
widget catalog and runs a keyword search.

**Reasoning:** rule 3 fires. The backend has its own slice of work (Widget data
model, ILIKE search query, search endpoint, query tests). The frontend has its
own slice (Blazor catalog page, search box, result list, empty/no-result states).
Each side is independently verifiable.

**Proposed split:**

- *Backend task:* widget data access, search endpoint with name/description ILIKE
  query, endpoint tests.
- *Frontend task:* catalog page, search form, result rendering, empty-state
  message.

This story shipped as a single PR and was larger than it should have been. Rule 3
would have caught it.

### Worked example 2: schema migration carved out

**Story shape:** introducing a new entity (e.g., a new `Address` table for saved
shipping addresses) plus the API and UI to manage it.

**Reasoning:** rule 1 fires — a new table is a non-trivial schema change. The
team benefits from getting the migration reviewed and merged independently.

**Proposed split:**

- *Migration task:* EF migration for the new table, applied locally and in dev.
- *Application task:* endpoints, handlers, and pages that use the new table.

## Anti-examples

These stories *look* like the checklist should fire, but the right call is to
keep them as a single issue. Anti-examples matter more than positive examples —
the cost of an unnecessary split is real.

### Anti-example 1: catalog CSV import (`milestone-01/02-catalog-csv-import.md`)

This story was split backend/frontend in practice. In hindsight, that was wrong.

**Why rule 3 looks like it fires:** there is a backend endpoint (multipart
upload, CSV parsing, upsert) and a frontend page (admin upload form, summary
display).

**Why it shouldn't:** the frontend is a thin form whose only purpose is to post a
file to the one new endpoint and render the summary it returns. There is nothing
demoable on the frontend without the backend, and the backend has no other
consumer. The two sides are not independent slices — they are one slice with a
form on top. Splitting added integration overhead without producing two
independently valuable pieces of work.

**Verdict:** single issue.

### Anti-example 2: customer login and logout (`milestone-01/04-customer-login-logout.md`)

Login and logout *look* like two issues — separate pages, separate endpoints,
separate flows.

**Why no rule fires hard enough:** cookie authentication wiring, the login flow,
and the logout flow are interdependent for verification. You can't meaningfully
verify "login works" without a way to log out and try again, and the cookie
config has to land in both projects in one go.

**Verdict:** single issue.

### Anti-example 3: small column add + form tweak

**Story shape:** adding a single new field to an existing entity, with the
corresponding form and display changes (e.g., "show the customer's email in the
page header after sign-in," with a hypothetical schema tweak attached).

**Why rule 1 looks like it fires:** any schema change is on the hard-rule list.

**Why it shouldn't:** the exception in rule 1 applies. A single-column additive
change that is only meaningful when paired with the matching UI change in the
same story does not justify the splitting overhead. The migration is too small
to need its own review cycle, and the UI change is too small to demonstrate
without it.

**Verdict:** single issue.

## Coverage check

After proposing a split, verify that the union of the proposed task descriptions
addresses every acceptance criterion in the source story. If a criterion isn't
covered by any task, the split is incomplete: stop and flag it. Do not invent a
task to cover the gap silently — surface it to the user instead.

This check is what `/refine-story` runs before creating the task issues.
