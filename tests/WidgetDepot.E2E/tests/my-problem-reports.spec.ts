import { test, expect } from './fixtures';

test.describe('My Problem Reports (unauthenticated)', () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  // AC: An authenticated customer can access the "My Problem Reports" page
  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/problem-reports');
    await expect(page).toHaveURL(/\/accounts\/login/);
  });
});

test.describe('My Problem Reports', () => {
  // AC: An authenticated customer can access the "My Problem Reports" page
  test('authenticated user can access the page', async ({ myProblemReportsPage }) => {
    await myProblemReportsPage.goto();
    await myProblemReportsPage.waitForReady();

    await expect(myProblemReportsPage.heading).toBeVisible();
  });

  // AC: The page lists the customer's most recent 10 problem reports; if fewer than 10 exist, all are shown
  // AC: Each row displays: order number, date the order was submitted, date the report was filed, and email status
  test('shows report table with correct columns when reports exist', async ({ myProblemReportsPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');

    await myProblemReportsPage.goto();
    await myProblemReportsPage.waitForReady();

    await expect(myProblemReportsPage.table).toBeVisible();
    await expect(myProblemReportsPage.table.getByRole('columnheader', { name: 'Order #' })).toBeVisible();
    await expect(myProblemReportsPage.table.getByRole('columnheader', { name: 'Order Submitted' })).toBeVisible();
    await expect(myProblemReportsPage.table.getByRole('columnheader', { name: 'Report Filed' })).toBeVisible();
    await expect(myProblemReportsPage.table.getByRole('columnheader', { name: 'Email Status' })).toBeVisible();
  });

  // AC: Reports where EmailSent = false display a "Resend email" button
  test('shows Resend email button for reports with email not sent', async ({ myProblemReportsPage }) => {
    test.skip(!process.env.E2E_PROBLEM_REPORT_UNSENT_EMAIL, 'E2E_PROBLEM_REPORT_UNSENT_EMAIL not set');

    await myProblemReportsPage.goto();
    await myProblemReportsPage.waitForReady();

    await expect(myProblemReportsPage.page.getByRole('button', { name: 'Resend email' }).first()).toBeVisible();
  });
});
