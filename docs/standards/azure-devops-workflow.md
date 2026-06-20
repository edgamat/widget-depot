# Azure DevOps Workflow

This is the Azure DevOps equivalent of [github-workflow.md](./github-workflow.md). It maps the same
local workflow onto Azure DevOps Boards (work items) and Azure Repos (pull requests). The
tool-agnostic parts — Definition of Done, the pre-commit checks, and the commit-footer rule — are
carried over unchanged.

We drive Azure DevOps through its **REST API** using `curl`, authenticated with a Personal Access
Token (PAT). Three environment variables are assumed to be set for every example below:

```bash
export ADO_ORG="your-organization"      # e.g. the org segment in https://dev.azure.com/<org>
export ADO_PROJECT="your-project"       # the Azure DevOps project name
export ADO_PAT="<your personal access token>"   # NEVER commit this; treat it like a password
```

The PAT is sent via HTTP Basic auth with an empty username. The helper used throughout is:

```bash
# Authorization header value: Basic base64(":$ADO_PAT")
AUTH="Authorization: Basic $(printf ':%s' "$ADO_PAT" | base64)"
```

> **IMPORTANT** Never hardcode the PAT in commands, scripts, or commit it to the repo. Always read it
> from `$ADO_PAT`. Never read or print the contents of a `.env` file.

---

## Concept Mapping (GitHub → Azure DevOps)

| GitHub | Azure DevOps |
| --- | --- |
| Issue | Work item (**User Story**, Agile process) |
| `gh issue create` | `POST .../wit/workitems/$User%20Story` (content-type `application/json-patch+json`) |
| `gh issue view 12` | `GET .../wit/workitems/12` |
| `gh issue comment 12` | `POST .../wit/workItems/12/comments` |
| `gh pr create` | `POST .../git/repositories/{repo}/pullrequests` |
| `Closes #12` (auto-close) | `AB#12` mention in the PR title/description (links the work item) |
| `gh repo view --json nameWithOwner` | `$ADO_ORG` / `$ADO_PROJECT` env vars (no lookup needed) |
| Acceptance-criteria checkboxes in the issue body | Checklist in the **Acceptance Criteria** field (`Microsoft.VSTS.Common.AcceptanceCriteria`), updated via JSON-patch |

Base URL used throughout:

```
https://dev.azure.com/$ADO_ORG/$ADO_PROJECT/_apis/...
```

---

## The Local Workflow

- You create a **User Story** work item in Azure DevOps (via the web UI or the REST API).
- You tell Claude Code: "Let's work on work item 12".
- Claude Code checks out the `main` branch and pulls down the latest commits.
- Claude Code creates a branch named `users/<name>/<id>-short-description` (e.g.
  `users/edgamat/12-add-password-reset`) **before any Edit or Write tool call**.
- Claude Code reads the work item with a REST `GET` on `.../wit/workitems/12`, understands the task,
  and starts working.
- Implementing the work item includes writing Playwright E2E tests that cover the acceptance criteria,
  following the `playwright-add-tests` skill (see [Definition of Done](#definition-of-done)).
- As work progresses, it (or you) can post progress updates as work-item comments with a REST `POST`
  on `.../wit/workItems/12/comments`.
- When done, Claude Code pushes the commits to the remote repo and then opens a PR with a REST `POST`
  on `.../git/repositories/{repo}/pullrequests`, including `AB#12` in the title/description so the
  work item is linked.
- You review, complete the PR, and the work item resolves.
- Once the PR is completed, you say **"run post merge cleanup"** and Claude Code runs the
  [Post-merge cleanup](#post-merge-cleanup) sequence.

---

## Definition of Done

A work item is not complete until all of the following are true:

- The acceptance criteria are implemented.
- Unit tests cover the new/changed behavior (see [testing](./testing.md)).
- **Playwright E2E tests cover the work item's acceptance criteria.** For each acceptance criterion
  that is observable in the UI, there is a corresponding end-to-end test (or an assertion within one)
  under `tests/WidgetDepot.E2E`. When writing these tests you MUST follow the `playwright-add-tests`
  skill — do not write free-form specs.
- All checks in [Before Creating a Commit](#before-creating-a-commit) pass.
- The acceptance-criteria checklist in the work item's **Acceptance Criteria** field is ticked off.

If a criterion genuinely cannot be exercised end-to-end (e.g. a pure backend/data concern with no UI
surface), note why in a work-item comment instead of silently skipping it.

---

## Before Creating a Commit

Run all of the following before any `git commit`, in this order:

1. `dotnet format --exclude src/WidgetDepot.ApiService/Data/Migrations` — format C# code first, so any file changes are picked up by the build
2. `dotnet build` — compile the solution once
3. `dotnet test --no-build` — run the unit test suite, skipping recompilation
4. `CI=true npm test --prefix tests/WidgetDepot.E2E` — run all end-to-end tests in headless mode (`CI=true` prevents Playwright from auto-opening the HTML report)

Do not commit if any step fails. Fix the failure first, then re-run the full sequence.

---

## Post-merge cleanup

After a PR has completed in Azure DevOps, the local repo is still sitting on the task branch. Use the
trigger phrase **"cleanup"** (variants like "clean up" or "cleanup #12" are accepted) to run this
sequence:

1. Run `git branch --show-current` to identify the current branch. If it is already `main`, note that and skip to step 4.
2. `git checkout main`
3. `git pull`
4. `git branch -D <previous-branch>` — force-delete is required because squash-completion leaves the local branch looking unmerged to git even though the work is on `main`.
5. `git fetch --prune` to drop stale remote-tracking refs.
6. Report the final state: branch deleted, now on `main` at the latest commit.

Example sequence, for a task branch named `users/edgamat/12-add-password-reset`:

```bash
git branch --show-current          # -> users/edgamat/12-add-password-reset
git checkout main
git pull
git branch -D users/edgamat/12-add-password-reset
git fetch --prune
```

---

## Azure DevOps REST Usage

- Always interact with Azure DevOps through the REST API using `$ADO_PAT` — never hardcode the token, and never use a CI/pipeline trigger in place of the documented local steps.
- Always use the `$ADO_ORG` and `$ADO_PROJECT` environment variables to build URLs — never parse the org/project from `git remote get-url origin`.
- When starting work on a work item, post a comment: "Starting work on this work item".
- When starting work on a work item, `GET` the full work item details before doing anything.
- Create a branch named `users/<name>/<id>-short-description` **before any Edit or Write tool call**.
- Post progress comments on the work item as significant steps are completed.
- Check off acceptance-criteria items in the work item's **Acceptance Criteria** field as each one is completed, using a JSON-patch `PATCH`.
- When work is complete, push the commits to the remote repo and open a PR with `AB#12` in the title/description so the work item is linked.
- Never commit directly to `main`, and never edit files while checked out on `main`.
- Never combine `git commit` with other commands. Always run `git commit` as a separate command.
- Never add a 'Co-authored-by' footer in commit messages. Instead use the following:

AI-Assisted-By: <Model Name>

---

## Useful REST and `git` Commands Reference

All `curl` snippets assume `$ADO_ORG`, `$ADO_PROJECT`, and `$ADO_PAT` are set and that `$AUTH` holds
the Basic auth header (see top of this document):

```bash
AUTH="Authorization: Basic $(printf ':%s' "$ADO_PAT" | base64)"
BASE="https://dev.azure.com/$ADO_ORG/$ADO_PROJECT/_apis"
```

```bash
# --- Work items (Azure Boards) ---

# List open User Stories via WIQL
curl -s -H "$AUTH" -H "Content-Type: application/json" \
  "$BASE/wit/wiql?api-version=7.1" \
  -d '{"query":"SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.WorkItemType] = '\''User Story'\'' AND [System.State] <> '\''Closed'\'' ORDER BY [System.ChangedDate] DESC"}'

# Read work item #12
curl -s -H "$AUTH" \
  "$BASE/wit/workitems/12?api-version=7.1"

# Create a User Story (json-patch). $User%20Story is the URL-encoded work item type.
curl -s -H "$AUTH" -H "Content-Type: application/json-patch+json" -X POST \
  "$BASE/wit/workitems/\$User%20Story?api-version=7.1" \
  -d '[
        {"op":"add","path":"/fields/System.Title","value":"Short, action-oriented title"},
        {"op":"add","path":"/fields/System.Description","value":"<p>What to do…</p>"},
        {"op":"add","path":"/fields/Microsoft.VSTS.Common.AcceptanceCriteria","value":"<ul><li>[ ] Criterion 1</li><li>[ ] Criterion 2</li></ul>"}
      ]'

# Add a comment to work item #12
curl -s -H "$AUTH" -H "Content-Type: application/json" -X POST \
  "$BASE/wit/workItems/12/comments?api-version=7.1-preview.3" \
  -d '{"text":"Starting work on this work item"}'

# Update state and acceptance criteria on work item #12 (json-patch)
curl -s -H "$AUTH" -H "Content-Type: application/json-patch+json" -X PATCH \
  "$BASE/wit/workitems/12?api-version=7.1" \
  -d '[
        {"op":"add","path":"/fields/System.State","value":"Resolved"},
        {"op":"add","path":"/fields/Microsoft.VSTS.Common.AcceptanceCriteria","value":"<ul><li>[x] Criterion 1</li><li>[x] Criterion 2</li></ul>"}
      ]'

# --- Pull requests (Azure Repos) ---
# Replace {repo} with the repository name or id. Include AB#12 to link the work item.
curl -s -H "$AUTH" -H "Content-Type: application/json" -X POST \
  "$BASE/git/repositories/{repo}/pullrequests?api-version=7.1" \
  -d '{
        "sourceRefName":"refs/heads/users/edgamat/12-add-password-reset",
        "targetRefName":"refs/heads/main",
        "title":"Add password reset (AB#12)",
        "description":"Implements the password reset story. AB#12"
      }'
```

```bash
# Branch pre-flight (run before any Edit/Write)
git branch --show-current                       # Confirm current branch
git checkout main                               # Switch to main
git pull                                        # Pull latest commits
git checkout -b users/edgamat/12-add-password-reset   # Create the task branch

# Post-merge cleanup
git checkout main
git pull
git branch -D users/edgamat/12-add-password-reset     # Force-delete local branch
git fetch --prune                               # Drop stale remote-tracking refs
```

---

## Work Item Format

Work items should be written with enough detail for Claude Code to act on them without ambiguity.
Use the following structure when creating a User Story. The acceptance criteria live in the dedicated
**Acceptance Criteria** field; everything else goes in the **Description**:

```markdown
**Title:** Short, action-oriented description

**What to do:** (Description field)
A clear description of the feature, bug fix, or task.

**Acceptance criteria:** (Acceptance Criteria field, as a checklist)
- [ ] Specific, testable condition 1
- [ ] Specific, testable condition 2
- [ ] Specific, testable condition 3

**Notes:** (Description field)
- Reference any relevant files, utilities, or patterns to follow
- Call out anything that should NOT be changed
```
