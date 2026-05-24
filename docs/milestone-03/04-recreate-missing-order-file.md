---
status: draft
milestone: 3
github_issue:
task_issues: []
---

# Re-create Missing Order File

## User Story

> As a **customer**,
> I need to re-create the order file for an order whose transmission status is Missing,
> in order to unblock ERP delivery without waiting for support.

## Background

When the daily FTP job (Story 2) cannot find an order file in the pickup directory, it sets the order's transmission status to `Missing` and retries on every subsequent run. However, retrying cannot succeed if the file is genuinely absent. This story gives the customer a self-service way to regenerate the file from the order data already stored in the database, resetting the order to `Pending` so the daily job can transmit it normally.

## Scope

**In scope:**
- Re-create action on My Recent Orders, visible only for orders with `Missing` status
- Confirmation prompt before proceeding
- System regenerates the order file in the pickup directory using stored order data and the ERP format spec (`docs/standards/erp-order-format.md`)
- Transmission status reset to `Pending` after successful file creation
- Confirmation message shown to the customer

**Out of scope:**
- Immediate FTP attempt after re-creation (status returns to `Pending`; the daily job handles transmission)
- Re-create action for any status other than `Missing`
- Admin-initiated file re-creation

## Developer Notes

- File generation uses the same format and naming convention as the original submission: `docs/standards/erp-order-format.md`
- After the file is written, set transmission status to `Pending` — do not attempt FTP inline
- The daily job (Story 2) will pick up the regenerated file on its next run

## Acceptance Criteria

- [ ] My Recent Orders shows a re-create action only for orders with `Missing` status
- [ ] Activating the action displays a confirmation prompt before proceeding
- [ ] On confirmation, the system regenerates the order file in the pickup directory using the order's stored data and the ERP format spec
- [ ] After successful file creation, the order's transmission status is reset to `Pending`
- [ ] A confirmation message is shown to the customer after the file is re-created
- [ ] The re-create action is not shown for orders with `Pending`, `Transmitted`, or `Failed` status

## Refinement Notes

Re-creation resets to `Pending` rather than triggering immediate FTP — confirmed with user. This mirrors the original submission flow and avoids duplicating the FTP logic from Story 3 (on-demand retransmission). The daily job then picks the order up on the next run.
