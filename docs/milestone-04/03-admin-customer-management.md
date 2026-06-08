---
status: draft
milestone: 4
github_issue:
task_issues: []
---

# Admin: Customer Management

## User Story

> As a **WDI Staff (Admin)**,
> I need to edit a customer's email address, reset their password, and manage their admin rights,
> in order to assist customers with account issues and control who has admin access.

## Background

This story covers the editing actions on a customer's account, building on the read-only profile view from the Admin: Customer List story. All actions are initiated from the customer profile view in the admin area.

## Scope

**In scope:**
- Edit a customer's email address
- Reset a customer's password (system-generated temporary password, with confirmation step before it takes effect)
- Promote a customer to admin (`IsAdmin = true`)
- Demote an admin back to customer (`IsAdmin = false`)

**Out of scope:**
- Editing any other profile fields (e.g., name, address)
- Deactivating or deleting a customer account
- Sending the temporary password to the customer via email (admin reads it directly from the screen)

## Developer Notes

- All three actions are accessible from the customer profile view established in story 02-admin-customer-list.
- An admin cannot remove their own admin rights — the demote action must be disabled when viewing the currently logged-in user's own profile.
- Email uniqueness must be enforced at the application level before saving.

## Acceptance Criteria

- [ ] Admin can edit a customer's email address; the change is saved and reflected immediately in the customer list and profile view
- [ ] Validation prevents saving an empty or malformed email address
- [ ] If the new email address is already in use by another customer, the save is rejected with an error message
- [ ] Admin can reset a customer's password; a confirmation prompt is shown before the reset takes effect
- [ ] On confirmation, the system generates a temporary password and displays it to the admin
- [ ] Admin can promote a customer to admin (`IsAdmin = true`)
- [ ] Admin can demote an admin back to customer (`IsAdmin = false`)
- [ ] An admin cannot remove their own admin rights; the demote action is disabled on the currently logged-in user's profile
- [ ] All three actions are accessible from the customer profile view
- [ ] A success confirmation is shown after each action completes
- [ ] If an action fails, an error message is displayed and no changes are saved

## Refinement Notes

Password reset uses a system-generated temporary password displayed directly to the admin — no email delivery in scope. A confirmation step is required before the reset takes effect, per user request.

An admin cannot demote themselves — this covers both the "demoting yourself" and "demoting the last admin" concerns in a single simple rule. No need to track admin count.
