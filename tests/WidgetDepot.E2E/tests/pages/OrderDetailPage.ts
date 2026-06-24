import { Page, Locator } from '@playwright/test';

export class OrderDetailPage {
  readonly page: Page;
  readonly reportProblemLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.reportProblemLink = page.getByRole('link', { name: 'Report a Problem' });
  }

  async goto(orderNumber: number) {
    await this.page.goto(`/orders/${orderNumber}`);
  }

  async waitForReady() {
    await this.page.waitForLoadState('networkidle');
  }
}
