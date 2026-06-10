---
status: shared
milestone: 4
github_issue: 137
task_issues: []
---

# Admin: Order Lookup

## User Story

> As a **WDI Staff (Admin)**,
> I need to look up any order by order number,
> in order to assist customers with order questions and investigate issues.

## Background

Orders are placed by customers through the order creation wizard (Milestone 2). Submitted orders generate ERP files and have a transmission status (Pending, Transmitted, Failed) introduced in Milestone 3. This story provides read-only access to that data for admin staff without requiring database access.

## Scope

**In scope:**
- A lookup form where admin enters an order number
- Display of full order details on a successful lookup
- ERP transmission status included in the order details

**Out of scope:**
- Editing or cancelling orders
- Searching by anything other than order number (e.g., customer name, date range)
- Triggering ERP retransmission from this screen (that capability exists on the customer-facing orders page)

## Developer Notes

- **Shared view component.** Extract the inline order-detail markup (header, items table, shipping/billing addresses) currently in `Features/Orders/Detail/DetailPage.razor` into a presentational `OrderDetailView.razor` that takes an `OrderDetail` model. Both the customer-facing detail page and this admin lookup render the same component, so the layout lives in one place. Data fetching stays per-page — the two screens read from different endpoints. Suggested location: `src/WidgetDepot.Web/Features/Orders/Shared/OrderDetailView.razor`.

- **ERP transmission status shown to both audiences.** Add nullable `TransmissionStatus` / `TransmissionStatusChangedAt` to the `OrderDetail` model (`Features/Orders/Detail/DetailModels.cs`); the shared component renders an ERP-status row whenever the value is present. Per decision, the customer-facing detail page displays this too, for a consistent view across audiences — transmission status is already customer-visible on the order history page, so this exposes nothing new. This requires extending the customer `GET /orders/{orderNumber}` response to include the transmission fields; today only `GET /orders/recent` (`GetRecentSubmitted`) returns them.

- **Admin needs a non-customer-scoped data source.** The existing `GET /orders/{orderNumber}` endpoint filters by the authenticated customer (`o.CustomerId == customerId`), so it cannot serve admins looking up arbitrary orders. Add a separate admin endpoint (e.g. `GET /admin/orders/{orderNumber}`) and a matching web service that map into the same `OrderDetail` model and feed the shared component.

- **Admin authorization is a prerequisite.** No role/policy system exists yet (the admin area is a placeholder and `CatalogImport` is currently unauthenticated). The lookup page and its endpoint must be restricted to WDI staff once an admin role/policy is in place — track separately if needed.

## Acceptance Criteria

- [ ] Admin can navigate to the order lookup page from the admin navigation
- [ ] Admin can enter an order number and submit the lookup
- [ ] If the order exists, the full order details are displayed (customer, items, quantities, addresses, order status, and ERP transmission status)
- [ ] If the order number does not exist, a clear "not found" message is shown
- [ ] If the order number field is submitted empty, a validation message is shown

## Refinement Notes

Lookup is by order number only — searching by customer or date is deferred. ERP transmission status is included in the detail view, per the Milestone 3 feature. No editing or retransmission actions are in scope for this screen.
