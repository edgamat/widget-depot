import { Page, Locator, expect } from '@playwright/test';

export class CatalogPage {
  readonly page: Page;
  readonly searchInput: Locator;
  readonly searchButton: Locator;
  readonly searchResults: Locator;
  readonly noResultsMessage: Locator;
  readonly resultsTable: Locator;

  constructor(page: Page) {
    this.page = page;
    this.searchInput     = page.locator('#search-input');
    this.searchButton    = page.locator('#search-button');
    this.searchResults   = page.locator('#search-results');
    this.noResultsMessage = page.getByText('No results found.');
    this.resultsTable    = page.getByRole('table');
  }

  async goto() {
    await this.page.goto('/catalog');
  }

  // Blazor InteractiveServer activates @onclick/@bind handlers only after the
  // SignalR circuit is ready. Any interaction before this silently does nothing.
  async waitForReady() {
    await this.page.waitForFunction(
      () => !!(window as any).Blazor?._internal?.navigationManager,
      { timeout: 30_000 }
    );
    await this.page.waitForLoadState('networkidle');
  }

  // Blazor's @bind:event="oninput" syncs over SignalR asynchronously. Typing
  // one key at a time with a small delay lets each roundtrip commit before the
  // next event fires, preventing a race between typing and the click handler.
  async search(term: string) {
    await this.waitForReady();
    await this.searchInput.click();
    await this.searchInput.pressSequentially(term, { delay: 30 });
    await expect(this.searchInput).toHaveValue(term);
    await this.searchButton.click();
  }

  async clearSearch() {
    await this.searchInput.clear();
    await this.searchButton.click();
  }

  firstResultRow(): Locator {
    return this.searchResults.locator('tbody tr').first();
  }
}
