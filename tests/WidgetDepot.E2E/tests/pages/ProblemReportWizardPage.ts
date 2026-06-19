import { Page, Locator } from '@playwright/test';

export class ProblemReportWizardPage {
  readonly page: Page;
  readonly nextButton: Locator;
  readonly submitButton: Locator;
  readonly errorAlert: Locator;
  readonly confirmationAlert: Locator;
  readonly itemsTable: Locator;
  readonly notesTextarea: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nextButton         = page.getByRole('button', { name: 'Next' });
    this.submitButton       = page.getByRole('button', { name: 'Submit' });
    this.errorAlert         = page.getByRole('alert');
    this.confirmationAlert  = page.getByRole('alert');
    this.itemsTable         = page.getByRole('table');
    this.notesTextarea      = page.getByLabel('Notes (optional)');
  }

  async goto(orderId: number) {
    await this.page.goto(`/problem-reports/create/${orderId}`);
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  getItemCheckbox(index: number): Locator {
    return this.itemsTable.locator('input[type="checkbox"]').nth(index);
  }

  getIssueTypeSelect(widgetName: string): Locator {
    return this.page.getByLabel(`Issue type for ${widgetName}`);
  }
}
