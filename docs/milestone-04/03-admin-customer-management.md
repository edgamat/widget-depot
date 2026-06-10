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
- Edit a customer's email address (inline edit-in-place section on the profile view)
- Reset a customer's password (system-generated temporary password, with confirmation step before it takes effect)
- Promote a customer to admin (`IsAdmin = true`), with a confirmation step
- Demote an admin back to customer (`IsAdmin = false`), with a confirmation step
- Persistent "Back to Customer List" navigation on the profile view

**Out of scope:**
- Editing any other profile fields (e.g., name, address)
- Deactivating or deleting a customer account
- Sending the temporary password to the customer via email (admin reads it directly from the screen)

## Developer Notes

- All three actions are accessible from the customer profile view established in story 02-admin-customer-list.
- An admin cannot remove their own admin rights — the demote action must be disabled when viewing the currently logged-in user's own profile.
- Email uniqueness must be enforced at the application level before saving.

### Navigation & UX conventions to reuse

- **Back to list:** the profile view includes a persistent "Back to Customer List" link (visible at all times, not only after an action), navigating via `NavigationManager.NavigateTo("/admin/customers")` — following the explicit Back-button pattern in the Orders wizard (`Features/Orders/Create/Step3/Step3Page.razor`). The app has no breadcrumbs.
- **Post-action behavior:** every action keeps the admin on the profile view and surfaces an inline Bootstrap success alert, mirroring the `saved=true` query-param pattern in `Features/Accounts/Profile/ProfilePage.razor`. The admin returns to the list manually via the back link. This is required so the generated temporary password stays on screen after a reset until the admin navigates away.
- **Confirmation steps:** reuse the inline "Are you sure?" confirm pattern from `Features/Orders/List/ListPage.razor` for the password reset **and** for both the promote and demote actions (not a modal dialog — the app uses inline conditional confirm/cancel buttons).
- **Inline email edit:** mirror the `ProfilePage` `EditForm` + `DataAnnotationsValidator` + `InputText`/`ValidationMessage` pattern for the inline email edit section.

### Dependencies & open questions (resolve with story owner; do not design here)

- **Authorization:** these pages must be admin-gated. This depends on story 01-admin-area-setup and the project's auth scheme, which is still undecided (Azure Entra was denied). Do not design admin gating in this story.
- **Email-change side effects:** email is the login identity — confirm whether changing it should invalidate active sessions or trigger any notification.
- **Audit logging** of password resets and role changes is out of scope unless the story owner says otherwise.

## Acceptance Criteria

- [ ] Admin can edit a customer's email address; the change is saved and reflected immediately in the customer list and profile view
- [ ] Validation prevents saving an empty or malformed email address
- [ ] If the new email address is already in use by another customer, the save is rejected with an error message
- [ ] Admin can reset a customer's password; a confirmation prompt is shown before the reset takes effect
- [ ] On confirmation, the system generates a temporary password and displays it to the admin
- [ ] Admin can promote a customer to admin (`IsAdmin = true`); a confirmation step is shown before it takes effect
- [ ] Admin can demote an admin back to customer (`IsAdmin = false`); a confirmation step is shown before it takes effect
- [ ] An admin cannot remove their own admin rights; the demote action is disabled on the currently logged-in user's profile
- [ ] All three actions are accessible from the customer profile view
- [ ] The profile view has a persistent "Back to Customer List" link that returns to the customer list
- [ ] After any action completes, the admin remains on the profile view with an inline success alert
- [ ] The generated temporary password remains visible on the profile until the admin navigates away
- [ ] A success confirmation is shown after each action completes
- [ ] If an action fails, an error message is displayed and no changes are saved

## Refinement Notes

Password reset uses a system-generated temporary password displayed directly to the admin — no email delivery in scope. A confirmation step is required before the reset takes effect, per user request.

An admin cannot demote themselves — this covers both the "demoting yourself" and "demoting the last admin" concerns in a single simple rule. No need to track admin count.

The temporary password generated here is a plain password — it works like any other password until the customer changes it themselves. Forcing the customer to reset it on next login was requested but deferred to its own story (06-force-temp-password-reset) so this story stays self-contained.

Navigation and post-action UX were clarified during refinement: a persistent "Back to Customer List" link on the profile, all actions keeping the admin on the profile with an inline success alert, inline edit-in-place for email, and a confirmation step on promote and demote as well as password reset.
