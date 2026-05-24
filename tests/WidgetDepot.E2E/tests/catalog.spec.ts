import { test, expect, Page, Locator } from '@playwright/test';

const SEARCH_INPUT = '#search-input';
const SEARCH_BUTTON = '#search-button';

// Blazor InteractiveServer renders the page via SSR first, then connects a
// SignalR circuit before @onclick / @bind handlers become active. Clicking
// before the circuit is ready silently does nothing.
async function waitForBlazorReady(page: Page): Promise<void> {
  await page.waitForFunction(
    () => !!(window as any).Blazor?._internal?.navigationManager,
    { timeout: 30_000 }
  );
}

// Blazor's @bind:event="oninput" pushes the value to the server over SignalR
// asynchronously. fill() can land its input event so close to a follow-up
// click that the server runs the click handler before the binding has been
// applied. Typing one key at a time with a small delay gives each oninput
// roundtrip time to commit before the next event fires.
async function typeSearch(input: Locator, value: string): Promise<void> {
  await input.click();
  await input.pressSequentially(value, { delay: 30 });
  // Sanity check: confirm typing reached the DOM. If this fails, the typing
  // never landed; if it passes, the problem is downstream (bind propagation
  // or the click handler).
  await expect(input).toHaveValue(value);
}

test.describe('Catalog page', () => {
  // AC #1: Unauthenticated visitor can navigate to catalog without redirect
  test('unauthenticated visitor can access catalog without being redirected to login', async ({ page }) => {
    await page.goto('/catalog');
    await expect(page).toHaveURL(/\/catalog/);
    await expect(page).toHaveTitle(/Widget Catalog/);
  });

  // AC #2: On initial page load, no widgets displayed and no search executed
  test('shows no results and no message on initial load', async ({ page }) => {
    await page.goto('/catalog');
    await expect(page.getByRole('table')).not.toBeVisible();
    await expect(page.getByText('No results found.')).not.toBeVisible();
  });

  // AC #3: Search box and Search button are visible
  test('displays search input and Search button', async ({ page }) => {
    await page.goto('/catalog');
    await expect(page.locator(SEARCH_INPUT)).toBeVisible();
    await expect(page.locator(SEARCH_BUTTON)).toBeVisible();
  });

  // AC #4: Search returns matching widgets (case-insensitive)
  test('returns matching widgets for a case-insensitive search term', async ({ page }) => {
    await page.goto('/catalog');
    await waitForBlazorReady(page);
    await typeSearch(page.locator(SEARCH_INPUT), 'flux');
    await page.locator(SEARCH_BUTTON).click();
    await expect(page.locator('table tbody tr').first()).toBeVisible();
  });

  // AC #5: Each result displays SKU, Name, Description, Manufacturer, Weight, Price, Date Available
  test('displays all required columns for each result', async ({ page }) => {
    await page.goto('/catalog');
    await waitForBlazorReady(page);
    await typeSearch(page.locator(SEARCH_INPUT), 'Flux Capacitor');
    await page.locator(SEARCH_BUTTON).click();

    const firstRow = page.locator('table tbody tr').first();
    await expect(firstRow).toBeVisible();
    await expect(firstRow.getByText('WGT-001')).toBeVisible();
    await expect(firstRow.getByText('Flux Capacitor')).toBeVisible();
    await expect(firstRow.getByText('Enables time travel when 1.21 gigawatts applied')).toBeVisible();
    await expect(firstRow.getByText('Outatime Industries')).toBeVisible();
    await expect(firstRow.getByText('1.21')).toBeVisible();
    await expect(firstRow.getByText(/9,999\.99/)).toBeVisible();
    await expect(firstRow.getByText('2025-10-26')).toBeVisible();
  });

  // AC #6: No-results message shown when search returns nothing
  test('shows no results message when search matches nothing', async ({ page }) => {
    await page.goto('/catalog');
    await waitForBlazorReady(page);
    await typeSearch(page.locator(SEARCH_INPUT), 'xyzzy-no-match-abc123');
    await page.locator(SEARCH_BUTTON).click();
    await expect(page.getByText('No results found.')).toBeVisible();
    await expect(page.getByRole('table')).not.toBeVisible();
  });

  // AC #7: Clearing search term and pressing Search returns page to initial empty state
  test('returns to empty initial state when search term is cleared', async ({ page }) => {
    await page.goto('/catalog');
    await waitForBlazorReady(page);
    const input = page.locator(SEARCH_INPUT);
    await typeSearch(input, 'Flux Capacitor');
    await page.locator(SEARCH_BUTTON).click();
    await expect(page.getByRole('table')).toBeVisible();

    await input.clear();
    await page.locator(SEARCH_BUTTON).click();
    await expect(page.getByRole('table')).not.toBeVisible();
    await expect(page.getByText('No results found.')).not.toBeVisible();
  });
});
