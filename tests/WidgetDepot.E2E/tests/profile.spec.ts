import { test, expect } from './fixtures';

test.describe('User profile', () => {
  // AC: Saving profile redirects back to profile page with a success confirmation
  test('saving profile shows success confirmation', async ({ profilePage, page }) => {
    await profilePage.goto();
    await profilePage.waitForReady();

    await profilePage.firstNameInput.clear();
    await profilePage.firstNameInput.fill('Jane');
    await profilePage.submitButton.click();

    await page.waitForURL(/\/accounts\/profile/);
    await expect(page).toHaveURL(/saved=true/);
    await expect(profilePage.successAlert).toContainText('Your profile has been updated.');
  });

  // Regression: do-signin redirect after save was missing isAdmin and mustChangePassword params,
  // causing a 400 and breaking the user's session.
  test('authenticated session is preserved after saving profile', async ({ profilePage, page }) => {
    await profilePage.goto();
    await profilePage.waitForReady();

    await profilePage.firstNameInput.clear();
    await profilePage.firstNameInput.fill('Jane');
    await profilePage.submitButton.click();

    await page.waitForURL(/\/accounts\/profile/);

    // Navigate to a protected route — if the session broke, we'd be redirected to login
    await profilePage.goto();
    await expect(page).not.toHaveURL(/\/accounts\/login/);
  });
});
