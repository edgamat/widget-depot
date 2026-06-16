---
status: draft
milestone: 5
github_issue:
task_issues: []
---

# Report a Problem Link

## User Story

> As a **customer**,
> I need a direct link from my order detail to the problem report wizard,
> in order to report an issue without having to manually copy the order number.

## Background

Without this link, a customer must navigate to the problem report wizard separately and type in their order number. Adding a contextual link on the order detail page removes that friction and makes problem reporting more discoverable.

## Scope

**In scope:**
- "Report a Problem" link on the order detail page for submitted orders
- The link navigates to the problem report wizard with the order number pre-filled

**Out of scope:**
- The link on non-submitted orders (draft, expired, etc.)
- The link on the "My Recent Orders" list page
- Any changes to the wizard's validation logic (story 1 handles invalid order numbers)

## Developer Notes

- The order number should be passed as a URL parameter to the wizard; the wizard already validates ownership and status.
- The link only renders when the order is in a submitted state.

## Acceptance Criteria

- [ ] The order detail page for a submitted order displays a "Report a Problem" link
- [ ] Clicking the link navigates to the problem report wizard with the order number pre-filled
- [ ] The link does not appear on non-submitted orders

## Refinement Notes

Link placement is order detail only — the "My Recent Orders" list was considered and deferred. Pre-filling via URL parameter keeps the wizard self-contained; story 1's validation remains the authoritative check on order eligibility.
