---
status: ready
milestone: 2
github_issue:
task_issues: []
---

# Order wizard: update profile addresses from order

## User Story

> As an **authenticated customer**,
> I need the option to save the addresses I enter during checkout back to my profile,
> in order to keep my saved defaults up to date without visiting the profile page separately.

## Background

This story is the complement to story 10 (pre-fill addresses from profile). Story 10 explicitly
scoped out saving wizard address edits back to the profile; this story adds that capability.

When a customer reaches step 2 of the order wizard, two "Save to profile" checkboxes appear —
one for shipping, one for billing. Both are pre-checked by default. If the customer clicks
"Continue" with a checkbox checked, that address is written back to their profile at that moment
(not deferred to order submission). The save is best-effort: if it fails, the wizard proceeds
to step 3 regardless.

## Scope

**In scope:**
- Two "Save to profile" checkboxes on step 2: one below the shipping address fields, one below the billing address fields
- Both checkboxes are pre-checked by default
- When "Continue" is clicked: each checked address is saved (created or updated) on the customer's profile immediately
- Unchecking a checkbox means that address type on the profile is not modified

**Out of scope:**
- Deleting a saved profile address (unchecking a checkbox does not delete the existing saved address)
- Deferring the profile save to order submission time

## Developer Notes

- Modifies the existing feature slice: `WidgetDepot.Web/Features/Orders/Create/Step2/`
- On "Continue", if a checkbox is checked, call the profile update endpoint to persist that address; if the call fails, log the error and proceed to step 3 — do not block the wizard
- The save writes whatever address values are in the form at "Continue" time; no additional validation beyond what step 2 already enforces
- If the customer has no saved address of that type yet, the call creates one; if they do, it updates it
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] Step 2 displays a "Save to profile" checkbox below the shipping address fields and another below the billing address fields
- [ ] Both checkboxes are pre-checked by default
- [ ] When the customer clicks "Continue" with the shipping checkbox checked, the shipping address entered is saved (created or updated) on their profile
- [ ] When the customer clicks "Continue" with the billing checkbox checked, the billing address entered is saved (created or updated) on their profile
- [ ] When a checkbox is unchecked and the customer clicks "Continue", that address type on their profile is not modified
- [ ] A profile save failure does not block the wizard; the customer proceeds to step 3 regardless
- [ ] Unit tests are written where appropriate

## Refinement Notes

Story 10 pre-fills addresses from the profile into the wizard and explicitly scoped out writing
edits back — this story adds that capability as a deliberate follow-on.

Both checkboxes are pre-checked by default (opt-out rather than opt-in) so the common case
(customer edits addresses and wants them saved) requires no extra action.

The profile save fires on "Continue" click, not at order submission, so the profile is updated
even if the order is later abandoned. Profile save failures are swallowed — the wizard must
never be blocked by a non-critical side-effect.

Unchecking a checkbox is a no-op against the profile: it does not delete the existing saved
address, it simply skips the update.
