# Customer password change

## User Story

> As a **registered customer**,
> I need to change my password,
> in order to maintain the security of my account.

## Background

Customers log in with an email and password. Once logged in they can view and
edit their profile, but have no way to change their password. This story adds a
dedicated password change page where a customer can update their password by
providing their current password and choosing a new one.

## Scope

**In scope:**
- A protected password change page at `/accounts/password` (redirect to login if unauthenticated)
- Form fields: Current Password, New Password, Confirm New Password
- Validate that current password is correct before applying the change
- Validate that New Password and Confirm New Password match
- Validate minimum password length (8 characters)
- A "Change Password" link on the Profile page (`/accounts/profile`)

**Out of scope:**
- Forgot password / password reset via email
- Password strength meter or complexity rules beyond minimum length
- Forced password expiry

## Developer Notes

- **Customer entity:** `src/WidgetDepot.ApiService/Data/Customer.cs`
  — has `PasswordHash`; use `PasswordHasher` (already used in Register) to
  verify the current password and hash the new one
- **Feature folder:** `src/WidgetDepot.Web/Features/Accounts/PasswordChange/`
  — `PasswordChangePage.razor` (InteractiveServer render mode, `[Authorize]`)
  — `PasswordChangeService.cs` (inject `HttpClient`, return discriminated union result)
  — `PasswordChangeModels.cs` (DataAnnotations validation, `[Compare]` for confirmation field)
- **API endpoint:** `src/WidgetDepot.ApiService/Features/Accounts/PasswordChange/`
  — `PUT /accounts/password` — accepts `{ CurrentPassword, NewPassword }`; identifies
  the customer from `ClaimTypes.NameIdentifier` in the request's auth cookie
  — Returns `204 No Content` on success, `409 Conflict` with `{ "errorCode": "IncorrectPassword" }` if current password does not match
- **No cookie refresh needed** — password change does not affect auth claims
- **No DB migration needed** — `PasswordHash` column already exists
- Follow `ProfilePage` / `ProfileService` / `ProfileModels` patterns for form
  structure, service shape, and discriminated union results
- Add a "Change Password" link to `ProfilePage.razor` beneath the Save button

## Acceptance Criteria

- [ ] An authenticated customer can navigate to `/accounts/password` and see a
      form with Current Password, New Password, and Confirm New Password fields
- [ ] An unauthenticated visitor who navigates to `/accounts/password` is
      redirected to the login page
- [ ] Submitting with any field blank shows a required-field validation error
- [ ] Submitting with New Password and Confirm New Password that do not match
      shows a validation error
- [ ] Submitting with a New Password shorter than 8 characters shows a
      validation error
- [ ] Submitting with an incorrect Current Password shows an error message
- [ ] Submitting with a valid Current Password and matching New Password
      updates the password and shows a success confirmation
- [ ] After a successful change, the customer remains logged in (no re-login required)
- [ ] The Profile page includes a "Change Password" link to `/accounts/password`
- [ ] Unit tests are written for the PasswordChangeService and the API handler
