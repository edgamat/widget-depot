---
status: shared
milestone: 4
github_issue: 138
task_issues: []
---

# Admin: Catalog CSV Upload

## User Story

> As a **WDI Staff (Admin)**,
> I need to upload a catalog CSV file from within the admin area,
> in order to update the widget catalog without needing CLI access or developer involvement.

## Background

The catalog CSV upload functionality was implemented in Milestone 1 (Story 2). The page (`CatalogImportPage.razor`) already lives at `Features/Admin/CatalogImport/`. This story ensures the page is protected by the admin authorization policy and is accessible from the admin navigation.

The CSV format is a hard constraint defined by the warehouse team and must not change (PRD §7.2).

## Scope

**In scope:**
- Page is protected by the admin authorization policy established in story 01-admin-area-setup
- Page is accessible from the admin navigation

**Out of scope:**
- Any changes to the upload or catalog replacement logic
- Any changes to the CSV format
- Relocating the page (it is already in `Features/Admin/CatalogImport/`)

## Developer Notes

- `CatalogImportPage.razor` already exists at `Features/Admin/CatalogImport/` — no move is needed.
- Apply the admin authorization policy to the existing page and add it to the admin navigation.
- Verify the page still functions correctly (routing, service calls, error handling).

## Acceptance Criteria

- [ ] The catalog CSV upload page is accessible from the admin navigation
- [ ] The page is protected by the admin authorization policy
- [ ] Non-admin users cannot access the upload page directly via URL
- [ ] The upload functionality works correctly (a valid CSV updates the catalog; an invalid CSV shows an error)

## Refinement Notes

The page already lives in `Features/Admin/CatalogImport/`. The remaining work is wiring up auth policy enforcement and the admin nav link.
