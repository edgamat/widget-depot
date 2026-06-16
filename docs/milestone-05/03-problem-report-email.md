---
status: draft
milestone: 5
github_issue:
task_issues: []
---

# Problem Report: Email Notification

## User Story

> As a **warehouse staff member**,
> I need to receive an email when a customer files a problem report,
> in order to act on the reported issue without manually checking the system.

## Background

When a customer submits a problem report, the system attempts to send an email to a configured warehouse staff mailbox. The report is always saved to the database regardless of email send outcome. An `EmailSent` flag on the report tracks whether the email was delivered successfully. Retry handling for failed sends is addressed in the "My Problem Reports" story.

## Scope

**In scope:**
- Sending an email to the configured warehouse staff address on problem report submission
- Email content: order number, affected items with issue types, and report notes
- `EmailSent` flag on the problem report (set to `true` on successful send, `false` on failure)
- Logging email send failures
- Warehouse staff email address configurable in app settings

**Out of scope:**
- Customer-facing email confirmation (the wizard confirmation message covers this)
- Retry UI for failed emails (handled in the "My Problem Reports" story)
- Admin UI configuration of the recipient address

## Developer Notes

- This story includes setting up email integration for the first time in the project — there is no existing email infrastructure to build on.
- Use the **MailKit** NuGet package for sending email.
- For local development, spin up **Mailpit** as a containerized SMTP server via the Aspire community toolkit (`CommunityToolkit.Aspire.Hosting.Mailpit`). Mailpit captures outbound emails and exposes a web UI (port 8025) for inspection — no real emails are sent locally. In production, swap the connection string for a real SMTP provider.
- Guidance on building Aspire hosting and client integrations:
  - [Hosting integrations](https://aspire.dev/integrations/custom-integrations/hosting-integrations)
  - [Client integrations](https://aspire.dev/integrations/custom-integrations/client-integrations)
- The report must be saved before the email is attempted, so a send failure never causes data loss.
- The `EmailSent` flag is the mechanism "My Problem Reports" will use to surface retry eligibility.
- Recipient address lives in app configuration (e.g. `appsettings.json`), not in the database.

## Acceptance Criteria

- [ ] When a problem report is submitted, an email is sent to the configured warehouse staff address
- [ ] The email contains the full report: order number, affected items with issue types, and notes
- [ ] The warehouse staff email address is configurable in app settings
- [ ] On successful send, `EmailSent` is set to `true` on the problem report record
- [ ] If the email send fails, the report is still saved with `EmailSent` set to `false`
- [ ] Email send failures are logged
- [ ] The customer sees a confirmation message regardless of whether the email send succeeded

## Refinement Notes

Report persistence is decoupled from email delivery by design — confirmed with user. The `EmailSent` flag is the bridge to the retry flow that will live in "My Problem Reports." Recipient address is app config, not a database setting, so no admin UI is needed here.
