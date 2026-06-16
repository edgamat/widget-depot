---
status: shared
milestone: 5
github_issue: 156
task_issues: []
---

# Problem Report: Order Lookup

## User Story

> As a **customer**,
> I need to look up one of my submitted orders by order number,
> in order to begin filing a problem report against it.

## Background

This is the first step of the problem reporting wizard. The customer enters an order number, the system validates it, and displays the line items from that order. No problem report record is created yet — item selection happens in the next story.

Only authenticated customers can access the wizard. The database contains only orders placed through the new system, so no special "old-system order" rejection logic is needed.

## Scope

**In scope:**
- Order number entry form (authenticated customers only)
- Validation that the order exists and belongs to the logged-in customer
- Validation that the order is in a submitted state
- Display of line items for the matched order (widget name, quantity ordered)

**Out of scope:**
- Item selection or issue type entry (story 2)
- Creation of any problem report record
- Any special rejection for "old-system" orders

## Developer Notes

- The wizard entry point should require authentication; redirect unauthenticated users to login.
- A submitted order will always have at least one line item — no need to handle the empty items case.

## Acceptance Criteria

- [ ] Unauthenticated users are redirected to login when attempting to access the problem report wizard
- [ ] An authenticated customer can enter an order number and submit the lookup form
- [ ] If the order number matches a submitted order belonging to the customer, the line items are displayed (widget name, quantity ordered)
- [ ] If the order number does not exist or belongs to a different customer, a clear error message is shown
- [ ] If the order exists but is not in a submitted state, an appropriate error message is shown
- [ ] Submitting an empty order number field shows a validation error

## Refinement Notes

The PRD restricts problem reports to orders placed through the new system. This is not a special validation rule — it is a natural consequence of the database only containing new-system orders. No rejection logic is required.
