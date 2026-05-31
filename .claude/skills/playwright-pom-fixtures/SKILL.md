---
name: playwright-pom-fixtures
description: Use this skill whenever adding, modifying, or refactoring Playwright (TypeScript) end-to-end tests in this project. The project follows a strict Page Object Model + Playwright fixtures pattern with `tests/pages/` and `tests/fixtures/` directories, plus a reusable auth/storageState fixture. Apply this skill any time a new `.spec.ts` test is created, any time raw locators (`page.locator(...)`, `page.getByRole(...)`, etc.) appear in test code, any time a new page or component needs test coverage, or any time the user mentions "Playwright test", "e2e test", "page object", "POM", or "test fixture". Do not write Playwright tests in this project without consulting this skill first — the project's structural conventions matter and free-form tests will not match the existing codebase.
---

# Playwright Page Object Model + Fixtures

This project uses the **Page Object Model (POM)** combined with **Playwright fixtures** for all end-to-end tests. Tests never touch raw selectors directly — locators live in page objects, and page objects are injected into tests via fixtures.

This skill exists to make sure any new Playwright test added to this project matches the established structure. Deviating from this pattern creates maintenance debt: when a UI element changes, the fix should touch one page object, not a dozen scattered tests.

## Project conventions

### Directory layout

```
tests/WidgetDepot.E2E/
├── playwright.config.ts
├── package.json
└── tests/
    ├── fixtures/
    │   ├── pages.ts      # Page object fixtures (single source of truth)
    │   ├── auth.ts       # Authentication / storageState fixture
    │   └── index.ts      # Re-exports `test` and `expect`
    ├── pages/            # One file per page
    │   ├── CatalogImportPage.ts
    │   ├── CatalogPage.ts
    │   ├── HomePage.ts
    │   ├── LoginPage.ts
    │   ├── RegisterPage.ts
    │   └── components/   # Reusable component objects (nav bar, modals, etc.)
    ├── data/             # CSV and other test data files
    └── *.spec.ts         # The actual tests
```

When adding a new page object, place it in `tests/pages/`. When adding a reusable widget (e.g. a header used across many pages), place it in `tests/pages/components/`.

### What goes where

- **Page objects** own locators and user-facing actions (`login`, `addItemToCart`, `openSettingsMenu`).
- **Tests** orchestrate page objects and contain assertions about business outcomes.
- **Fixtures** wire page objects into tests and handle setup/teardown.

Assertion helpers on page objects (e.g. `expectErrorVisible()`) are acceptable when they make tests read more clearly — use judgment per case. The hard rule is that tests should not contain raw selectors.

## Writing a page object

Use this shape for any new page object:

```ts
// tests/pages/LoginPage.ts
import { Page, Locator, expect } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  // Locators are readonly properties initialized in the constructor.
  // Prefer role-based and label-based locators over CSS/XPath.
  readonly usernameInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorAlert: Locator;

  constructor(page: Page) {
    this.page = page;
    this.usernameInput = page.getByLabel('Username');
    this.passwordInput = page.getByLabel('Password');
    this.submitButton  = page.getByRole('button', { name: 'Sign in' });
    this.errorAlert    = page.getByRole('alert');
  }

  // Navigation method — every page object that represents a routable page has one.
  async goto() {
    await this.page.goto('/login');
  }

  // Action methods describe user intent, not mechanics.
  async login(username: string, password: string) {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }

  // Optional assertion helper — fine when it improves test readability.
  async expectError(text: string) {
    await expect(this.errorAlert).toHaveText(text);
  }
}
```

**Locator priority** (use the first one that fits):
1. `getByRole` — most resilient to markup changes
2. `getByLabel` — for form inputs
3. `getByPlaceholder`, `getByText`, `getByAltText`, `getByTitle`
4. `getByTestId` — when nothing semantic is available
5. CSS / XPath — last resort; flag this in a code comment explaining why

## Writing fixtures

### Page object fixtures

`tests/fixtures/pages.ts` registers each page object as a fixture so tests receive it via destructuring:

```ts
// tests/fixtures/pages.ts
import { test as base } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';

export type PageFixtures = {
  loginPage: LoginPage;
  dashboardPage: DashboardPage;
};

export const pagesTest = base.extend<PageFixtures>({
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page));
  },
  dashboardPage: async ({ page }, use) => {
    await use(new DashboardPage(page));
  },
});
```

When adding a new page object, register it here too. The fixture name should be the camelCase page-object name (e.g. `SettingsPage` → `settingsPage`).

### Auth fixture (storageState pattern)

Logging in via the UI on every test is slow. The project uses Playwright's `storageState` to authenticate once per worker and reuse the session.

```ts
// tests/fixtures/auth.ts
import { test as base, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import fs from 'node:fs';
import path from 'node:path';

export type AuthFixtures = {
  // Provides a Page already signed in as a standard user.
  authedPage: import('@playwright/test').Page;
};

// Worker-scoped: the storage state file is created once per worker.
const STORAGE_STATE_DIR = path.resolve(__dirname, '../.auth');

export const authTest = base.extend<{}, { workerStorageState: string }>({
  workerStorageState: [async ({ browser }, use, workerInfo) => {
    const fileName = path.join(STORAGE_STATE_DIR, `worker-${workerInfo.workerIndex}.json`);

    if (!fs.existsSync(fileName)) {
      fs.mkdirSync(STORAGE_STATE_DIR, { recursive: true });
      const context = await browser.newContext();
      const page = await context.newPage();
      const loginPage = new LoginPage(page);
      await loginPage.goto();
      // Credentials should come from environment variables, not hardcoded.
      await loginPage.login(process.env.E2E_USER!, process.env.E2E_PASSWORD!);
      // Wait for an indicator that login succeeded before saving state.
      await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
      await context.storageState({ path: fileName });
      await context.close();
    }

    await use(fileName);
  }, { scope: 'worker' }],

  // Override the default storageState so every test in this fixture starts signed in.
  storageState: ({ workerStorageState }, use) => use(workerStorageState),
});
```

### Combined export

`tests/fixtures/index.ts` merges fixtures so tests have one import:

```ts
// tests/fixtures/index.ts
import { mergeTests } from '@playwright/test';
import { pagesTest } from './pages';
import { authTest } from './auth';

export const test = mergeTests(pagesTest, authTest);
export { expect } from '@playwright/test';
```

Tests import `test` and `expect` from `./fixtures` (or the appropriate relative path) — **never directly from `@playwright/test`**. This is the signal that you're in the project's test ecosystem.

## Writing a test

```ts
// tests/login.spec.ts
import { test, expect } from './fixtures';

test.describe('Login', () => {
  // Unauthenticated test — does not use the auth fixture.
  test.use({ storageState: { cookies: [], origins: [] } });

  test('shows error on bad credentials', async ({ loginPage }) => {
    await loginPage.goto();
    await loginPage.login('wrong', 'wrong');
    await loginPage.expectError('Invalid username or password');
  });
});

test.describe('Dashboard (authenticated)', () => {
  // This describe block inherits the auth fixture's storageState automatically.
  test('shows welcome banner', async ({ page, dashboardPage }) => {
    await dashboardPage.goto();
    await expect(dashboardPage.welcomeBanner).toBeVisible();
  });
});
```

## Workflow when adding a new test

Follow this order. Skipping steps tends to produce tests that drift from the project's structure.

1. **Identify the page(s) under test.** Check `tests/pages/` for an existing page object.
2. **If no page object exists, create one first.** Don't write the test against raw locators with a plan to refactor later.
3. **Register the new page object as a fixture** in `tests/fixtures/pages.ts`.
4. **Decide if the test needs authentication.** If yes, do nothing extra — the auth fixture is the default. If no, add `test.use({ storageState: { cookies: [], origins: [] } })` to the describe block.
5. **Write the test** importing `test` and `expect` from the fixtures index.
6. **Verify no raw selectors appear in the `.spec.ts` file.** If they do, move them into the page object.

## Anti-patterns to avoid

- **Importing `test` from `@playwright/test` in a spec file.** Always import from the project's fixtures index.
- **`page.locator('.some-css')` inside a `.spec.ts`.** Selectors live in page objects.
- **Instantiating page objects manually** (`new LoginPage(page)`) inside a test. Use the fixture.
- **One giant `BasePage` that all pages inherit from.** Composition beats inheritance here — extract shared behavior into component objects or helper functions.
- **Hardcoded credentials or URLs.** Use environment variables and `baseURL` from `playwright.config.ts`.
- **Sleeps / arbitrary waits** (`page.waitForTimeout(2000)`). Use Playwright's auto-waiting locators and web-first assertions.

## When the user says "add a Playwright test for X"

Default behavior:
1. Look at `tests/pages/` to see what page objects exist.
2. If `X` involves a page without a page object, create the page object first, then the fixture registration, then the test.
3. If `X` is an authenticated flow, use the existing auth fixture rather than logging in inside the test.
4. Show the user the new/modified files: the page object, the fixture update (if any), and the spec.
5. Run `npm test --prefix tests/WidgetDepot.E2E` and report the results to the user.
