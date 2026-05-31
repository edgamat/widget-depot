import { test, expect } from './fixtures';

test.describe('Catalog page', () => {
  // AC #1: Unauthenticated visitor can navigate to catalog without redirect
  test('unauthenticated visitor can access catalog without being redirected to login', async ({ catalogPage }) => {
    await catalogPage.goto();
    await expect(catalogPage.page).toHaveURL(/\/catalog/);
    await expect(catalogPage.page).toHaveTitle(/Widget Catalog/);
  });

  // AC #2: On initial page load, no widgets displayed and no search executed
  test('shows no results and no message on initial load', async ({ catalogPage }) => {
    await catalogPage.goto();
    await expect(catalogPage.resultsTable).not.toBeVisible();
    await expect(catalogPage.noResultsMessage).not.toBeVisible();
  });

  // AC #3: Search box and Search button are visible
  test('displays search input and Search button', async ({ catalogPage }) => {
    await catalogPage.goto();
    await expect(catalogPage.searchInput).toBeVisible();
    await expect(catalogPage.searchButton).toBeVisible();
  });

  // AC #4: Search returns matching widgets (case-insensitive)
  test('returns matching widgets for a case-insensitive search term', async ({ catalogPage }) => {
    await catalogPage.goto();
    await catalogPage.search('flux');
    await expect(catalogPage.firstResultRow()).toBeVisible();
  });

  // AC #5: Each result displays SKU, Name, Description, Manufacturer, Weight, Price, Date Available
  test('displays all required columns for each result', async ({ catalogPage }) => {
    await catalogPage.goto();
    await catalogPage.search('Flux Capacitor');

    const firstRow = catalogPage.firstResultRow();
    await expect(firstRow).toBeVisible();
    await expect(firstRow.getByText('WGT-001')).toBeVisible();
    await expect(firstRow.getByText('Flux Capacitor')).toBeVisible();
    await expect(firstRow.getByText('Enables time travel when 1.21 gigawatts applied')).toBeVisible();
    await expect(firstRow.getByText('Outatime Industries')).toBeVisible();
    await expect(firstRow.getByText('1.210')).toBeVisible();
    await expect(firstRow.getByText(/9,999\.99/)).toBeVisible();
    await expect(firstRow.getByText('2025-10-26')).toBeVisible();
  });

  // AC #6: No-results message shown when search returns nothing
  test('shows no results message when search matches nothing', async ({ catalogPage }) => {
    await catalogPage.goto();
    await catalogPage.search('xyzzy-no-match-abc123');
    await expect(catalogPage.noResultsMessage).toBeVisible();
    await expect(catalogPage.resultsTable).not.toBeVisible();
  });

  // AC #7: Clearing search term and pressing Search returns page to initial empty state
  test('returns to empty initial state when search term is cleared', async ({ catalogPage }) => {
    await catalogPage.goto();
    await catalogPage.search('Flux Capacitor');
    await expect(catalogPage.resultsTable).toBeVisible();

    await catalogPage.clearSearch();
    await expect(catalogPage.resultsTable).not.toBeVisible();
    await expect(catalogPage.noResultsMessage).not.toBeVisible();
  });
});
