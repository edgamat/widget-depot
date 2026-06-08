---
status: draft
milestone: 4
github_issue:
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

- The order detail view should reuse or closely mirror the data shown on the customer-facing order detail, with the ERP transmission status added.

## Acceptance Criteria

- [ ] Admin can navigate to the order lookup page from the admin navigation
- [ ] Admin can enter an order number and submit the lookup
- [ ] If the order exists, the full order details are displayed (customer, items, quantities, addresses, order status, and ERP transmission status)
- [ ] If the order number does not exist, a clear "not found" message is shown
- [ ] If the order number field is submitted empty, a validation message is shown

## Refinement Notes

Lookup is by order number only — searching by customer or date is deferred. ERP transmission status is included in the detail view, per the Milestone 3 feature. No editing or retransmission actions are in scope for this screen.
