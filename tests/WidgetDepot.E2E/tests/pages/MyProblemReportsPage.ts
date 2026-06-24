import { Page, Locator } from '@playwright/test';

export class MyProblemReportsPage {
  readonly page: Page;
  readonly heading: Locator;
  readonly table: Locator;
  readonly emptyMessage: Locator;
  readonly loadError: Locator;
  readonly resendAlert: Locator;

  constructor(page: Page) {
    this.page = page;
    this.heading      = page.getByRole('heading', { name: 'My Problem Reports' });
    this.table        = page.getByRole('table');
    this.emptyMessage = page.getByText('You have no problem reports yet.');
    this.loadError    = page.getByRole('alert');
    this.resendAlert  = page.getByRole('alert');
  }

  async goto() {
    await this.page.goto('/problem-reports');
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  getResendButton(rowIndex: number): Locator {
    return this.table.locator('tbody tr').nth(rowIndex).getByRole('button', { name: 'Resend email' });
  }

  getEmailStatusCell(rowIndex: number): Locator {
    return this.table.locator('tbody tr').nth(rowIndex).locator('td').nth(3);
  }
}
