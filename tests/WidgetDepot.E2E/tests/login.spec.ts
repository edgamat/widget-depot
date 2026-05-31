import { test, expect } from './fixtures';

test.describe('Customer login', () => {
  test.use({ storageState: { cookies: [], origins: [] } });

  // AC #4: Login form validates that fields are not empty before submitting
  test('submitting an empty form shows validation messages for email and password', async ({ loginPage }) => {
    await loginPage.goto();
    await loginPage.waitForReady();
    await loginPage.submitButton.click();
    await expect(loginPage.emailValidation).toBeVisible();
    await expect(loginPage.passwordValidation).toBeVisible();
  });

  // AC #2: Incorrect credentials show a generic error without indicating which field is wrong
  test('shows error message on invalid credentials', async ({ loginPage }) => {
    await loginPage.goto();
    await loginPage.login('unknown@example.com', 'wrongpassword');
    await expect(loginPage.errorAlert).toBeVisible();
    await expect(loginPage.errorAlert).toContainText('Invalid email or password');
  });

  // AC #1 + AC #6: Valid credentials redirect to home page and show email in header
  test('redirects to home page and shows email in header after successful login', async ({ loginPage, navBar, page }) => {
    await loginPage.goto();
    await loginPage.login(process.env.E2E_EMAIL!, process.env.E2E_PASSWORD!);
    await expect(page).toHaveURL('/');
    await navBar.expectEmailDisplayed(process.env.E2E_EMAIL!);
  });
});
