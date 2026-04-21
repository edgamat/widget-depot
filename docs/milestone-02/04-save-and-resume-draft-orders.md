# Save and resume draft orders

## User Story

> As an **authenticated customer**,
> I need to view my saved draft orders and return to any of them to complete or delete them,
> in order to finish placing orders I started but didn't submit right away.

## Background

Orders are persisted as drafts from the moment a customer completes step 1 of the wizard
(story 01). This story adds the ability for a customer to see all their in-progress drafts
in one place, navigate back into the wizard to continue a draft, or remove a draft they no
longer want. Draft orders expire automatically after 30 days (story 06).

## Scope

**In scope:**
- An authenticated page at `/orders` listing the customer's draft orders
- Each row displays: order ID, number of widgets, created date, expiry date (created + 30 days)
- A "Continue" action per row that returns the customer to the correct wizard step (the first step that is incomplete)
- A "Delete" action per row that removes the draft order after confirmation
- An empty state message when the customer has no draft orders

**Out of scope:**
- Listing submitted orders (not part of this milestone)
- Editing widgets or addresses on a draft that has already progressed past those steps (deferred)
- Pagination (if needed it can be added later)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/List/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/GetDrafts/` (returns drafts for the authenticated customer)
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/DeleteDraft/`
- The "correct wizard step" for resuming is determined by which data is missing: no addresses → step 2; no shipping estimate → step 3; all complete → step 3 (re-review before submitting)
- Delete should be a soft confirmation (e.g., a modal or inline confirmation prompt) to prevent accidental removal
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] An unauthenticated visitor attempting to navigate to `/orders` is redirected to login
- [ ] An authenticated customer with no draft orders sees an empty state message
- [ ] An authenticated customer with draft orders sees each draft listed with: order ID, widget count, created date, and expiry date
- [ ] Clicking "Continue" on a draft navigates the customer back into the wizard at the appropriate incomplete step, with previously entered data still present
- [ ] Clicking "Delete" on a draft prompts the customer to confirm before deleting
- [ ] Confirming the delete removes the draft order and it no longer appears in the list
- [ ] Unit tests are written where appropriate
