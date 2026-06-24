import { test, expect } from './fixtures';

test.describe('Order detail — Report a Problem link', () => {
  // AC: The order detail page for a submitted order displays a "Report a Problem" link
  test('shows Report a Problem link for submitted orders', async ({ orderDetailPage }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await orderDetailPage.goto(orderNumber);
    await orderDetailPage.waitForReady();

    await expect(orderDetailPage.reportProblemLink).toBeVisible();
  });

  // AC: Clicking the link navigates to the problem report wizard with the order number pre-filled
  test('Report a Problem link navigates to wizard with order number pre-filled', async ({ orderDetailPage, page }) => {
    test.skip(!process.env.E2E_SUBMITTED_ORDER_NUMBER, 'E2E_SUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_SUBMITTED_ORDER_NUMBER!);

    await orderDetailPage.goto(orderNumber);
    await orderDetailPage.waitForReady();
    await orderDetailPage.reportProblemLink.click();

    await expect(page).toHaveURL(new RegExp(`/problem-reports/create/${orderNumber}`));
  });

  // AC: The link does not appear on non-submitted orders
  test('does not show Report a Problem link for non-submitted orders', async ({ orderDetailPage }) => {
    test.skip(!process.env.E2E_NONSUBMITTED_ORDER_NUMBER, 'E2E_NONSUBMITTED_ORDER_NUMBER not set');
    const orderNumber = parseInt(process.env.E2E_NONSUBMITTED_ORDER_NUMBER!);

    await orderDetailPage.goto(orderNumber);
    await orderDetailPage.waitForReady();

    await expect(orderDetailPage.reportProblemLink).not.toBeVisible();
  });
});
