# Order detail lookup

## User Story

> As an **authenticated customer**,
> I need to look up one of my orders by order number and see its full details,
> in order to review or reference a specific order at any time.

## Background

Customers may want to check the details of a specific order — whether it's a draft they're
still working on or a submitted order they want to review. This story provides a detail page
reachable by order number. Customers can only view their own orders; entering another
customer's order number returns a not-found response.

This page is also the destination for the "view detail" links in the recent orders list
(story 07).

## Scope

**In scope:**
- An authenticated page at `/orders/{orderNumber}` displaying full order details
- Details shown: order ID, status, created date, submission date (if submitted), all widgets with quantities and weight, shipping address, billing address, estimated shipping cost
- A search/lookup form at `/orders/lookup` where the customer can enter an order number and be redirected to the detail page
- If the order number is not found, or belongs to a different customer, a "not found" message is shown (no distinction between the two cases, to avoid leaking information)

**Out of scope:**
- Editing or re-submitting an order from this page
- Admin order lookup (Milestone 4, story 5.7 — staff can look up any order regardless of customer)
- Linking to or from the draft orders wizard steps

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Detail/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/GetByOrderNumber/`
- The API endpoint must filter by both order number **and** the authenticated customer's ID — never return an order that belongs to another customer, and return the same 404-equivalent response whether the order doesn't exist or belongs to someone else
- Route: `/orders/{orderNumber}` — `orderNumber` maps to `Order.Id`
- The lookup form at `/orders/lookup` can be a simple text input that navigates to `/orders/{enteredNumber}` on submit
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] An unauthenticated visitor attempting to navigate to `/orders/{orderNumber}` or `/orders/lookup` is redirected to login
- [ ] An authenticated customer who enters their own valid order number is shown the full order detail page
- [ ] The detail page displays: order ID, status, created date, submission date (if submitted), all widgets with quantities, shipping address, billing address, and estimated shipping cost
- [ ] An authenticated customer who enters an order number belonging to a different customer sees a "not found" message (same as if the order does not exist)
- [ ] An authenticated customer who enters an order number that does not exist sees a "not found" message
- [ ] Orders of any status (Draft or Submitted) are accessible via this lookup
- [ ] Unit tests are written where appropriate
