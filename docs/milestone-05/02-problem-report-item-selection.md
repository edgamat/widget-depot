---
status: shared
milestone: 5
github_issue: 157
task_issues: []
---

# Problem Report: Item Selection and Submission

## User Story

> As a **customer**,
> I need to select the affected items from my order, specify the issue type for each, and submit the problem report,
> in order to notify warehouse staff of the exact nature of the problem.

## Background

This is the second step of the problem reporting wizard, following the order lookup (story 1). The customer selects one or more affected items, picks an issue type for each, optionally adds notes for the whole report, and submits. Submission saves the problem report to the database; the email notification is handled separately (story 3).

Multiple problem reports can be filed against the same order.

## Scope

**In scope:**
- Display of order line items with checkboxes for selection
- Issue type selection per selected item (under-requested, over-requested, damaged)
- Optional free-text notes field for the whole report (not per item)
- Saving the completed problem report to the database on submission
- Confirmation feedback after successful submission

**Out of scope:**
- Email notification (story 3)
- Editing or deleting a submitted problem report
- A "My Problem Reports" history view (deferred — to be discussed after milestone interviews)

## Developer Notes

The wizard uses a two sub-step flow within this screen:

1. **Step A — Item selection:** All order line items are displayed with checkboxes. The customer checks the affected items and clicks "Next". At least one item must be selected to proceed.
2. **Step B — Issue specification:** Only the selected items are shown, each with a required issue type dropdown (under-requested, over-requested, damaged). A single optional notes field for the whole report appears below the items. The customer clicks "Submit" to save the report.

- Notes apply to the whole report, not to individual items.
- Multiple reports against the same order are permitted.

## Acceptance Criteria

- [ ] Step A displays all order line items with checkboxes
- [ ] Clicking "Next" on Step A with no items selected shows a validation error
- [ ] Clicking "Next" with at least one item selected advances to Step B
- [ ] Step B displays only the selected items, each with an issue type dropdown (under-requested, over-requested, damaged)
- [ ] Step B includes a single optional free-text notes field for the whole report
- [ ] Clicking "Submit" on Step B with any issue type unset shows a validation error
- [ ] On valid submission, the problem report (order reference, affected items, issue types, notes) is saved to the database
- [ ] After successful submission, the customer sees a confirmation message
- [ ] Multiple problem reports can be submitted against the same order

## Refinement Notes

Notes are scoped to the whole report, not per item — confirmed with user. Multiple reports per order are explicitly permitted.

The two sub-step flow (select items → specify issue types) was chosen over a single dynamic-reveal page to match the PRD's sequential framing and keep each step focused.

"My Problem Reports" history view was raised during refinement and added as story 4.
