import { mergeTests } from '@playwright/test';
import { pagesTest } from './pages';
import { authTest } from './auth';

export const test = pagesTest;
export { expect } from '@playwright/test';
