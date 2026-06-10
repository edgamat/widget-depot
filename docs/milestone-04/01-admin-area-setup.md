---
status: draft
milestone: 4
github_issue:
task_issues: []
---

# Admin: Area Setup

## User Story

> As a **WDI Staff (Admin)**,
> I need to access a secured admin area of the site,
> in order to perform administrative tasks that are not available to customers.

## Background

Authentication uses ASP.NET Identity with cookie auth. The `Features/Admin/` folder is already part of the planned architecture but currently empty. Admin access is determined by an `IsAdmin` flag on the customer record rather than an ASP.NET Identity role.

Admin access is session-bound: if the `IsAdmin` flag is revoked in the database, the change takes effect when the user's next session begins, not immediately.

## Scope

**In scope:**
- Add `IsAdmin` flag to the customer/user record; existing customers default to `false`
- EF Core migration for the new flag
- Authorization policy that reads `IsAdmin` and gates access to admin routes
- `_Imports.razor` in `Features/Admin/` applying the policy to all admin pages
- Non-admin authenticated users navigating to any `/admin` route are redirected to an access-denied page
- Unauthenticated users navigating to any `/admin` route are redirected to the login page
- `AdminLayout.razor` in `Components/` with admin navigation (links to Users, Order Lookup, Catalog Upload)
- Data migration to seed one initial admin user with credentials sourced from `appsettings.json`

**Out of scope:**
- Actual admin feature pages (covered in stories 2–4)
- Any changes to the existing customer authentication flow
- Real-time revocation of admin access during an active session

## Developer Notes

- The seed migration must guard against duplicate user creation — check before inserting.
- If the admin seed config is absent from `appsettings.json`, skip seeding and log a warning. The app must still start successfully.
- The admin layout should be visually distinct from the main site layout (e.g., different header/nav).
- Add the following to `appsettings.json` (or `appsettings.Development.json`) to enable admin seeding:

```json
{
  "Admin": {
    "SeedCredentials": {
      "UserName": "admin",
      "Password": "P@ssw0rd"
    }
  }
}
```

## Acceptance Criteria

- [ ] Customer record has an `IsAdmin` flag; existing customers default to `false`
- [ ] A data migration seeds one admin user with `IsAdmin = true`, with credentials sourced from `appsettings.json`
- [ ] The seed migration does not create a duplicate user if run more than once
- [ ] If admin seed config is absent from `appsettings.json`, seeding is skipped and a warning is logged; the app starts normally
- [ ] Authenticated non-admin users who navigate to any `/admin` route are redirected to an access-denied page
- [ ] Unauthenticated users who navigate to any `/admin` route are redirected to the login page
- [ ] An authenticated admin user can reach the admin area and sees the admin navigation (links to Users, Order Lookup, Catalog Upload)
- [ ] The admin layout is visually distinct from the main site layout

## Refinement Notes

Admin access is session-bound: revoking `IsAdmin` in the database takes effect when the user's session ends, not immediately. This is the intended behavior — accepted by the user.

Seeded admin credentials are driven from `appsettings.json` to support per-environment configuration. Missing config causes a logged warning and skipped seeding (not a startup failure), so environments that don't need a seeded admin are not blocked.
