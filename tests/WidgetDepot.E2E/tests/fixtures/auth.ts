import { test as base } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import fs from 'node:fs';
import path from 'node:path';

const STORAGE_STATE_DIR = path.resolve(__dirname, '../../.auth');

export const authTest = base.extend<{}, { workerStorageState: string }>({
  workerStorageState: [async ({ browser }, use, workerInfo) => {
    const fileName = path.join(STORAGE_STATE_DIR, `worker-${workerInfo.workerIndex}.json`);

    if (!fs.existsSync(fileName)) {
      fs.mkdirSync(STORAGE_STATE_DIR, { recursive: true });
      const context = await browser.newContext();
      const page = await context.newPage();
      const loginPage = new LoginPage(page);
      await loginPage.goto();
      await loginPage.login(process.env.E2E_EMAIL!, process.env.E2E_PASSWORD!);
      await page.waitForURL('/');
      await page.waitForLoadState('networkidle');
      await context.storageState({ path: fileName });
      await context.close();
    }

    await use(fileName);
  }, { scope: 'worker' }],

  storageState: ({ workerStorageState }, use) => use(workerStorageState),
});
