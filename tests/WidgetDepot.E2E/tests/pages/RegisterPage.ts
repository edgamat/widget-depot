import { Page, Locator } from '@playwright/test';

export class RegisterPage {
  readonly page: Page;
  readonly firstNameInput: Locator;
  readonly lastNameInput: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly confirmPasswordInput: Locator;
  readonly submitButton: Locator;
  readonly errorAlert: Locator;
  readonly firstNameValidation: Locator;
  readonly lastNameValidation: Locator;
  readonly emailValidation: Locator;
  readonly passwordValidation: Locator;
  readonly confirmPasswordValidation: Locator;

  constructor(page: Page) {
    this.page = page;
    this.firstNameInput       = page.getByLabel('First Name');
    this.lastNameInput        = page.getByLabel('Last Name');
    this.emailInput           = page.getByLabel('Email Address');
    // exact: true avoids matching "Confirm Password" as well
    this.passwordInput        = page.getByLabel('Password', { exact: true });
    this.confirmPasswordInput = page.getByLabel('Confirm Password');
    this.submitButton         = page.getByRole('button', { name: 'Create Account' });
    this.errorAlert           = page.locator('.alert-danger');

    // Validation messages scoped to their field's .mb-3 wrapper
    this.firstNameValidation       = page.locator('.mb-3').filter({ has: page.locator('#firstName') }).locator('.text-danger');
    this.lastNameValidation        = page.locator('.mb-3').filter({ has: page.locator('#lastName') }).locator('.text-danger');
    this.emailValidation           = page.locator('.mb-3').filter({ has: page.locator('#email') }).locator('.text-danger');
    this.passwordValidation        = page.locator('.mb-3').filter({ has: page.locator('#password') }).locator('.text-danger');
    this.confirmPasswordValidation = page.locator('.mb-3').filter({ has: page.locator('#confirmPassword') }).locator('.text-danger');
  }

  async goto() {
    await this.page.goto('/accounts/register');
  }

  // Blazor InteractiveServer activates @onclick/@bind handlers only after the
  // SignalR circuit is ready. Any interaction before this silently does nothing.
  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  async register(first: string, last: string, email: string, password: string, confirm = password) {
    await this.waitForReady();
    await this.firstNameInput.fill(first);
    await this.lastNameInput.fill(last);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.confirmPasswordInput.fill(confirm);
    await this.submitButton.click();
  }
}
