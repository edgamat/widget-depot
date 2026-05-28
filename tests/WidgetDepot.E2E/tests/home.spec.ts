import { test, expect } from '@playwright/test';

test.describe('Home page', () => {
  test('loads successfully', async ({ page }) => {
    const response = await page.goto('/');
    expect(response?.status()).toBeLessThan(400);
  });

  test('has expected title', async ({ page }) => {
    await page.goto('/');
    // Default Blazor template title — adjust to match your app
    await expect(page).toHaveTitle(/Home/);
  });

  test('renders the welcome heading', async ({ page }) => {
    await page.goto('/');
    // Default Blazor template heading — adjust to match your app
    const heading = page.getByRole('heading', { level: 1 });
    await expect(heading).toBeVisible();
  });

  test('navigation menu is present', async ({ page }) => {
    await page.goto('/');
    // Blazor templates use a <nav> element with a "Home" link
    await expect(page.getByRole('link', { name: 'Home' })).toBeVisible();
  });
});