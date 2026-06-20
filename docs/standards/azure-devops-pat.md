# Azure DevOps Personal Access Token (PAT) Setup

The [Azure DevOps workflow](./azure-devops-workflow.md) drives Azure Boards and Azure Repos through
the REST API, authenticated with a **Personal Access Token (PAT)**. This document explains how to
create that PAT, which scopes it needs, and how to make it available to the workflow.

> **IMPORTANT** A PAT is a credential equivalent to your password. Never commit it, paste it into
> chat, hardcode it in a script, or store it in a file that is tracked by git. The workflow reads it
> only from the `$ADO_PAT` environment variable.

---

## Why a PAT is needed

The workflow's `curl` calls authenticate with HTTP Basic auth, sending the PAT as the password (with
an empty username). Without a valid PAT, every REST call (reading work items, posting comments,
creating pull requests) returns `401 Unauthorized` or `203 Non-Authoritative Information` (the sign-in
page HTML).

---

## Creating the PAT

1. Sign in to your organization at `https://dev.azure.com/<your-organization>`.
2. Click the **User settings** icon (top right, next to your avatar) → **Personal access tokens**.
   (Direct link: `https://dev.azure.com/<your-organization>/_usersSettings/tokens`.)
3. Click **+ New Token**.
4. Fill in the token details:
   - **Name** — something identifiable, e.g. `widget-depot-claude-code`.
   - **Organization** — select the organization the project lives in (a PAT is scoped to one org;
     create one per org if you work across several).
   - **Expiration** — choose the shortest practical lifetime. 30–90 days is a good default; you will
     rotate it (see [Rotating the PAT](#rotating-the-pat)).
5. Under **Scopes**, choose **Custom defined** (do **not** use "Full access") and select only the
   scopes listed below.
6. Click **Create**.
7. **Copy the token immediately** — Azure DevOps shows it only once. If you lose it, revoke it and
   create a new one.

---

## Required scopes

Select the minimum scopes the workflow actually uses. With **Custom defined** selected, tick:

| Scope (UI label) | Access | Why it's needed |
| --- | --- | --- |
| **Work Items** | Read & write | Read work items, post comments, update state, and tick off the Acceptance Criteria field. |
| **Code** | Read & write | Push the task branch and create pull requests in Azure Repos. |

That's it — these two cover every operation in the workflow. Notes:

- "Read & write" includes read, so you do **not** also need the read-only variants.
- You do **not** need Build, Release, Packaging, Test Management, or any other scope. Granting extra
  scopes only increases the blast radius if the token leaks.
- The underlying OAuth scope identifiers (for reference, not something you select directly) are
  `vso.work_write` (Work Items) and `vso.code_write` (Code).

---

## Making the PAT available to the workflow

The workflow expects the PAT (plus the org and project) in environment variables. Set them in your
shell session before running any of the workflow's REST commands:

```bash
export ADO_ORG="your-organization"
export ADO_PROJECT="your-project"
export ADO_PAT="<paste the token here>"
```

To avoid re-exporting every session, store them in an untracked file and source it — for example a
`.env` that is already covered by `.gitignore`:

```bash
# .env  (MUST be git-ignored — never commit this)
ADO_ORG=your-organization
ADO_PROJECT=your-project
ADO_PAT=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```

> Before relying on a `.env` file, confirm it is listed in `.gitignore`. Per the repo conventions,
> the contents of a `.env` file must never be read or printed back.

You can verify the token works with a read-only call (this lists projects in the org and should
return JSON, not an HTML sign-in page):

```bash
AUTH="Authorization: Basic $(printf ':%s' "$ADO_PAT" | base64)"
curl -s -o /dev/null -w "%{http_code}\n" -H "$AUTH" \
  "https://dev.azure.com/$ADO_ORG/_apis/projects?api-version=7.1"
# Expect: 200
```

---

## Rotating the PAT

- Recreate the token before it expires (Azure DevOps emails a reminder). Update `$ADO_PAT` with the
  new value; nothing else changes.
- If a token is ever exposed (committed, pasted, logged), **revoke it immediately** from the same
  Personal access tokens page, then create a replacement.

---

## Troubleshooting

| Symptom | Likely cause |
| --- | --- |
| `203` response or HTML sign-in page returned | Token missing, expired, or not sent — check `$ADO_PAT` is exported. |
| `401 Unauthorized` | Token valid but lacks the required scope, or is for the wrong organization. |
| `404 Not Found` on a work item / repo that exists | Wrong `$ADO_PROJECT`, or the project name needs URL-encoding (spaces → `%20`). |
| Works for reads, fails on create/update | Token has read-only scopes; recreate with **Read & write**. |
