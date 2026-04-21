# Order wizard: pre-fill addresses from profile

## User Story

> As an **authenticated customer**,
> I need the order wizard to pre-fill my shipping and billing addresses from my profile,
> in order to save time when placing orders without having to retype the same details.

## Background

Once customers can save default addresses on their profile (story 09), those saved values
should be carried into the order wizard automatically. When the customer reaches step 2
(address entry), the form is pre-populated with their saved addresses. They can edit the
values before continuing, so the saved address acts as a convenient starting point rather
than a hard constraint.

If no address has been saved on the profile, the behaviour is unchanged from story 02 (empty
form fields).

## Scope

**In scope:**
- Step 2 of the order wizard pre-fills the shipping and billing address fields with the customer's saved profile addresses when they are available
- The customer can edit any pre-filled field before continuing; the order stores whatever values are present when "Continue" is clicked (profile defaults are not modified)
- If only one address type is saved on the profile, only that section is pre-filled; the other remains empty
- If no addresses are saved on the profile, the form behaves exactly as before (empty)

**Out of scope:**
- A "reset to profile defaults" button (the customer can navigate back to their profile to update saved addresses)
- Saving address edits made during the wizard back to the profile

## Developer Notes

- Modifies the existing feature slice: `WidgetDepot.Web/Features/Orders/Create/Step2/`
- No new API endpoints required — step 2 should call the existing profile GET endpoint (or a dedicated lightweight endpoint) to retrieve the customer's saved addresses and use them to initialise the address form
- Pre-fill happens client-side at component initialisation; no changes to the address save endpoint
- The order stores the addresses as entered at "Continue" time, independent of the profile values
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] When a customer with saved profile addresses reaches step 2 of the order wizard, the shipping address fields are pre-filled with their saved shipping address
- [ ] When a customer with saved profile addresses reaches step 2, the billing address fields are pre-filled with their saved billing address
- [ ] The customer can edit any pre-filled field; the edited values (not the profile defaults) are stored on the order when "Continue" is clicked
- [ ] Editing the address fields during the wizard does not modify the customer's saved profile addresses
- [ ] When a customer has no saved profile addresses, step 2 behaves as before (empty address fields)
- [ ] When only one address type is saved, only that section is pre-filled; the other section remains empty
- [ ] Unit tests are written where appropriate
