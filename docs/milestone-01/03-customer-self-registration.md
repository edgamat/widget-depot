# Customer self-registration

## User Story

> As a **visitor**,
> I need to create an account by providing my profile details and a password,
> in order to gain access to ordering and other authenticated features.

## Background

Widget Depot requires customers to have an account to place orders. Visitors can browse the catalog
without logging in, but must register before they can order. Registration collects the basic profile
details needed to fulfill orders: name, email, and a password. The email address serves as the
unique identifier for the account and is used to log in.

A `Customer` entity must be introduced to the data model. Passwords are stored as a hashed value —
plain-text passwords must never be persisted.

## Scope

**In scope:**
- A publicly accessible registration page at `/accounts/register`
- A registration form collecting: first name, last name, email address, password, and confirm password
- Client-side and server-side validation of all fields
- Server-side check that the email address is not already registered
- Password hashing before storage (e.g. `PasswordHasher<Customer>` from ASP.NET Core Identity)
- On successful registration, redirect the visitor to the login page with a confirmation message
- A link to the registration page visible on the login page

**Out of scope:**
- Email verification / confirmation link
- Social or federated login (Azure Entra is wired up separately and bypassed locally)
- Admin-created or bulk-provisioned accounts
- Password reset / forgot password (separate story)
- Phone number or shipping address collection at registration time (covered in story 05)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Accounts/Register/`
- New API endpoint: `WidgetDepot.ApiService/Features/Accounts/Register/`
- New `Customer` entity in `WidgetDepot.ApiService/Data/Customer.cs`; add `DbSet<Customer>` to `AppDbContext` and create a new EF Core migration
- `Customer` fields: `Id` (int), `FirstName`, `LastName`, `Email` (unique index), `PasswordHash`, `CreatedAt`
- Use `Microsoft.AspNetCore.Identity.PasswordHasher<Customer>` for hashing — no additional Identity infrastructure required
- The registration API endpoint should return a typed error (e.g. `EmailAlreadyRegistered`) rather than a generic 400, so the Blazor page can show a specific message
- Auth is bypassed locally; the registration page does not need an auth guard
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout; no MediatR

## Acceptance Criteria

- [ ] A visitor can navigate to `/accounts/register` without being redirected to login
- [ ] The registration form displays fields for: first name, last name, email address, password, and confirm password
- [ ] Submitting the form with any required field empty shows a validation message for that field and does not submit
- [ ] The email field is validated to be a well-formed email address
- [ ] The password must be at least 8 characters long; submitting a shorter password shows a validation error
- [ ] The confirm password field must match the password field; a mismatch shows a validation error
- [ ] Submitting the form with an email address already associated with an existing account shows an error message indicating the email is already in use
- [ ] On successful registration, the customer record is created with a hashed password (never plain text) and the visitor is redirected to the login page
- [ ] A "Create an account" (or equivalent) link on the login page navigates to the registration page
