# Initial catalog CSV import

## User Story

> As a **warehouse staff member**,
> I need to upload the widget catalog from a CSV file,
> in order to populate the site with product data at launch.

## Background

Widget Depot does not manage catalog data directly — the source of truth is the ERP system, which
exports widget data as a daily CSV batch via FTP. This story covers the initial import capability:
a staff-facing upload page that parses an ERP-exported CSV and upserts widgets into the database.

The CSV format is fixed by the ERP and contains these columns (in order):
`SKU, Name, Description, Manufacturer, Weight, Price, Date Available`

The `Widget` entity is already defined in `WidgetDepot.ApiService/Data/Widget.cs`.

## Scope

**In scope:**
- An authenticated admin page with a file upload control for `.csv` files
- CSV parsing and per-row validation
- Upsert logic: insert new widgets; update existing widgets matched by SKU
- An import summary displayed after upload (rows inserted, rows updated, rows skipped with reasons)

**Out of scope:**
- Scheduled or automated import (future story)
- Changes to the CSV format or ERP export structure
- Bulk delete or catalog reset
- Partial rollback on error (failed rows are skipped; valid rows are committed)

## Developer Notes

- New feature slice: `WidgetDepot.Web/Features/Admin/CatalogImport/`
- New API endpoint in `WidgetDepot.ApiService/Features/Widgets/Import/`
- The import endpoint receives a multipart form upload and returns a summary DTO
- Use EF Core upsert via `ExecuteUpdateAsync` or `AddOrUpdate` pattern keyed on `Sku`
- CSV parsing: use `CsvHelper` or plain `StreamReader` line-by-line — no third-party dependency required for a fixed-format file
- Auth is bypassed locally; the page does not need an auth guard for this milestone
- The `DateAvailable` column maps to `DateOnly` — parse with `DateOnly.Parse`

## Acceptance Criteria

- [ ] A warehouse staff member can navigate to an admin catalog import page
- [ ] The page displays a file upload control that accepts `.csv` files and an "Import" button
- [ ] Uploading a valid CSV file triggers an upsert: new SKUs are inserted, existing SKUs are updated
- [ ] After a successful import, the page displays a summary: number of rows inserted, number updated, and number skipped
- [ ] Rows with a missing or blank SKU, Name, or Price are skipped and included in the skipped count
- [ ] If the uploaded file is not valid CSV or is missing expected columns, an error message is shown and no data is written
- [ ] Uploading a CSV where every row fails validation shows an appropriate error message (zero records imported)
- [ ] Uploading the same CSV a second time results in zero insertions and the same number of updates as there are rows (idempotent)
