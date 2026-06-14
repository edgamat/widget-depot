import { Page, Locator } from '@playwright/test';

export class CustomerProfilePage {
  readonly page: Page;
  readonly adminBadge: Locator;
  readonly backButton: Locator;
  readonly notFoundAlert: Locator;

  constructor(page: Page) {
    this.page          = page;
    this.adminBadge    = page.locator('span.badge.bg-warning');
    this.backButton    = page.getByRole('link', { name: 'Back to Customer List' });
    this.notFoundAlert = page.locator('.alert-warning');
  }

  async goto(id: number) {
    await this.page.goto(`/admin/customers/${id}`);
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  heading() {
    return this.page.getByRole('heading', { level: 1 });
  }
}
