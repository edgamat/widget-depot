import { defineConfig, devices } from '@playwright/test';

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
import dotenv from 'dotenv';
import path from 'path';
dotenv.config({ path: path.resolve(__dirname, '.env') });

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',

  use: {
    baseURL: 'https://localhost:7092',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    // Blazor dev cert is self-signed; accept it during tests
    ignoreHTTPSErrors: true,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    // Uncomment when you want broader coverage:
    // { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    // { name: 'webkit',  use: { ...devices['Desktop Safari'] } },
  ],

  // Automatically start all services via Aspire before tests run
  webServer: {
    command: 'aspire run --project ../../src/WidgetDepot.AppHost',
    url: 'https://localhost:7092',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
    ignoreHTTPSErrors: true,
    stdout: 'pipe',
    stderr: 'pipe',
  },
});

