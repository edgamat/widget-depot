import { Page, Locator } from '@playwright/test';

export class ProblemReportOrderLookupPage {
  readonly page: Page;
  readonly orderNumberInput: Locator;
  readonly submitButton: Locator;
  readonly errorAlert: Locator;
  readonly orderNumberValidation: Locator;
  readonly itemsTable: Locator;

  constructor(page: Page) {
    this.page = page;
    this.orderNumberInput    = page.getByLabel('Order Number');
    this.submitButton        = page.getByRole('button', { name: 'Look Up Order' });
    this.errorAlert          = page.getByRole('alert');
    this.orderNumberValidation = page.locator('.mb-3').filter({ has: page.locator('#orderNumber') }).locator('.text-danger');
    this.itemsTable          = page.getByRole('table');
  }

  async goto() {
    await this.page.goto('/problem-reports/order-lookup');
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  async lookupOrder(orderNumber: number) {
    await this.waitForReady();
    await this.orderNumberInput.fill(String(orderNumber));
    await this.submitButton.click();
  }
}
