---
status: split
milestone: 2
github_issue: 104
task_issues: [105, 106]
---

# Profile: saved shipping and billing addresses

## User Story

> As an **authenticated customer**,
> I need to save a default shipping address and a default billing address on my profile,
> in order to avoid re-entering the same address details every time I place an order.

## Background

Customers currently have no way to persist address information between orders. This story extends
the existing customer profile page (Milestone 1, story 05) with shipping and billing address
sections. Saved addresses are used to pre-populate the order wizard (story 10).

Both address sections are optional — customers are not required to save addresses on their
profile.

## Scope

**In scope:**
- Shipping address section and billing address section added to the existing profile edit page (`/accounts/profile`)
- Address fields for each: recipient name, street line 1, street line 2 (optional), city, state/province, postal code, country
- A "Same as shipping address" toggle that copies shipping address values into the billing address fields
- Saving either address section is optional; a customer may save only shipping, only billing, or both
- Both address sections are saved as part of the existing profile save action (no separate save button per section)
- `Customer` entity extended with shipping and billing address columns; EF Core migration required

**Out of scope:**
- Storing multiple saved addresses per customer (one default of each type only)
- Address book or labelled address management
- Address validation against a postal database or geocoding service

## Developer Notes

- This story modifies the existing feature slice: `WidgetDepot.Web/Features/Accounts/Profile/`
- This story modifies the existing API endpoint: `WidgetDepot.ApiService/Features/Accounts/UpdateProfile/`
- Add shipping and billing address columns to the `Customer` entity using owned value objects or prefixed flat columns (consistent with the approach chosen in story 02)
- All new address fields are nullable — the customer is not required to provide them
- The profile GET endpoint should return the saved addresses so the page can pre-populate them on load
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] The profile edit page includes a shipping address section and a billing address section
- [ ] Each section collects: recipient name, street line 1, street line 2 (optional), city, state/province, postal code, and country
- [ ] The "Same as shipping address" toggle copies the current shipping address values into the billing address fields
- [ ] All address fields are optional; the profile can be saved without providing any address
- [ ] When the customer saves the profile, the address values are persisted to the database
- [ ] On returning to the profile edit page, previously saved address values are pre-filled in the form
- [ ] If only one address type has been saved, the other section's fields are empty
- [ ] Unit tests are written where appropriate
