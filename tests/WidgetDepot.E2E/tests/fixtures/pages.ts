import { test as base } from '@playwright/test';
import { CatalogImportPage } from '../pages/CatalogImportPage';
import { CatalogPage } from '../pages/CatalogPage';
import { CustomerListPage } from '../pages/CustomerListPage';
import { CustomerProfilePage } from '../pages/CustomerProfilePage';
import { HomePage } from '../pages/HomePage';
import { LoginPage } from '../pages/LoginPage';
import { ProblemReportOrderLookupPage } from '../pages/ProblemReportOrderLookupPage';
import { ProblemReportWizardPage } from '../pages/ProblemReportWizardPage';
import { ProfilePage } from '../pages/ProfilePage';
import { RegisterPage } from '../pages/RegisterPage';
import { NavBarComponent } from '../pages/components/NavBarComponent';

export type PageFixtures = {
  catalogImportPage: CatalogImportPage;
  catalogPage: CatalogPage;
  customerListPage: CustomerListPage;
  customerProfilePage: CustomerProfilePage;
  homePage: HomePage;
  loginPage: LoginPage;
  problemReportOrderLookupPage: ProblemReportOrderLookupPage;
  problemReportWizardPage: ProblemReportWizardPage;
  profilePage: ProfilePage;
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
  customerListPage: async ({ page }, use) => {
    await use(new CustomerListPage(page));
  },
  customerProfilePage: async ({ page }, use) => {
    await use(new CustomerProfilePage(page));
  },
  homePage: async ({ page }, use) => {
    await use(new HomePage(page));
  },
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page));
  },
  problemReportOrderLookupPage: async ({ page }, use) => {
    await use(new ProblemReportOrderLookupPage(page));
  },
  problemReportWizardPage: async ({ page }, use) => {
    await use(new ProblemReportWizardPage(page));
  },
  profilePage: async ({ page }, use) => {
    await use(new ProfilePage(page));
  },
  navBar: async ({ page }, use) => {
    await use(new NavBarComponent(page));
  },
  registerPage: async ({ page }, use) => {
    await use(new RegisterPage(page));
  },
});
