---
status: shared
milestone: 3
github_issue: 118
task_issues: []
---

# Order Transmission Status

## User Story

> As a **customer**,
> I need to see the ERP transmission status of each submitted order on My Recent Orders,
> in order to know whether my order has reached the warehouse without contacting support.

## Background

When an order is submitted, a file is written to a pickup directory for daily FTP transmission to the ERP system. Customers currently have no visibility into whether that file has been transmitted. This story introduces the status field and surfaces it in the UI; the actual transmission logic is implemented in Story 2.

## Scope

**In scope:**
- Add a transmission status field to submitted orders (Pending / Transmitted / Failed / Missing)
- Add a timestamp field to record when the status last changed (used for Transmitted, Failed, and Missing)
- Set transmission status to Pending when an order is submitted
- Display transmission status and conditional timestamp on the My Recent Orders page

**Out of scope:**
- Actual FTP transmission (Story 2)
- Updating status from Pending to Transmitted or Failed (Story 2)
- Customer-initiated retransmission UI (Story 3)

## Developer Notes

- Status transitions: Pending (set on submission, this story) → Transmitted or Failed (set by the daily job, Story 2)
- My Recent Orders shows submitted orders only, so every visible row will have a transmission status

## Acceptance Criteria

- [ ] When a customer submits an order, its transmission status is set to Pending
- [ ] My Recent Orders displays a transmission status for every order in the list
- [ ] Orders with Pending status show "Pending" with no timestamp
- [ ] Orders with Transmitted status show "Transmitted" and the timestamp when the file was transmitted
- [ ] Orders with Failed status show "Failed" and the timestamp of the last transmission attempt
- [ ] Orders with Missing status show "Missing" and the timestamp of the last failed file lookup

## Refinement Notes

Pending status is set at submission time (this story) rather than by the transmission job, so every submitted order always has a status value. Transmitted, Failed, and Missing all include a timestamp; Pending does not, as no transmission has been attempted yet. Missing was added during Story 2 refinement to distinguish "file not found in pickup directory" from an FTP-level failure.
