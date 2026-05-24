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

When the daily FTP job (Story 2) cannot find an order file in the pickup directory, it sets the order's transmission status to `Missing` and retries on every subsequent run. However, retrying cannot succeed if the file is genuinely absent. This story gives the customer a self-service way to regenerate the file from the order data already stored in the database and immediately retransmit it to the FTP site using the same service as Story 3, so the customer does not need to wait for the next daily job run.

## Scope

**In scope:**
- Re-create action on My Recent Orders, visible only for orders with `Missing` status
- Confirmation prompt before proceeding
- System regenerates the order file in the pickup directory using stored order data and the ERP format spec (`docs/standards/erp-order-format.md`)
- Immediate FTP retransmission using Story 3's service after file re-creation
- Transmission status set to `Transmitted` after successful FTP retransmission
- Transmission status set to `Failed` if FTP retransmission fails
- Error message shown to customer if FTP retransmission fails
- Confirmation message shown to the customer after successful re-creation and transmission

**Out of scope:**
- Re-create action for any status other than `Missing`
- Admin-initiated file re-creation

## Developer Notes

- File generation uses the same format and naming convention as the original submission: `docs/standards/erp-order-format.md`
- After the file is written, invoke the same FTP retransmission service used in Story 3
- Set transmission status to `Transmitted` on successful FTP upload; set to `Failed` if the upload fails

## Acceptance Criteria

- [ ] My Recent Orders shows a re-create action only for orders with `Missing` status
- [ ] Activating the action displays a confirmation prompt before proceeding
- [ ] On confirmation, the system regenerates the order file in the pickup directory using the order's stored data and the ERP format spec
- [ ] After successful file creation, the system immediately retransmits the file to the FTP site using the same service as Story 3
- [ ] After successful FTP retransmission, the order's transmission status is set to `Transmitted`
- [ ] If FTP retransmission fails, the order's transmission status is set to `Failed` and an error message describing the failure is shown to the customer
- [ ] A confirmation message is shown to the customer after successful re-creation and FTP transmission
- [ ] The re-create action is not shown for orders with `Pending`, `Transmitted`, or `Failed` status

## Refinement Notes

PO changed requirement: re-creation now triggers immediate FTP retransmission rather than resetting to `Pending` for the daily job. Reuses Story 3's FTP service to avoid duplicating logic. On success the status is set to `Transmitted`; on FTP failure it is set to `Failed` and the customer sees an error message.
