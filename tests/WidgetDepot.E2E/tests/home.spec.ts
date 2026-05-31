import { test, expect } from './fixtures';

test.describe('Home page', () => {
  test('loads successfully', async ({ homePage }) => {
    const response = await homePage.goto();
    expect(response?.status()).toBeLessThan(400);
  });

  test('has expected title', async ({ homePage }) => {
    await homePage.goto();
    await expect(homePage.page).toHaveTitle(/Home/);
  });

  test('renders the welcome heading', async ({ homePage }) => {
    await homePage.goto();
    await expect(homePage.heading).toBeVisible();
  });

  test('navigation menu is present', async ({ homePage }) => {
    await homePage.goto();
    await expect(homePage.homeNavLink).toBeVisible();
  });
});
