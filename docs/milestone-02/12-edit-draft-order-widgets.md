# Edit draft order: widget selection

## User Story

> As an **authenticated customer**,
> I need to edit the list of widgets on a draft order I previously started,
> in order to adjust my selection before continuing to submit the order.

## Background

When a customer resumes a draft order (story 04), they always start at step 1 of the wizard.
This story covers that entry point: loading the existing widget selection from the draft and
allowing the customer to make changes before proceeding through the remaining wizard steps.
Previously saved addresses on the draft are preserved when widgets are updated.

**Dependency:** Requires story 01 (Order entity and `OrderItem` data model), story 04 (draft
orders list with "Resume" action), and the step 2 page from story 02 to accept an existing
`orderId`.

## Scope

**In scope:**
- A version of step 1 that accepts an existing `orderId` and pre-populates the widget list from the saved `OrderItem` rows
- Entry point: the "Resume" action in the draft orders list (`/orders/{orderId}/step1`)
- The customer can add widgets, remove widgets, and adjust quantities
- A "Continue" button that replaces the existing `OrderItem` rows and advances to step 2
- Previously saved addresses on the draft are preserved (not cleared when widget selection changes)

**Out of scope:**
- Starting a new order (story 01)
- Editing addresses or shipping estimate directly (steps 2 and 3)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Edit/Step1/` (or reuse the story 01 step 1 component with an `orderId` parameter — developer's choice)
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/UpdateDraftItems/` — deletes existing `OrderItem` rows for the order and inserts the new set
- On load, fetch the current `OrderItem` rows for the draft to populate the widget selection UI
- "Continue" calls `UpdateDraftItems`, then navigates to step 2 with the `orderId`
- Only orders in `Draft` status owned by the authenticated customer may be edited; return a typed error otherwise
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [x] Step 1 (edit mode) is only reachable for orders in `Draft` status belonging to the authenticated customer; any other attempt returns an appropriate error
- [ ] Previously selected widgets and quantities are pre-populated when the customer arrives at step 1 via "Resume"
- [ ] The customer can add a widget with a quantity, adjust quantity, and remove a widget from the order
- [ ] A summary of selected widgets and their quantities is visible at all times during this step
- [ ] The "Continue" button is disabled (or shows a validation message) if no widgets are selected
- [ ] Clicking "Continue" with at least one widget replaces the order's items with the updated selection and advances to step 2
- [ ] Previously saved addresses on the draft are still present when the customer reaches step 2
- [ ] Unit tests are written where appropriate
