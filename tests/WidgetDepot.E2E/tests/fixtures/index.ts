import { mergeTests } from '@playwright/test';
import { pagesTest } from './pages';
import { authTest } from './auth';

export const test = mergeTests(pagesTest, authTest);
export { expect } from '@playwright/test';
