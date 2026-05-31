import { test, expect } from './fixtures';

test.describe('Customer self-registration', () => {
  // AC #1: Visitor can navigate to /accounts/register without being redirected to login
  test('unauthenticated visitor can access registration page without redirect', async ({ registerPage }) => {
    await registerPage.goto();
    await expect(registerPage.page).toHaveURL(/\/accounts\/register/);
    await expect(registerPage.page).toHaveTitle(/Create Account/);
  });

  // AC #2: Registration form displays all five required fields
  test('registration form displays all required fields', async ({ registerPage }) => {
    await registerPage.goto();
    await registerPage.waitForReady();
    await expect(registerPage.firstNameInput).toBeVisible();
    await expect(registerPage.lastNameInput).toBeVisible();
    await expect(registerPage.emailInput).toBeVisible();
    await expect(registerPage.passwordInput).toBeVisible();
    await expect(registerPage.confirmPasswordInput).toBeVisible();
    await expect(registerPage.submitButton).toBeVisible();
  });

  // AC #3: Submitting with any required field empty shows a validation message
  test('submitting empty form shows validation messages for required fields', async ({ registerPage }) => {
    await registerPage.goto();
    await registerPage.waitForReady();
    await registerPage.submitButton.click();
    await expect(registerPage.firstNameValidation).toBeVisible();
    await expect(registerPage.lastNameValidation).toBeVisible();
    await expect(registerPage.emailValidation).toBeVisible();
    await expect(registerPage.passwordValidation).toBeVisible();
  });

  // AC #4: Email field validates email format
  test('invalid email format shows a validation error', async ({ registerPage }) => {
    await registerPage.goto();
    await registerPage.waitForReady();
    await registerPage.firstNameInput.fill('Jane');
    await registerPage.lastNameInput.fill('Doe');
    await registerPage.emailInput.fill('not-an-email');
    await registerPage.passwordInput.fill('Password1!');
    await registerPage.confirmPasswordInput.fill('Password1!');
    await registerPage.submitButton.click();
    await expect(registerPage.emailValidation).toBeVisible();
    await expect(registerPage.page).toHaveURL(/\/accounts\/register/);
  });

  // AC #5: Password must be at least 8 characters
  test('password shorter than 8 characters shows a validation error', async ({ registerPage }) => {
    await registerPage.goto();
    await registerPage.waitForReady();
    await registerPage.firstNameInput.fill('Jane');
    await registerPage.lastNameInput.fill('Doe');
    await registerPage.emailInput.fill('jane.doe@example.com');
    await registerPage.passwordInput.fill('short');
    await registerPage.confirmPasswordInput.fill('short');
    await registerPage.submitButton.click();
    await expect(registerPage.passwordValidation).toBeVisible();
    await expect(registerPage.page).toHaveURL(/\/accounts\/register/);
  });

  // AC #6: Confirm password must match password
  test('mismatched confirm password shows a validation error', async ({ registerPage }) => {
    await registerPage.goto();
    await registerPage.waitForReady();
    await registerPage.firstNameInput.fill('Jane');
    await registerPage.lastNameInput.fill('Doe');
    await registerPage.emailInput.fill('jane.doe@example.com');
    await registerPage.passwordInput.fill('Password1!');
    await registerPage.confirmPasswordInput.fill('DifferentPass1!');
    await registerPage.submitButton.click();
    await expect(registerPage.confirmPasswordValidation).toBeVisible();
    await expect(registerPage.page).toHaveURL(/\/accounts\/register/);
  });

  // AC #7: Duplicate email shows "already in use" error message
  test('registering with an already-used email shows an error', async ({ registerPage }) => {
    const email = `duplicate-${Date.now()}@test.example.com`;

    // First registration — should succeed and redirect to login
    await registerPage.goto();
    await registerPage.register('Jane', 'Doe', email, 'Password1!');
    await expect(registerPage.page).toHaveURL(/\/accounts\/login/);

    // Second registration with the same email
    await registerPage.goto();
    await registerPage.register('Jane', 'Doe', email, 'Password1!');
    await expect(registerPage.errorAlert).toBeVisible();
    await expect(registerPage.errorAlert).toContainText('already exists');
  });

  // AC #8: Successful registration redirects to login with a confirmation message
  test('successful registration redirects to login page with confirmation alert', async ({ registerPage, loginPage }) => {
    const email = `new-customer-${Date.now()}@test.example.com`;

    await registerPage.goto();
    await registerPage.register('Alice', 'Smith', email, 'Password1!');

    await expect(registerPage.page).toHaveURL(/\/accounts\/login/);
    await expect(loginPage.registrationSuccessAlert).toBeVisible();
  });

  // AC #9: Login page has a "Create an account" link that navigates to the registration page
  test('login page has a link to the registration page', async ({ loginPage }) => {
    await loginPage.goto();
    await expect(loginPage.createAccountLink).toBeVisible();
    await loginPage.createAccountLink.click();
    await expect(loginPage.page).toHaveURL(/\/accounts\/register/);
  });
});
