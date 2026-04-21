# Order wizard: addresses

## User Story

> As an **authenticated customer**,
> I need to enter shipping and billing addresses for my order,
> in order to tell WDI where to send my widgets and who to bill.

## Background

This is step 2 of the order creation wizard, reached after the customer has selected widgets
(story 01). The customer provides a shipping address (where the widgets will be sent) and a
billing address. Both addresses are stored on the order.

## Scope

**In scope:**
- Step 2 of the wizard: a form with shipping address fields and billing address fields
- Address fields for each address: recipient name, street line 1, street line 2 (optional), city, state/province, postal code, country
- A "Same as shipping address" toggle that copies the shipping address into the billing address fields
- Client-side and server-side validation of all required fields
- A "Back" button that returns to step 1 without losing wizard state
- A "Continue" button that saves the addresses to the draft order and advances to step 3
- Address data stored on the `Order` entity (inline columns or owned value objects — developer's choice)
- EF Core migration for the new address columns

**Out of scope:**
- Address validation against a postal address database or geocoding service
- Saving a customer's address for re-use on future orders
- Shipping cost calculation (step 3, story 03)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Create/Step2/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/SaveAddresses/`
- Address fields can be stored as owned entity types (`ShippingAddress` / `BillingAddress`) on `Order` using EF Core owned entities, or as prefixed flat columns — whichever is simpler
- The wizard must carry the `OrderId` (created in step 1) forward to this step (e.g., via route parameter or Blazor cascading state)
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] Step 2 is only reachable after step 1 has been completed (order exists in Draft status)
- [ ] The form displays separate sections for shipping address and billing address
- [ ] Each address section collects: recipient name, street line 1, street line 2 (optional), city, state/province, postal code, and country
- [ ] Submitting with any required address field empty shows a validation message and does not proceed
- [ ] The "Same as shipping address" toggle, when checked, populates the billing address fields with the values from the shipping address fields
- [ ] Clicking "Back" returns the customer to step 1 with previously selected widgets still present
- [ ] Clicking "Continue" with valid addresses saves the addresses to the draft order and advances to step 3
- [ ] Unit tests are written where appropriate
