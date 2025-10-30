import { test, expect } from '@playwright/test';
import { LoginPage } from './page-objects/LoginPage';

const E2E_EMAIL = 'e2e@example.e2e';
const E2E_PASSWORD = 'e2ee2e';

test.describe('Login E2E', () => {
  test('shows error on invalid credentials', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();

    await login.fill(`invalid_${Date.now()}@example.com`, 'wrongpassword');
    await login.submit();

    await login.expectError(/Login failed|Invalid login|email|password/i);
  });

  test('logs in successfully with valid credentials', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();

    await login.fill(E2E_EMAIL as string, E2E_PASSWORD as string);
    await login.submit();

    // Expect redirect to home
    await page.waitForURL('**/');
    await expect(page).toHaveTitle(/10xWarehouse|Home|Dashboard/i);
  });
});


