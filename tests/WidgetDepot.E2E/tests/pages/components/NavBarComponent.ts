import { Page, Locator, expect } from '@playwright/test';

export class NavBarComponent {
  readonly page: Page;
  readonly emailDisplay: Locator;
  readonly signOutLink: Locator;
  readonly catalogUploadLink: Locator;

  constructor(page: Page) {
    this.page             = page;
    this.emailDisplay     = page.locator('.top-row span');
    this.signOutLink      = page.getByRole('link', { name: 'Sign Out' });
    this.catalogUploadLink = page.getByRole('link', { name: 'Catalog Upload' });
  }

  async signOut() {
    await this.signOutLink.click();
    await this.page.waitForURL('/');
  }

  async expectEmailDisplayed(email: string) {
    await expect(this.page.locator('.top-row').getByText(email)).toBeVisible();
  }
}
