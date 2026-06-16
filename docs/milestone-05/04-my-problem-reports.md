---
status: shared
milestone: 5
github_issue: 159
task_issues: []
---

# My Problem Reports

## User Story

> As a **customer**,
> I need to see my recent problem reports and retry failed email notifications,
> in order to confirm my reports were received by warehouse staff.

## Background

After filing a problem report, a customer has no way to know if the warehouse staff email was delivered. This page surfaces that status and allows the customer to trigger a resend if the email failed. The `EmailSent` flag on the problem report (set in story 3) is the data source for email status.

## Scope

**In scope:**
- A "My Problem Reports" page listing the customer's most recent 10 problem reports
- Each row shows: order number, date order was submitted, date report was filed, email status
- "Resend email" button for any report where `EmailSent = false`
- Resend attempts to send to the same configured warehouse staff address
- On successful resend, `EmailSent` is updated to `true` and the button is removed
- On failed resend, an error message is shown and the button remains

**Out of scope:**
- Paginating beyond the most recent 10 reports
- Automatic background retry (all retries are customer-initiated)
- Editing or deleting a submitted problem report

## Developer Notes

- Resend uses the same email logic as the initial send in story 3.
- The `EmailSent` flag is the sole source of truth for retry eligibility.

## Acceptance Criteria

- [ ] An authenticated customer can access the "My Problem Reports" page
- [ ] The page lists the customer's most recent 10 problem reports; if fewer than 10 exist, all are shown
- [ ] Each row displays: order number, date the order was submitted, date the report was filed, and email status (sent / not sent)
- [ ] Reports where `EmailSent = false` display a "Resend email" button
- [ ] Clicking "Resend email" triggers an email send to the configured warehouse staff address
- [ ] On successful resend, `EmailSent` is set to `true` and the "Resend email" button is no longer shown
- [ ] If the resend fails, an error message is shown and the "Resend email" button remains

## Refinement Notes

This story emerged from discussion about what happens when the initial email send fails (story 3). The customer-initiated retry model was chosen over a background retry job for simplicity. The 10-report limit was a deliberate choice — pagination deferred; the most common use case is checking the most recent report.
