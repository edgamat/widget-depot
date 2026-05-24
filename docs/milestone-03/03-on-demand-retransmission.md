---
status: draft
milestone: 3
github_issue:
task_issues: []
---

# On-Demand Retransmission

## User Story

> As a **customer**,
> I need to request immediate retransmission of a failed order file from My Recent Orders,
> in order to recover from a transmission failure without waiting for the next daily batch.

## Background

The daily FTP job (Story 2) only retries `Pending` and `Missing` orders. Orders that fail FTP transmission are marked `Failed` and require explicit customer action to retry. This story adds that capability directly to the My Recent Orders page.

## Scope

**In scope:**
- A retransmit action on My Recent Orders, visible only for orders with `Failed` status
- Confirmation prompt before the retransmission attempt
- Immediate FTP transmission attempt on confirmation
- Status and timestamp updated based on the outcome
- Confirmation message shown to the customer after the attempt

**Out of scope:**
- Retransmission for `Missing` orders (those are automatically retried by the daily job)
- Admin-initiated retransmission (not in this milestone)
- Bulk retransmission of multiple orders at once

## Developer Notes

- Retransmission follows the same logic as the daily job: locate the file, attempt FTP, update status
- If the file is not found during retransmission, set status to `Missing` — consistent with job behavior
- The retransmit action should only be rendered for `Failed` orders; do not show it for `Pending`, `Transmitted`, or `Missing`

## Acceptance Criteria

- [ ] My Recent Orders shows a retransmit action for orders with `Failed` status only
- [ ] Activating the retransmit action displays a confirmation prompt before proceeding
- [ ] On confirmation, the system immediately attempts FTP transmission of the order file
- [ ] If the file is not found, the order status is updated to `Missing` with the current timestamp
- [ ] On successful transmission, the order status is updated to `Transmitted` with the current timestamp
- [ ] On FTP failure, the order status remains `Failed` with an updated timestamp
- [ ] A confirmation message is displayed to the customer after the attempt, reflecting the outcome
- [ ] The retransmit action is not shown for orders with `Pending`, `Transmitted`, or `Missing` status

## Refinement Notes

On-demand retransmission is scoped to `Failed` orders only. `Missing` orders recover automatically via the daily job and do not need a manual trigger. Retransmission reuses the same file-lookup-and-FTP logic as the job, including the `Missing` status outcome if the file is absent.
