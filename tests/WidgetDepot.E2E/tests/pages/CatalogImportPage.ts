import { Page, Locator } from '@playwright/test';

export class CatalogImportPage {
  readonly page: Page;
  readonly heading: Locator;
  // Blazor <InputFile> renders as <input type="file"> with no label — CSS selector is the only option
  readonly fileInput: Locator;
  readonly importButton: Locator;
  readonly successAlert: Locator;
  readonly errorAlert: Locator;

  constructor(page: Page) {
    this.page = page;
    this.heading      = page.getByRole('heading', { name: 'Catalog Import' });
    this.fileInput    = page.locator('input[type="file"]');
    this.importButton = page.getByRole('button', { name: /^Import/ });
    this.successAlert = page.locator('.alert-success');
    this.errorAlert   = page.locator('.alert-danger');
  }

  async goto() {
    await this.page.goto('/admin/catalog-import');
  }

  // Blazor InteractiveServer activates handlers only after the SignalR circuit is ready.
  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  async uploadAndImport(filePath: string) {
    await this.waitForReady();
    await this.fileInput.setInputFiles(filePath);
    await this.importButton.click();
  }
}
