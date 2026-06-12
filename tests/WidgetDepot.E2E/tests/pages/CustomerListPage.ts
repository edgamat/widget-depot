import { Page, Locator } from '@playwright/test';

export class CustomerListPage {
  readonly page: Page;
  readonly heading: Locator;
  readonly table: Locator;
  readonly emptyState: Locator;
  readonly loadError: Locator;
  readonly firstPageButton: Locator;
  readonly previousPageButton: Locator;
  readonly nextPageButton: Locator;
  readonly lastPageButton: Locator;
  readonly pageIndicator: Locator;

  constructor(page: Page) {
    this.page = page;
    this.heading          = page.getByRole('heading', { name: 'Customers' });
    this.table            = page.getByRole('table');
    this.emptyState       = page.getByText('No customers are registered yet.');
    this.loadError        = page.locator('.alert-danger');
    this.firstPageButton  = page.getByTitle('First');
    this.previousPageButton = page.getByTitle('Previous');
    this.nextPageButton   = page.getByTitle('Next');
    this.lastPageButton   = page.getByTitle('Last');
    this.pageIndicator    = page.getByText(/^Page \d+ of \d+$/);
  }

  async goto() {
    await this.page.goto('/admin/customers');
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  rows() {
    return this.table.locator('tbody tr');
  }

  viewButtonForRow(index: number) {
    return this.rows().nth(index).getByRole('button', { name: 'View' });
  }
}
