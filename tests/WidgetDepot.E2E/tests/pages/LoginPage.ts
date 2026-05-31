import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly registrationSuccessAlert: Locator;
  readonly createAccountLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.registrationSuccessAlert = page.locator('.alert-success');
    this.createAccountLink        = page.getByRole('link', { name: 'Create an account' });
  }

  async goto() {
    await this.page.goto('/accounts/login');
  }
}
