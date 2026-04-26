# CLAUDE.md

This is a public-facing widget ordering application using ASP.NET Blazor 10.0 and Postgres.

---

## Key Commands

- `dotnet build` for building the application
- `dotnet test` for running the test suite
- `aspire run` for running the application

---

## Plan Mode

Do not explore the codebase during planning unless the task requires it or the user explicitly asks. If the user's request is self-contained (e.g., breaking down a story, writing a plan from a spec), go straight to planning.

---

## Agile Process

TBD 

---

## Before editing any file

Branch creation is a **precondition**, not a step. Before the first Edit or Write tool call in any task, all of the following must be true:

1. Run `git branch --show-current` to confirm the current branch.
2. If the current branch is `main`, **stop** and create a new branch first:
   - `git checkout main`
   - `git pull`
   - `git checkout -b <branch-name>`
3. Only then proceed with edits.

**Never run Edit or Write tools while checked out on `main` or on a stale task branch.** If you notice mid-task that you are on the wrong branch, stop and switch before making further changes.

---


## Local Workflow

> refer to the [github-workflow](./docs/standards/github-workflow.md) for how to interact with the GitHub CLI when asked to work on issues.

---

## General Coding Conventions

Refer to the [architecture](./docs/standards/architecture.md) for the solution structure and technologies to use.

When writing C# classes, you MUST read and follow [code-design](./docs/standards/code-design.md) before writing any code.

When writing C# unit tests, you MUST read and follow [testing](./docs/standards/testing.md) before writing any code.
