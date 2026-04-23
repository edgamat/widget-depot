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
- A "Resume" action per row that always returns the customer to step 1 of the wizard
- A "Delete" action per row that removes the draft order after confirmation
- An empty state message when the customer has no draft orders

**Out of scope:**
- Listing submitted orders (not part of this milestone)
- Pagination (if needed it can be added later)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/List/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/GetDrafts/` (returns drafts for the authenticated customer)
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/DeleteDraft/`
- "Resume" always navigates to `/orders/{orderId}/step1`; the customer works through the wizard from step 1 each time they resume a draft
- Delete should be a soft confirmation (e.g., a modal or inline confirmation prompt) to prevent accidental removal
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] An unauthenticated visitor attempting to navigate to `/orders` is redirected to login
- [ ] An authenticated customer with no draft orders sees an empty state message
- [ ] An authenticated customer with draft orders sees each draft listed with: order ID, widget count, created date, and expiry date
- [ ] Clicking "Resume" on a draft navigates the customer to step 1 of the wizard, with previously selected widgets pre-populated
- [ ] Clicking "Delete" on a draft prompts the customer to confirm before deleting
- [ ] Confirming the delete removes the draft order and it no longer appears in the list
- [ ] Unit tests are written where appropriate
