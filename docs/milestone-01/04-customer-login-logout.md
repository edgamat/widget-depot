# Customer login and logout

## User Story

> As a **registered customer**,
> I need to log in and out of my account,
> in order to access features that require authentication.

## Background

Customers register with an email address and password (story #3). Their credentials are stored in the `Customer` table with a hashed password. This story implements the login and logout flows so authenticated customers can access protected features in future stories.

Authentication uses ASP.NET Core cookie authentication. The login form submits credentials to a `POST /accounts/login` API endpoint; on success the Web app signs the customer in via a cookie. Logout clears the cookie and redirects to the home page.

Azure Entra authentication is wired for production but bypassed locally. This story does not change that configuration.

## Scope

**In scope:**
- Login page (`/accounts/login`) — email/password form with validation
- `POST /accounts/login` API endpoint that validates credentials and returns customer info
- ASP.NET Core cookie authentication middleware wired into both projects
- Logout action that signs the customer out and redirects to `/`
- Redirect unauthenticated users to `/accounts/login` when accessing a protected route (wired up but no protected routes yet)
- "Invalid email or password" error message on failed login (no credential enumeration)
- Customer's email address is dsplayed to the left of the "About" link after then have signed in.

**Out of scope:**
- "Remember me" / persistent sessions
- Password reset or forgot-password flow
- Account lockout after failed attempts
- Two-factor authentication
- Azure Entra integration changes

## Developer Notes

- Relevant files:
  - `src/WidgetDepot.Web/Features/Accounts/Login/LoginPage.razor` — existing stub to complete
  - `src/WidgetDepot.ApiService/Features/Accounts/` — add `Login/` slice alongside existing `Register/`
  - `src/WidgetDepot.ApiService/Features/Accounts/AccountEndpointExtensions.cs` — register the new endpoint
- Patterns to follow:
  - Mirror the `Register` vertical slice: `LoginEndpoint.cs`, `LoginHandler.cs`, models, `LoginService.cs`, updated `LoginPage.razor`
  - Use `PasswordHasher<Customer>` (already used in `RegisterHandler`) to verify passwords
  - Return a generic "invalid credentials" response for both bad email and bad password — do not distinguish between them
  - Use discriminated union result type (abstract record pattern) as in `RegisterHandler`
- Cookie auth: configure `AddAuthentication().AddCookie()` in both `WidgetDepot.Web` and `WidgetDepot.ApiService` `Program.cs` files
- The login API endpoint should return customer id, first name, and email so the Web layer can build the `ClaimsPrincipal`

## Acceptance Criteria

- [ ] A customer with valid credentials can log in and is redirected to the home page
- [ ] An incorrect email or password shows "Invalid email or password" without indicating which field is wrong
- [ ] A logged-in customer can log out; they are redirected to `/` and can no longer access authenticated routes
- [ ] The login form validates that email and password fields are not empty before submitting
- [ ] Unit tests cover: successful login, invalid password, unknown email, and empty field validation
- [ ] Customer's email address is dsplayed in page header after then have signed in.