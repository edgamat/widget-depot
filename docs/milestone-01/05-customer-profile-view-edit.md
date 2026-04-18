# Customer profile view and edit

## User Story

> As a **registered customer**,
> I need to view and update my profile details,
> in order to keep my account information accurate.

## Background

Customers register with a first name, last name, and email address. Once
logged in, they can see their email in the page header, but have no way to
view or change their profile information. This story adds a profile page
where a customer can review and edit their name and email.

## Scope

**In scope:**
- A protected profile page at `/accounts/profile` (redirect to login if unauthenticated)
- Display current FirstName, LastName, and Email
- Allow editing FirstName, LastName, and Email
- Validate input (required fields, valid email format, email uniqueness)
- On successful save, refresh the auth cookie so the updated name/email is
  reflected in the page header without requiring a re-login
- A "Profile" link in the nav menu, visible only when authenticated

**Out of scope:**
- Password change (separate story)
- Account deletion
- Additional profile fields (phone, address, etc.)

## Developer Notes

- **Customer entity:** `src/WidgetDepot.ApiService/Data/Customer.cs`
  — has `FirstName`, `LastName`, `Email`, `PasswordHash`, `CreatedAt`
- **Auth claims:** Logged-in customer is identified via
  `ClaimTypes.NameIdentifier` (CustomerId), `ClaimTypes.Email`, and
  `ClaimTypes.Name` (FirstName) stored in the cookie
- **Existing patterns to follow:**
  - Feature folder layout: `src/WidgetDepot.Web/Features/Accounts/Profile/`
  - Page: `ProfilePage.razor` (InteractiveServer render mode, `[Authorize]`)
  - Service: `ProfileService.cs` (inject `HttpClient`, return discriminated union results)
  - Models: `ProfileModels.cs` (DataAnnotations validation)
  - API endpoint: `src/WidgetDepot.ApiService/Features/Accounts/Profile/`
    — GET `/accounts/profile` returns current values
    — PUT `/accounts/profile` applies updates
  - Follow `LoginPage` / `RegisterPage` for form structure
    (`EditForm` + `DataAnnotationsValidator` + `InputText` + `ValidationMessage`)
- **Auth cookie refresh:** After a successful update the Web app must call
  `HttpContext.SignInAsync()` with updated claims (same pattern used in
  `do-signin` endpoint in `src/WidgetDepot.Web/Program.cs`)
- **Email uniqueness:** Email has a unique index
  (`src/WidgetDepot.ApiService/Data/AppDbContext.cs`); the PUT handler must
  detect duplicate-email conflicts and return an appropriate error result
- **Nav menu:** Add a "Profile" link to
  `src/WidgetDepot.Web/Components/Layout/NavMenu.razor` inside the
  `<AuthorizeView>` block (alongside the existing "Sign Out" link)
- **No DB migration needed** — no new columns required

## Acceptance Criteria

- [ ] An authenticated customer can navigate to `/accounts/profile` and see
      their current FirstName, LastName, and Email
- [ ] An unauthenticated visitor who navigates to `/accounts/profile` is
      redirected to the login page
- [ ] The customer can edit their FirstName, LastName, and/or Email and save
      successfully
- [ ] Saving with a blank FirstName, LastName, or Email shows a validation error
- [ ] Saving with an invalid email format shows a validation error
- [ ] Saving with an email already used by another customer shows an error
- [ ] After a successful save, the header reflects any name/email change
      without requiring re-login
- [ ] A "Profile" link appears in the nav menu for authenticated customers
- [ ] Unit tests are written for the ProfileService and the API handler
