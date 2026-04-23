# Order wizard: widget selection

## User Story

> As an **authenticated customer**,
> I need to start a new order by selecting widgets and specifying quantities,
> in order to begin the process of placing an order.

## Background

Order creation is a multi-step wizard. This story covers step 1: selecting one or more widgets
from the catalog and entering desired quantities. At the end of this step the order is persisted
as a draft so that subsequent wizard steps and the save-draft feature (story 04) can build on it.

This story also introduces the core `Order` and `OrderItem` data model.

## Scope

**In scope:**
- An authenticated-only page at `/orders/new` that begins the wizard
- A searchable widget list (reuses catalog search) for the customer to pick from
- Ability to add a widget with a quantity, adjust quantity, and remove a widget from the order
- A running summary of selected widgets and quantities
- A "Continue" button that persists the order in `Draft` status and advances to step 2
- `Order` entity: `Id`, `CustomerId`, `Status` (Draft / Submitted / Cancelled), `CreatedAt`
- `OrderItem` entity: `Id`, `OrderId`, `WidgetId`, `Quantity`
- EF Core migration for both entities

**Out of scope:**
- Address entry (step 2, story 02)
- Shipping cost estimate (step 3, story 03)
- Submitting the order (story 05)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Orders/Create/Step1/`
- New API endpoint: `WidgetDepot.ApiService/Features/Orders/CreateDraft/`
- New entities in `WidgetDepot.ApiService/Data/`: `Order.cs`, `OrderItem.cs`; add `DbSet<Order>` and `DbSet<OrderItem>` to `AppDbContext`; create a new EF Core migration
- `WidgetWeight` on `OrderItem` should be captured at order time (denormalized) so that the shipping estimate is stable even if the catalog later changes
- Order status should be modelled as an enum: `Draft = 0`, `Submitted = 1`, `Cancelled = 2`
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR
- The widget search on this page can reuse the same API endpoint used by the public catalog

## Acceptance Criteria

- [ ] An unauthenticated visitor attempting to navigate to `/orders/new` is redirected to login
- [ ] An authenticated customer can navigate to `/orders/new` and sees a widget search interface
- [ ] Searching for a widget by name or description displays matching results from the catalog
- [ ] The customer can add a widget to the order by specifying a quantity (minimum 1)
- [ ] The customer can remove a widget from the order before continuing
- [ ] The customer can change the quantity of an added widget before continuing
- [ ] A summary of selected widgets and their quantities is visible at all times during this step
- [ ] The "Continue" button is disabled (or shows a validation message) if no widgets have been added
- [ ] Clicking "Continue" with at least one widget persists an `Order` in `Draft` status with the correct `CustomerId` and the corresponding `OrderItem` rows, and advances the wizard to step 2
- [ ] Unit tests are written where appropriate
