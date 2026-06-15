import { Page, Locator } from '@playwright/test';

export class ProfilePage {
  readonly page: Page;
  readonly firstNameInput: Locator;
  readonly submitButton: Locator;
  readonly successAlert: Locator;
  readonly errorAlert: Locator;

  constructor(page: Page) {
    this.page           = page;
    this.firstNameInput = page.getByLabel('First Name');
    this.submitButton   = page.getByRole('button', { name: 'Save' });
    this.successAlert   = page.locator('.alert-success');
    this.errorAlert     = page.locator('.alert-danger');
  }

  async goto() {
    await this.page.goto('/accounts/profile');
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }
}
