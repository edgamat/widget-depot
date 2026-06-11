---
status: shared
milestone: 4
github_issue: 142
task_issues: []
---

# Admin: Unified Layout with Role-Gated Navigation

## User Story

> As **WDI Staff (Admin)**,
> I want the admin features to live inside the same layout as the rest of the site,
> so that there is one consistent experience and admin tools are reachable from the
> normal navigation rather than a separate-looking admin section.

## Background

Story 01 ("Admin: Area Setup") introduced a distinct `AdminLayout.razor` with its own dark
sidebar and hardcoded navigation, applied to all `/admin` pages via `Features/Admin/_Imports.razor`,
plus a post-login redirect to `/admin`. After a demo, the product owner decided a separate admin
look adds no value. Admins and customers should share the existing main layout; admin-only
navigation should be hidden from non-admins and shown to admins.

The `IsAdmin` flag, the `IsAdmin` authorization policy, and admin seeding from story 01 are
unchanged. This story is purely about presentation and navigation — page-level authorization on
admin routes stays in force.

## Scope

**In scope:**
- Surface admin navigation inside the shared `NavMenu.razor` as a grouped "Admin" section
  (visual divider + small "Admin" heading), wrapped in `<AuthorizeView Policy="IsAdmin">` so it
  renders only for admins.
- Admin-only links in that group: **Users** (`admin/users`) and **Catalog Upload**
  (`admin/catalog-import`). "Order Lookup" stays in the regular authenticated link list (it is
  available to all authenticated users) and is not duplicated in the admin group.
- Stop applying a separate layout to admin pages: remove `@layout AdminLayout` from
  `Features/Admin/_Imports.razor` so admin pages fall back to the shared (main) layout. Keep the
  `[Authorize(Policy = "IsAdmin")]` attribute on that `_Imports.razor`.
- Remove the post-login redirect to `/admin`; admins land on the regular home page (`/`) like
  every other user.
- Show a small "Admin" badge near the signed-in user's name/email in the shared layout, visible
  only to admins.
- Move the css for the nav links from `AdminLayout.razor.css` to `NavMenu.razor.css` and then delete the now-unused `AdminLayout.razor` and `AdminLayout.razor.css`.
- Remove `AdminHomePage.razor` (the `/admin` landing page), since admins no longer land there and
  it provided no real content.

**Out of scope:**
- Any change to the `IsAdmin` flag, the `IsAdmin` policy, claims population at sign-in, or admin
  seeding.
- Any change to which routes are admin-protected (admin pages remain gated by the policy).
- Building the not-yet-implemented `admin/users` page (the link may point to its planned route).
- Restyling the shared layout beyond adding the admin nav group and the admin badge.

## Developer Notes

- The admin nav group belongs in `Components/Layout/NavMenu.razor`. Use
  `<AuthorizeView Policy="IsAdmin">` (policy-based), not a bare `<AuthorizeView>` (auth-only).
  Place it inside / after the existing `<Authorized>` block so it only shows for signed-in admins.
- After removing `@layout AdminLayout`, confirm admin pages render under the default layout set in
  `Routes.razor`/`App.razor`. The `_Imports.razor` keeps only the `@using` lines it still needs
  and the `[Authorize(Policy = "IsAdmin")]` attribute.
- The post-login redirect lives in the `/accounts/do-signin` endpoint in `Program.cs` (it
  currently redirects to `/admin` when `isAdmin`). Change it to send all users to `/` (or their
  return URL). The `isAdmin` claim is still added — only the redirect target changes.
- The "Admin" badge markup currently lives in `AdminLayout.razor`; relocate an equivalent small
  badge into the shared layout (e.g. `MainLayout.razor`) next to the username, gated by
  `<AuthorizeView Policy="IsAdmin">`.
- Deleting `AdminLayout.razor`/`.css` and `AdminHomePage.razor`: grep for any remaining references
  (e.g. `@layout AdminLayout`, links to `/admin`) before removing, and update or remove them.
- Update/extend tests: any E2E or unit tests asserting the separate admin layout, the `/admin`
  landing redirect, or the old admin nav need to be updated to assert the unified layout, the
  role-gated nav group, and the home-page landing instead.

## Acceptance Criteria

- [ ] Admin and non-admin users share the same site layout; there is no separate admin layout.
- [ ] An authenticated admin sees a grouped "Admin" navigation section (Users, Catalog Upload)
      within the shared nav.
- [ ] A non-admin authenticated user does not see the admin navigation section or its links.
- [ ] An unauthenticated user does not see the admin navigation section.
- [ ] Admin routes (e.g. `/admin/catalog-import`) remain protected by the `IsAdmin` policy:
      non-admins are still denied access if they navigate directly.
- [ ] After login, an admin lands on the regular home page (`/`), not `/admin`.
- [ ] A small "Admin" badge is shown near the signed-in admin's name; non-admins see no badge.
- [ ] `AdminLayout.razor`, `AdminLayout.razor.css`, and `AdminHomePage.razor` are removed, with no
      dangling references.
- [ ] The app builds and existing tests pass; tests tied to the old admin layout/redirect are
      updated to the unified behavior.

## Refinement Notes

This story supersedes the visual-distinctness decision from story 01. The product owner confirmed
after a demo that a distinct admin look adds no value; admins and customers share one layout, with
admin nav hidden from non-admins. Order Lookup stays in the common authenticated nav (not the admin
group). A small "Admin" badge is retained so an elevated account is identifiable. Decisions
accepted by the user.
