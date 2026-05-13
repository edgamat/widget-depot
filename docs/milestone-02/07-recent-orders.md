# Recent submitted orders

## User Story

> As an **authenticated customer**,
> I need to see my most recently submitted orders in one place,
> in order to track what I've ordered without having to remember order numbers.

## Background

Once customers begin submitting orders through the new system they will want a quick way to
review what they've ordered. This story adds a view of the customer's most recent submitted
orders — up to 10, newest first. Draft orders are not shown here; they remain accessible via
the draft orders list (story 04).

## Scope

**In scope:**
- An authenticated page at `/orders/history` showing the customer's submitted orders
- Up to 10 orders displayed, sorted by submission date descending
- Each row displays: order ID, submission date, number of widgets, estimated shipping cost
- Each row links to the order detail view (story 08)
- An empty state message when the customer has no submitted orders

**Out of scope:**
- Pagination or "load more" beyond the 10-order cap (deferred)
- Filtering or sorting controls
- Draft orders (covered by story 04)
- Admin order lookup (Milestone 4)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/History/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/GetRecentSubmitted/`
- Query: submitted orders for the authenticated customer, ordered by `SubmittedAt` descending, limited to 10 rows
- `SubmittedAt` timestamp should be added to the `Order` entity (nullable; set when status transitions to `Submitted`) — add an EF Core migration if story 05 has not already added this column
- Add a navigation link to access the new page (My Recent Orders).
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] An unauthenticated visitor attempting to navigate to `/orders/history` is redirected to login
- [ ] A customer with no submitted orders sees an empty state message
- [ ] A customer with submitted orders sees up to 10 of them, sorted newest first
- [ ] Each row displays: order ID, submission date, number of widgets, and estimated shipping cost
- [ ] Each row is clickable and navigates to the order detail view (story 08)
- [ ] If the customer has more than 10 submitted orders, only the 10 most recent are shown
- [ ] Draft orders are not shown on this page
- [ ] Unit tests are written where appropriate
