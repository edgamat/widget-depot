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

When breaking down stories into tasks, assume the story as well as the associated tasks all should be added to GitHub as issues. Link the tasks to the story by referring to them in the issue created for the story.

When breaking down stories into tasks, the coding tasks should include unit tests, rather than having separate unit tests tasks for the story. Each issue created for a task should include an acceptance criterion indicating that unit tests should be written where appropriate.

---

## Local Workflow

> refer to the [github-workflow](./docs/standards/github-workflow.md) for how to interact with the GitHub CLI when asked to work on issues.

---

## General Coding Conventions

Refer to the [architecture](./docs/standards/architecture.md) for the solution structure and technologies to use.

When writing C# classes, you MUST read and follow [code-design](./docs/standards/code-design.md) before writing any code.

When writing C# unit tests, you MUST read and follow [testing](./docs/standards/testing.md) before writing any code.
