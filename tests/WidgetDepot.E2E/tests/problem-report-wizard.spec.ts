import { test, expect } from './fixtures';

test.describe('Problem report wizard (unauthenticated)', () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  // AC: Only authenticated users can access the wizard
  test('redirects unauthenticated users to login', async ({ page }) => {
    await page.goto('/problem-reports/create/1');
    await expect(page).toHaveURL(/\/accounts\/login/);
  });
});

test.describe('Problem report wizard', () => {
  // AC: Step A displays all order line items with checkboxes
  test('displays item checkboxes for a valid submitted order', async ({ problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();

    await expect(problemReportWizardPage.itemsTable).toBeVisible();
    await expect(problemReportWizardPage.getItemCheckbox(0)).toBeVisible();
    await expect(problemReportWizardPage.nextButton).toBeVisible();
  });

  // AC: Clicking "Next" on Step A with no items selected shows a validation error
  test('shows validation error when Next is clicked with no items selected', async ({ problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();
    await problemReportWizardPage.nextButton.click();

    await expect(problemReportWizardPage.errorAlert).toBeVisible();
    await expect(problemReportWizardPage.errorAlert).toContainText('at least one item');
  });

  // AC: Clicking "Next" with at least one item selected advances to Step B
  test('advances to Step B when at least one item is selected', async ({ problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();
    await problemReportWizardPage.getItemCheckbox(0).check();
    await problemReportWizardPage.nextButton.click();

    await expect(problemReportWizardPage.submitButton).toBeVisible();
    await expect(problemReportWizardPage.notesTextarea).toBeVisible();
  });

  // AC: Step B includes a single optional free-text notes field for the whole report
  test('Step B shows a notes field', async ({ problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();
    await problemReportWizardPage.getItemCheckbox(0).check();
    await problemReportWizardPage.nextButton.click();

    await expect(problemReportWizardPage.notesTextarea).toBeVisible();
  });

  // AC: Clicking "Submit" on Step B with any issue type unset shows a validation error
  test('shows validation error when Submit is clicked with an unset issue type', async ({ problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();
    await problemReportWizardPage.getItemCheckbox(0).check();
    await problemReportWizardPage.nextButton.click();
    await problemReportWizardPage.submitButton.click();

    await expect(problemReportWizardPage.errorAlert).toBeVisible();
    await expect(problemReportWizardPage.errorAlert).toContainText('issue type');
  });

  // AC: On valid submission, the problem report is saved and a confirmation message is shown
  test('shows confirmation after successful submission', async ({ page, problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportWizardPage.goto(orderId);
    await problemReportWizardPage.waitForReady();
    await problemReportWizardPage.getItemCheckbox(0).check();
    await problemReportWizardPage.nextButton.click();

    const firstIssueSelect = page.getByRole('combobox').first();
    await firstIssueSelect.selectOption('Damaged');
    await problemReportWizardPage.submitButton.click();

    await expect(problemReportWizardPage.confirmationAlert).toBeVisible();
    await expect(problemReportWizardPage.confirmationAlert).toContainText('submitted successfully');
  });

  // AC: Multiple problem reports can be submitted against the same order
  test('allows submitting a second problem report for the same order', async ({ page, problemReportWizardPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderId = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    const submitReport = async () => {
      await problemReportWizardPage.goto(orderId);
      await problemReportWizardPage.waitForReady();
      await problemReportWizardPage.getItemCheckbox(0).check();
      await problemReportWizardPage.nextButton.click();
      await page.getByRole('combobox').first().selectOption('Damaged');
      await problemReportWizardPage.submitButton.click();
      await expect(problemReportWizardPage.confirmationAlert).toContainText('submitted successfully');
    };

    await submitReport();
    await submitReport();
  });
});

// AC: Order lookup page shows a "Next" button after a successful lookup that navigates to the wizard
test.describe('Order lookup navigation to wizard', () => {
  test('Next button navigates to the wizard page', async ({ problemReportOrderLookupPage, page }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await problemReportOrderLookupPage.goto();
    await problemReportOrderLookupPage.lookupOrder(orderNumber);

    const nextButton = page.getByRole('button', { name: 'Next: Select Items to Report' });
    await expect(nextButton).toBeVisible();
    await nextButton.click();

    await expect(page).toHaveURL(/\/problem-reports\/create\/\d+/);
  });
});
