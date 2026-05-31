import { test as base } from '@playwright/test';
import { CatalogPage } from '../pages/CatalogPage';
import { HomePage } from '../pages/HomePage';

export type PageFixtures = {
  catalogPage: CatalogPage;
  homePage: HomePage;
};

export const pagesTest = base.extend<PageFixtures>({
  catalogPage: async ({ page }, use) => {
    await use(new CatalogPage(page));
  },
  homePage: async ({ page }, use) => {
    await use(new HomePage(page));
  },
});
