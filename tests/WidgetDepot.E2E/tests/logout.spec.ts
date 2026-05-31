import { test, expect } from './fixtures';

test.describe('Customer logout', () => {
  // AC #3: Logged-in customer can log out and is redirected to home page
  test('redirects to home page after signing out', async ({ homePage, navBar, page }) => {
    await homePage.goto();
    await navBar.signOut();
    await expect(page).toHaveURL('/');
  });

  // AC #3: After logout, authenticated routes are no longer accessible
  test('cannot access authenticated routes after signing out', async ({ homePage, navBar, page }) => {
    await homePage.goto();
    await navBar.signOut();
    await page.goto('/accounts/profile');
    await expect(page).toHaveURL(/\/accounts\/login/);
  });
});
