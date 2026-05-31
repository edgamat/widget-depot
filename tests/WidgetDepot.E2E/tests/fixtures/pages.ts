import { test as base } from '@playwright/test';
import { CatalogImportPage } from '../pages/CatalogImportPage';
import { CatalogPage } from '../pages/CatalogPage';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { RegisterPage } from '../pages/RegisterPage';
import { NavBarComponent } from '../pages/components/NavBarComponent';

export type PageFixtures = {
  catalogImportPage: CatalogImportPage;
  catalogPage: CatalogPage;
  homePage: HomePage;
  loginPage: LoginPage;
  navBar: NavBarComponent;
  registerPage: RegisterPage;
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
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page));
  },
  navBar: async ({ page }, use) => {
    await use(new NavBarComponent(page));
  },
  registerPage: async ({ page }, use) => {
    await use(new RegisterPage(page));
  },
});
