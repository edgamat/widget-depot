import { Page, Locator } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly registrationSuccessAlert: Locator;
  readonly createAccountLink: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorAlert: Locator;
  readonly emailValidation: Locator;
  readonly passwordValidation: Locator;

  constructor(page: Page) {
    this.page = page;
    this.registrationSuccessAlert = page.locator('.alert-success');
    this.createAccountLink        = page.getByRole('link', { name: 'Create an account' });
    this.emailInput               = page.getByLabel('Email Address');
    this.passwordInput            = page.getByLabel('Password');
    this.submitButton             = page.getByRole('button', { name: 'Sign In' });
    this.errorAlert               = page.locator('.alert-danger');
    this.emailValidation          = page.locator('.mb-3').filter({ has: page.locator('#email') }).locator('.text-danger');
    this.passwordValidation       = page.locator('.mb-3').filter({ has: page.locator('#password') }).locator('.text-danger');
  }

  async goto() {
    await this.page.goto('/accounts/login');
  }

  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  async login(email: string, password: string) {
    await this.waitForReady();
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }
}
