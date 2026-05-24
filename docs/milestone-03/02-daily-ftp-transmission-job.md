---
status: draft
milestone: 3
github_issue:
task_issues: []
---

# Daily FTP Transmission Job

## User Story

> As a **customer**,
> I need my submitted orders to be automatically transmitted to the ERP system each day,
> in order to have my orders fulfilled without any manual intervention.

## Background

When an order is submitted, a fixed-width text file is written to a local pickup directory (format defined in `docs/standards/erp-order-format.md`). This job reads those files and transmits them to the ERP system via FTP once per day. Transmission status is tracked per order (see Story 1) so customers can monitor progress.

## Scope

**In scope:**
- Scheduled daily job that processes all orders with `Pending` or `Missing` transmission status
- For each order: locate the file in the pickup directory, transmit via FTP, update status
- If the file is not found: set status to `Missing` (retried on every subsequent run)
- If transmission succeeds: set status to `Transmitted` with current timestamp
- If transmission fails: set status to `Failed` with current timestamp
- Unit/integration tests validating generated order files conform to the ERP format spec
- Aspire AppHost FTP server resource for local development

**Out of scope:**
- Retrying `Failed` orders (on-demand retransmission, Story 3)
- UI changes (covered in Story 1)
- Modifying the ERP file format or FTP configuration

## Developer Notes

- FTP configuration (host, credentials, target directory) comes from app config; the Aspire AppHost overrides these for local development using a `delfer/alpine-ftp-server` container
- The job processes `Pending` and `Missing` orders each run; `Failed` orders are left untouched
- `Missing` orders re-enter the queue automatically on every run — no manual intervention needed
- ERP file format spec: `docs/standards/erp-order-format.md` — fixed-width, `\r\n` line separators, filename `EXT-{OrderNum zero-padded to 10}.TXT`

## Acceptance Criteria

- [ ] A scheduled job runs once per day and processes all orders with `Pending` or `Missing` status
- [ ] If the order file is not found in the pickup directory, the order status is set to `Missing` with the current timestamp
- [ ] If the order file is found, the job transmits it to the configured FTP server
- [ ] On successful transmission, the order status is updated to `Transmitted` with the current timestamp
- [ ] On FTP transmission failure, the order status is updated to `Failed` with the current timestamp
- [ ] `Failed` orders are not retried by the job
- [ ] `Missing` orders are retried on every subsequent job run
- [ ] The Aspire AppHost includes a `delfer/alpine-ftp-server` container that the app connects to in the Development environment
- [ ] Unit or integration tests verify that generated order files match the ERP format spec (filename convention, fixed-width fields, `\r\n` line separators, header/address/line-item structure)

## Refinement Notes

Four transmission statuses: `Pending` (set on submission), `Transmitted`, `Failed`, `Missing`. The job retries `Pending` and `Missing` each run; `Failed` is retried only on customer demand (Story 3). `Missing` was introduced to distinguish "file not found in pickup directory" from an FTP-level failure, as both are recoverable but through different paths.

FTP credentials and configuration are not changed from the existing system (hard constraint from PRD §7.1).
