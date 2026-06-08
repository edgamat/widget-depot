---
status: draft
milestone: 4
github_issue:
task_issues: []
---

# Admin: Customer List

## User Story

> As a **WDI Staff (Admin)**,
> I need to view a list of registered customers and their profile details,
> in order to find and review customer information quickly.

## Background

This is the read-only view of customer data in the admin area. Editing actions (email, password, admin rights) are covered in the Admin: Customer Management story. The customer list is the entry point for all customer-related admin work.

## Scope

**In scope:**
- Paginated list of all registered customers showing first name, last name, and email address
- List sorted by last name
- Compact table layout to minimize vertical scrolling
- Pagination controls: First, Previous, Next, Last; 20 customers per page
- Empty state when no customers are registered
- Read-only profile view for an individual customer showing their full profile details
- Profile view indicates whether the customer has admin rights

**Out of scope:**
- Search or filtering the customer list (deferred to a later milestone)
- Any editing actions (covered in story 03-admin-customer-management)

## Developer Notes

- The list and profile view live under `Features/Admin/` and use the admin layout.

## Acceptance Criteria

- [ ] Admin can navigate to the customer list from the admin navigation
- [ ] The list displays each customer's first name, last name, and email address
- [ ] The list is sorted by last name
- [ ] The list is paginated with 20 customers per page
- [ ] Pagination controls include First, Previous, Next, and Last buttons
- [ ] The table uses a compact layout to minimize vertical scrolling
- [ ] If there are no registered customers, the list shows an empty state message
- [ ] Clicking a customer opens a read-only profile view showing their full profile details
- [ ] The profile view includes a visible indicator if the customer has admin rights

## Refinement Notes

Search and filtering are explicitly deferred to a later milestone — confirmed with user. Pagination style (First/Prev/Next/Last, 20 per page) and compact table layout specified by the user to reduce scrolling on busy lists.
