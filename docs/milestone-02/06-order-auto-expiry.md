# 30-day order auto-expiry

## User Story

> As **WDI**,
> I need draft orders that haven't been submitted within 30 days to be automatically removed,
> in order to keep the database free of abandoned orders.

## Background

Customers can save draft orders and return to submit them later (story 04). To prevent stale
drafts from accumulating indefinitely, any draft order that is not submitted within 30 days of
creation is automatically deleted. The expiry date is surfaced to customers in the draft orders
list so they know how much time they have.

## Scope

**In scope:**
- A scheduled background job that runs once per day
- The job deletes all `Draft` orders where `CreatedAt` is more than 30 days in the past
- Deleted orders are removed permanently (no soft-delete or archive)

**Out of scope:**
- Notifying the customer before or after expiry (no email reminder)
- Configurable expiry window (30 days is fixed per the PRD)
- A UI for viewing or restoring expired orders

## Developer Notes

- The job should live in `WidgetDepot.ApiService` alongside other background services, or in the appropriate Aspire-hosted project if a worker service already exists
- Use a standard .NET hosted service (`IHostedService` / `BackgroundService`) or the project's existing scheduling mechanism if one is already in place
- The deletion query: `DELETE FROM Orders WHERE Status = 'Draft' AND CreatedAt < NOW() - INTERVAL '30 days'`
- The job should be idempotent — running it multiple times in a day must have no harmful side effects
- Log the number of orders deleted each time the job runs
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; no MediatR

## Acceptance Criteria

- [ ] A background job runs once per day (exact schedule configurable, e.g., midnight UTC)
- [ ] The job deletes all Draft orders with a `CreatedAt` timestamp older than 30 days
- [ ] Submitted orders are never deleted by this job regardless of age
- [ ] The job is idempotent: running it multiple times on the same day produces the same result as running it once
- [ ] The number of orders deleted is logged each time the job executes
- [ ] Unit tests are written where appropriate
