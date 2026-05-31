import path from 'node:path';
import { test, expect } from './fixtures';

const data = (file: string) => path.resolve(__dirname, 'data', file);

test.describe('Catalog import page', () => {
  // AC1: A warehouse staff member can navigate to the admin catalog import page
  test('can navigate to the catalog import page', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await expect(catalogImportPage.page).toHaveURL(/\/admin\/catalog-import/);
    await expect(catalogImportPage.heading).toBeVisible();
  });

  // AC2: The page displays a file upload control that accepts .csv files and an Import button
  test('displays a CSV file input and an Import button', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await expect(catalogImportPage.fileInput).toBeVisible();
    await expect(catalogImportPage.fileInput).toHaveAttribute('accept', '.csv');
    await expect(catalogImportPage.importButton).toBeVisible();
  });

  // AC3 + AC4: Uploading a valid CSV inserts new rows and shows the inserted/updated/skipped summary
  test('valid CSV import shows an inserted/updated/skipped summary', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await catalogImportPage.uploadAndImport(data('valid-import.csv'));

    await expect(catalogImportPage.successAlert).toBeVisible();
    await expect(catalogImportPage.successAlert).toContainText('Import complete.');
    await expect(catalogImportPage.successAlert).toContainText('Inserted:');
    await expect(catalogImportPage.successAlert).toContainText('Updated:');
    await expect(catalogImportPage.successAlert).toContainText('Skipped:');
    await expect(catalogImportPage.errorAlert).not.toBeVisible();
  });

  // AC5: Rows with a missing or blank SKU, Name, or Price are skipped
  test('rows with missing SKU, Name, or Price are skipped', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await catalogImportPage.uploadAndImport(data('partial-invalid.csv'));

    await expect(catalogImportPage.successAlert).toBeVisible();
    await expect(catalogImportPage.successAlert).toContainText('Skipped: 2');
  });

  // AC6: Invalid CSV or missing expected columns shows an error and writes no data
  test('invalid CSV with wrong column headers shows an error', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await catalogImportPage.uploadAndImport(data('wrong-headers.csv'));

    await expect(catalogImportPage.errorAlert).toBeVisible();
    await expect(catalogImportPage.successAlert).not.toBeVisible();
  });

  // AC7: CSV where every row fails validation shows the summary with zero records imported
  test('CSV where every row is invalid shows zero inserted and zero updated', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await catalogImportPage.uploadAndImport(data('all-invalid-rows.csv'));

    await expect(catalogImportPage.successAlert).toBeVisible();
    await expect(catalogImportPage.successAlert).toContainText('Inserted: 0');
    await expect(catalogImportPage.successAlert).toContainText('Updated: 0');
  });

  // AC8: Uploading the same CSV a second time results in zero insertions and all updates (idempotent)
  test('re-importing the same CSV results in zero insertions and all updates', async ({ catalogImportPage }) => {
    await catalogImportPage.goto();
    await catalogImportPage.uploadAndImport(data('idempotent-import.csv'));
    await expect(catalogImportPage.successAlert).toBeVisible();

    // Second upload of the same file — all SKUs now exist, so inserts become updates
    await catalogImportPage.uploadAndImport(data('idempotent-import.csv'));
    await expect(catalogImportPage.successAlert).toBeVisible();
    await expect(catalogImportPage.successAlert).toContainText('Inserted: 0');
    await expect(catalogImportPage.successAlert).toContainText('Updated: 2');
    await expect(catalogImportPage.successAlert).toContainText('Skipped: 0');
  });
});
