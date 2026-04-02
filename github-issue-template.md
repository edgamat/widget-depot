# GitHub Issue Template

Use this structure for all new issues. It is written for two audiences:
- **Product Owner** — understands the business context and can validate scope
- **Claude Code** — has enough detail to implement without ambiguity

---

## Issue Title
Short, action-oriented. Example: *"Add email validation to the login form"*

---

## User Story

> As a **[role — e.g. registered user / admin / manager]**,
> I need **[capability or feature]**,
> in order to **[business goal or user benefit]**.

---

## Background

Provide context the product owner (and Claude Code) needs to understand *why* this work matters.
Include any relevant business rules, prior decisions, related issues, or links to designs/specs.

---

## Scope

A plain-language description of what this issue covers and — equally important — what it does **not** cover. This helps the product owner validate boundaries and prevents Claude Code from over-reaching.

**In scope:**
- ...

**Out of scope:**
- ...

---

## Developer Notes

Implementation guidance for Claude Code. Reference specific files, existing utilities, patterns to follow, or constraints to respect.

- Relevant file(s): `src/...`
- Patterns to follow: ...
- Constraints / things not to change: ...

---

## Acceptance Criteria

Written as testable conditions. The product owner uses these to validate the work; Claude Code checks them off as each one is completed.

- [ ] Condition 1
- [ ] Condition 2
- [ ] Condition 3

---
