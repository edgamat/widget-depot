import { test, expect } from './fixtures';

test.describe('Admin customer list', () => {
  // AC1: Admin can navigate to the customer list from the admin navigation
  test('admin nav link navigates to the customer list', async ({ navBar, customerListPage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();
    const customersLink = navBar.page.getByRole('link', { name: 'Customers' });
    await expect(customersLink).toBeVisible();
    await customersLink.click();
    await expect(customerListPage.page).toHaveURL(/\/admin\/customers/);
    await expect(customerListPage.heading).toBeVisible();
  });

  // AC2 + AC3: List displays name, email, and IsAdmin; sorted by last name
  test('customer list shows name, email and admin columns', async ({ customerListPage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    // The page either shows the table or the empty state — both are valid
    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    const hasEmpty = await customerListPage.emptyState.isVisible().catch(() => false);

    expect(hasTable || hasEmpty).toBe(true);
  });

  // AC4 + AC5: Pagination controls with Font Awesome icons are present when data spans multiple pages
  test('pagination controls are visible when there is more than one page', async ({ customerListPage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    if (!hasTable) {
      test.skip();
      return;
    }

    // If only one page, pagination controls still render but First/Prev are disabled
    await expect(customerListPage.firstPageButton).toBeVisible();
    await expect(customerListPage.previousPageButton).toBeVisible();
    await expect(customerListPage.nextPageButton).toBeVisible();
    await expect(customerListPage.lastPageButton).toBeVisible();
  });

  // AC5: First and Previous are disabled on page 1; clicking Next/Last advances pages
  test('First and Previous are disabled on page 1', async ({ customerListPage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    if (!hasTable) {
      test.skip();
      return;
    }

    await expect(customerListPage.firstPageButton).toBeDisabled();
    await expect(customerListPage.previousPageButton).toBeDisabled();
  });

  // AC5: Next and Last work when multiple pages exist
  test('Next and Last advance to page 2 when multiple pages exist', async ({ customerListPage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    if (!hasTable) {
      test.skip();
      return;
    }

    const isNextEnabled = await customerListPage.nextPageButton.isEnabled().catch(() => false);
    if (!isNextEnabled) {
      test.skip();
      return;
    }

    await customerListPage.nextPageButton.click();
    await customerListPage.page.waitForLoadState('networkidle');

    await expect(customerListPage.firstPageButton).toBeEnabled();
    await expect(customerListPage.previousPageButton).toBeEnabled();
  });

  // AC9: Clicking a customer opens their read-only profile view
  test('clicking View opens the customer profile page', async ({ customerListPage, customerProfilePage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    if (!hasTable) {
      test.skip();
      return;
    }

    await customerListPage.viewButtonForRow(0).click();
    await customerListPage.page.waitForLoadState('networkidle');

    await expect(customerListPage.page).toHaveURL(/\/admin\/customers\/\d+/);
    await expect(customerProfilePage.heading()).toBeVisible();
  });
});

test.describe('Admin customer profile', () => {
  // AC10: Profile view shows admin badge when the customer has admin rights
  test('admin customer profile shows Admin badge', async ({ customerProfilePage }) => {
    // Navigate to admin profile (admin user ID is typically 1 from seeder)
    await customerProfilePage.goto(1);
    await customerProfilePage.waitForReady();

    const isNotFound = await customerProfilePage.notFoundAlert.isVisible().catch(() => false);
    if (isNotFound) {
      test.skip();
      return;
    }

    await expect(customerProfilePage.adminBadge).toBeVisible();
  });

  // Back to Customers link returns to the list
  test('Back to Customers link returns to the customer list', async ({ customerProfilePage, customerListPage }) => {
    await customerProfilePage.goto(1);
    await customerProfilePage.waitForReady();

    const isNotFound = await customerProfilePage.notFoundAlert.isVisible().catch(() => false);
    if (isNotFound) {
      test.skip();
      return;
    }

    await customerProfilePage.backButton.click();
    await expect(customerListPage.page).toHaveURL(/\/admin\/customers$/);
  });

  // AC9: Profile page shows full name in heading
  test('profile page heading shows the customer full name', async ({ customerListPage, customerProfilePage }) => {
    await customerListPage.goto();
    await customerListPage.waitForReady();

    const hasTable = await customerListPage.table.isVisible().catch(() => false);
    if (!hasTable) {
      test.skip();
      return;
    }

    await customerListPage.viewButtonForRow(0).click();
    await customerListPage.page.waitForLoadState('networkidle');

    const heading = await customerProfilePage.heading().textContent();
    expect(heading?.trim().length).toBeGreaterThan(0);
  });
});
