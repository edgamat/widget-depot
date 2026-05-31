import { Page, Locator } from '@playwright/test';

export class HomePage {
  readonly page: Page;
  readonly heading: Locator;
  readonly homeNavLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.heading = page.getByRole('heading', { level: 1 });
    this.homeNavLink = page.getByRole('link', { name: 'Home' });
  }

  async goto() {
    return await this.page.goto('/');
  }
}
