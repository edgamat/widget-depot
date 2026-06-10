---
status: draft
milestone: 4
github_issue:
task_issues: []
---

# Force Password Reset on Next Login

## User Story

> As a **WDI Staff (Admin)**,
> I need a customer to be forced to change their temporary password the next time they log in,
> so that an admin-generated temporary password cannot remain in use indefinitely.

## Background

Story 03-admin-customer-management lets an admin reset a customer's password to a system-generated temporary password, which is read directly off the screen. As written, that temporary password works like any other password until the customer chooses to change it. This story adds the "must change password" behavior on top of that flow: a customer whose password was reset by an admin is required to set a new password before they can continue.

This was split out of story 03 to keep that story self-contained.

## Scope

**In scope:**
- A "must change password" flag on the customer account, set when an admin resets the password (story 03)
- On login, a customer with the flag set is required to choose a new password before reaching the rest of the application
- Once the customer sets a new password, the flag is cleared

**Out of scope:**
- The admin-facing reset action itself (covered in story 03)
- Email delivery of the temporary password
- Password complexity/policy changes beyond what already exists in the registration/profile flows

## Developer Notes

- Adds a data-model field (e.g. `MustChangePassword`) to the customer/account entity, with the accompanying migration.
- Story 03's password-reset action sets this flag when it generates the temporary password.
- The login flow must check the flag and redirect to a "set a new password" screen, reusing the existing password-change validation from the profile flow (`Features/Accounts/Profile/`) where possible.
- Depends on the project auth scheme (still undecided — Azure Entra was denied) and on how the login flow is implemented; confirm with story owner before building.

## Acceptance Criteria

- [ ] When an admin resets a customer's password (story 03), the account is flagged as requiring a password change
- [ ] On the next login, a flagged customer is required to set a new password before accessing the rest of the application
- [ ] After the customer sets a valid new password, the flag is cleared and normal access resumes
- [ ] The new-password screen enforces the same validation as the existing password-change flow
- [ ] A customer without the flag set logs in normally with no change to current behavior

## Refinement Notes

Split out of story 03-admin-customer-management at the user's request: the force-reset behavior is wanted within milestone 4 but tracked as its own story so story 03 stays focused on the admin editing actions.
