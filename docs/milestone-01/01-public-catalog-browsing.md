# Public widget catalog browsing

## User Story

> As a **visitor**,
> I need to browse the widget catalog without logging in,
> in order to explore available products before deciding to register or place an order.

## Background

The widget catalog is the primary public-facing feature of Widget Depot. Visitors must be able to
explore available products without needing an account. The catalog is populated from CSV files
managed by warehouse staff — the website is not the source of truth for catalog data (see story 02).

## Scope

**In scope:**
- A publicly accessible catalog page (no authentication required)
- Search by keyword (contains match on widget name and description)
- Inline display of all widget fields for each result

**Out of scope:**
- Ordering or add-to-cart controls (requires authentication, covered in Milestone 2)
- Pagination
- Filtering by manufacturer, price range, or other fields
- Sorting
- A separate widget detail page

## Developer Notes

- Relevant file(s): `src/...`
- Patterns to follow: Vertical Slice Architecture; EF Core for data access; Bootstrap 5.3 for layout
- The search query should use a SQL `LIKE`/`ILIKE` contains match (e.g. `%term%`) across both `Name` and `Description` columns
- The catalog page is a Blazor component with no auth guard
- Constraints / things not to change: the widget data model must accommodate all fields from the warehouse CSV (SKU, Name, Description, Manufacturer, Weight, Price, Date Available)

## Acceptance Criteria

- [ ] An unauthenticated visitor can navigate to the catalog page without being redirected to login
- [ ] On initial page load, no widgets are displayed and no search has been executed
- [ ] A text search box and a "Search" button are visible on the catalog page
- [ ] When the visitor enters a search term and presses "Search", the catalog displays all widgets whose name or description contains the search term (case-insensitive)
- [ ] Each result displays: SKU, Name, Description, Manufacturer, Weight (kg), Price, and Date Available
- [ ] When a search returns no results, a message is shown indicating no records matched the search criteria
- [ ] Clearing the search term and pressing "Search" again returns no results (the page returns to the empty initial state)
