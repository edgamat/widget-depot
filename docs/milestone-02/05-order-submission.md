# Order submission

## User Story

> As an **authenticated customer**,
> I need to submit my completed draft order,
> in order to send my widget order to WDI for fulfillment.

## Background

Once a customer has completed all wizard steps (widgets, addresses, shipping estimate), they
can submit the order. Submission changes the order status to `Submitted` and causes the system
to write an order file to a designated pickup directory. A separate scheduled job (story in
Milestone 3) transmits those files to the ERP system via FTP.

**Dependency:** The ERP order file format is described here:

[ERP File Format](../standards/erp-order-format.md)

## Scope

**In scope:**
- A final review screen in the wizard showing a summary of widgets, addresses, and shipping estimate
- A "Submit" button that triggers order submission
- On submission: order status is updated to `Submitted`, `SubmittedAt` set to UTC timestamp, an order file is written to the configured pickup directory
- A confirmation page/message shown to the customer after successful submission, including the order ID
- Submitted orders are read-only; no further edits or deletion are possible

**Out of scope:**
- FTP transmission to the ERP system (Milestone 3, story separate)
- Email confirmation to the customer (not required by the PRD)
- Payment processing (out of scope for the entire project)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Submit/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/Submit/`
- Create an order file class (OrderFile) that uses the order, the addresses, and line items
  and creates the contents of the file as a string. 
- The order file writer should be implemented behind an interface (e.g., `IOrderFileWriter`) so it can be tested without touching the filesystem
- The pickup directory path should be read from configuration (e.g., `Orders:PickupDirectory`)
- Order file format must replicate the existing ERP format exactly
- Only orders with `Draft` status and all required fields populated (widgets, both addresses, shipping estimate) may be submitted; return a typed error otherwise
- `SubmittedAt` timestamp should be added to the `Order` entity (nullable; set when status transitions to `Submitted`) — add an EF Core migration
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] The final review screen displays a summary of all order details: selected widgets with quantities, shipping address, billing address, and estimated shipping cost
- [ ] A "Back" button on the review screen returns to step 3 without changing the order
- [ ] Clicking "Submit" submits the order and writes an order file to the configured pickup directory
- [ ] After successful submission, the customer sees a confirmation message containing the order ID
- [ ] The submitted order no longer appears in the customer's draft orders list
- [ ] An order that is missing widgets, addresses, or a shipping estimate cannot be submitted (API returns a descriptive error)
- [ ] Unit tests are written where appropriate, using a test double for the order file writer
