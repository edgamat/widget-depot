# Save draft from Step 1

## User Story

> As an **authenticated customer**,
> I need to save my widget selection on Step 1 without advancing to the next step,
> in order to preserve my progress and return to it later.

## Background

Step 1 of both the new-order wizard (story 01, `/orders/new`) and the edit-draft flow
(story 12, `/orders/{orderId}/step1`) currently only offer a "Continue" button that
persists the selection and advances to Step 2. This story adds a "Save Draft" button
alongside "Continue" so customers can save without committing to the next step.

**Dependency:** Requires story 01 (CreateDraft API and new-order Step 1 UI) and
story 12 (UpdateDraftItems API and edit-mode Step 1 UI).

## Scope

**In scope:**
- A "Save Draft" button added alongside "Continue" on Step 1 in both flows
- **New order (`/orders/new`):** clicking "Save Draft" calls the CreateDraft endpoint (story 01),
  then navigates to `/orders/{orderId}/step1` with a query param (e.g. `?saved=true`) that
  triggers a "Draft saved" success toast on arrival
- **Existing draft (`/orders/{orderId}/step1`):** clicking "Save Draft" calls the UpdateDraftItems
  endpoint (story 12), stays on the same page, and shows a "Draft saved" success toast inline
- Validation: "Save Draft" is disabled (or shows a validation message) if no widgets are selected —
  consistent with the "Continue" button behaviour
- No new API endpoints; both flows reuse endpoints already defined in stories 01 and 12

**Out of scope:**
- Saving from Step 2 or Step 3 (story 03 already has "Save for later" on Step 3)
- Any change to the "Continue" button behaviour

## Developer Notes

- Add "Save Draft" button to `WidgetDepot.Web/Features/Orders/Create/Step1/` (story 01 slice)
- Add "Save Draft" button to `WidgetDepot.Web/Features/Orders/Edit/Step1/` (story 12 slice)
- Reuse the `CreateDraft` endpoint (story 01) for the new-order save path
- Reuse the `UpdateDraftItems` endpoint (story 12) for the existing-draft save path
- Use a `?saved=true` query param (or Blazor navigation state) to trigger the success toast when
  landing on the edit-mode page after saving a brand-new order
- Button layout (Bootstrap): `[ Save Draft ]  [ Continue → ]` — Save Draft secondary, Continue primary
- Patterns to follow: Vertical Slice Architecture; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] A "Save Draft" button appears on Step 1 in both the new-order and edit-draft flows
- [ ] "Save Draft" is disabled (or shows a validation message) when no widgets are selected
- [ ] Clicking "Save Draft" on a new order (`/orders/new`) persists the order as `Draft` and
      navigates to `/orders/{orderId}/step1`, where a "Draft saved" success notification is shown
- [ ] Clicking "Save Draft" on an existing draft (`/orders/{orderId}/step1`) updates the draft's
      items, keeps the customer on the same page, and shows a "Draft saved" success notification
- [ ] The "Continue" button on Step 1 continues to work as before in both flows
- [ ] Unit tests are written where appropriate
