---
status: draft
milestone: 4
github_issue:
task_issues: []
---

# Admin: Catalog CSV Upload

## User Story

> As a **WDI Staff (Admin)**,
> I need to upload a catalog CSV file from within the admin area,
> in order to update the widget catalog without needing CLI access or developer involvement.

## Background

The catalog CSV upload functionality was implemented in Milestone 1 (Story 2). That page exists outside the secured admin area. This story moves it into `Features/Admin/` so it is protected by admin-only access and accessible from the admin navigation.

The CSV format is a hard constraint defined by the warehouse team and must not change (PRD §7.2).

## Scope

**In scope:**
- Move the existing catalog CSV upload page into `Features/Admin/`
- Page is protected by the admin authorization policy established in story 01-admin-area-setup
- Page is accessible from the admin navigation

**Out of scope:**
- Any changes to the upload or catalog replacement logic
- Any changes to the CSV format

## Developer Notes

- The existing upload logic should be reused as-is. This story is a relocation, not a rewrite.
- Verify the page still functions correctly after the move (routing, service calls, error handling).

## Acceptance Criteria

- [ ] The catalog CSV upload page is accessible from the admin navigation
- [ ] The page is located under `Features/Admin/` and protected by the admin authorization policy
- [ ] Non-admin users cannot access the upload page directly via URL
- [ ] The upload functionality works correctly after the move (a valid CSV updates the catalog; an invalid CSV shows an error)

## Refinement Notes

This story is a relocation of existing functionality, not new development. The upload logic, CSV format, and success/error behaviour are unchanged from Milestone 1.
