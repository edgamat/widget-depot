import { test, expect } from './fixtures';

test.describe('Problem report order lookup (unauthenticated)', () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  // AC: Unauthenticated users are redirected to login when attempting to access the problem report wizard
  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/problem-reports/order-lookup');
    await expect(page).toHaveURL(/\/accounts\/login/);
  });
});

test.describe('Problem report order lookup', () => {
  // AC: Submitting an empty order number field shows a validation error
  test('shows validation error when order number is empty', async ({ problemReportOrderLookupPage }) => {
    await problemReportOrderLookupPage.goto();
    await problemReportOrderLookupPage.waitForReady();
    await problemReportOrderLookupPage.submitButton.click();
    await expect(problemReportOrderLookupPage.orderNumberValidation).toBeVisible();
  });

  // AC: If the order number does not exist or belongs to a different customer, a clear error message is shown
  test('shows error message when order does not exist', async ({ problemReportOrderLookupPage }) => {
    await problemReportOrderLookupPage.goto();
    await problemReportOrderLookupPage.lookupOrder(99999999);
    await expect(problemReportOrderLookupPage.errorAlert).toBeVisible();
    await expect(problemReportOrderLookupPage.errorAlert).toContainText('Order not found');
  });

  // AC: If the order number matches a submitted order belonging to the customer, the line items are displayed
  test('displays line items for a valid submitted order', async ({ problemReportOrderLookupPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportOrderLookupPage.goto();
    await problemReportOrderLookupPage.lookupOrder(orderNumber);

    await expect(problemReportOrderLookupPage.itemsTable).toBeVisible();
  });

  // AC: If the order exists but is not in a submitted state, an appropriate error message is shown
  test('shows error message when order is not submitted', async ({ problemReportOrderLookupPage }) => {
    test.skip(!process.env.E2E_NONSUBMITTED_ORDER_NUMBER, 'E2E_NONSUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_NONSUBMITTED_ORDER_NUMBER!);

    await problemReportOrderLookupPage.goto();
    await problemReportOrderLookupPage.lookupOrder(orderNumber);

    await expect(problemReportOrderLookupPage.errorAlert).toBeVisible();
    await expect(problemReportOrderLookupPage.errorAlert).toContainText('not in a submitted state');
  });
});
