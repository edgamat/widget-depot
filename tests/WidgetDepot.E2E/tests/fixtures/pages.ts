import { test as base } from '@playwright/test';
import { CatalogImportPage } from '../pages/CatalogImportPage';
import { CatalogPage } from '../pages/CatalogPage';
import { HomePage } from '../pages/HomePage';

export type PageFixtures = {
  catalogImportPage: CatalogImportPage;
  catalogPage: CatalogPage;
  homePage: HomePage;
};

export const pagesTest = base.extend<PageFixtures>({
  catalogImportPage: async ({ page }, use) => {
    await use(new CatalogImportPage(page));
  },
  catalogPage: async ({ page }, use) => {
    await use(new CatalogPage(page));
  },
  homePage: async ({ page }, use) => {
    await use(new HomePage(page));
  },
});
