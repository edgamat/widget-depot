# Order wizard: shipping estimate

## User Story

> As an **authenticated customer**,
> I need to see an estimated shipping cost for my order before saving or submitting it,
> in order to make an informed decision about whether to proceed.

## Background

This is step 3 of the order creation wizard, reached after the customer has entered addresses
(story 02). The system calls a third-party shipping API to calculate an estimated cost based on
the total weight of the ordered widgets and the destination shipping address. The estimate is
stored on the order.

**Dependency:** The shipping API vendor, endpoint, and credentials are described in [shipping-api](docs/standards/shipping-api.md).

## Scope

**In scope:**
- Step 3 of the wizard: display of the calculated shipping estimate
- Calling the third-party shipping API on every arrival at step 3 to obtain a fresh estimate (including when resuming a draft that already has a stored estimate), overwriting any previously stored value
- Storing the estimated shipping cost (`ShippingEstimate`, decimal) on the `Order` entity
- EF Core migration for the new column if not already present
- A "Back" button that returns to step 2
- A "Save for later" button that saves the draft order and redirects to the customer's draft orders list (story 04)
- A "Submit order" button that transitions to order submission (story 05)

**Out of scope:**
- Actual payment or charge (WDI does not process payments)
- Displaying a guaranteed shipping date

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Create/Step3/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/CalculateShipping/`
- The shipping API client should be implemented behind an interface (e.g., `IShippingApiClient`) so it can be substituted in tests without calling the real API
- Total weight is calculated as the sum of (`Widget.Weight` × `OrderItem.Quantity`) across all items, by joining `OrderItem` to the `Widget` catalog at the time the shipping estimate is requested
- If the shipping API call fails, show an error message and allow the customer to retry; do not advance the wizard
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] Step 3 is only reachable after step 2 has been completed (order has addresses)
- [ ] On arrival at step 3, the system **always** calls the third-party shipping API for a fresh estimate and displays the result, overwriting any previously stored estimate
- [ ] The displayed estimate shows the cost in dollars (or the appropriate currency)
- [ ] If the API call fails, an error message is shown and a "Retry" option is available; the wizard does not advance
- [ ] The estimated shipping cost is stored on the order
- [ ] Clicking "Back" returns the customer to step 2 with previously entered addresses still present
- [ ] Clicking "Save for later" saves the current draft state and redirects to the draft orders list
- [ ] Clicking "Submit order" initiates order submission (story 05)
- [ ] Unit tests are written where appropriate, using a test double for the shipping API client
