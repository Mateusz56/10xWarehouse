import { test, expect } from '@playwright/test';
import { RegisterPage } from './page-objects/RegisterPage';

const API_BASE = process.env.PLAYWRIGHT_BACKEND_URL || 'http://localhost:8080/api';

test.describe('Registration E2E', () => {
  test('registers successfully without organization creation', async ({ page }) => {
    const register = new RegisterPage(page);
    await register.goto();
    await register.fillForm({
      displayName: 'John Smith',
      email: `john_${Date.now()}@example.com`,
      password: 'secret123',
      confirmPassword: 'secret123'
    });
    await register.submit();

    await register.expectSuccess();

    // Redirect after success
    await page.waitForURL('**/');
  });

  test('registers successfully and creates organization', async ({ page }) => {
    const register = new RegisterPage(page);
    await register.goto();
    await register.fillForm({
      displayName: 'Alice Walker',
      email: `alice_${Date.now()}@example.com`,
      password: 'secret123',
      confirmPassword: 'secret123',
      createOrganization: true,
      organizationName: 'My First Org'
    });
    await register.submit();

    await register.expectSuccess();
    await page.waitForURL('**/');
  });

  test('handles backend error gracefully (duplicate email)', async ({ page, request }) => {
    const duplicateEmail = `dup_${Date.now()}@example.com`;
    // Pre-create the user through backend API to simulate duplicate
    const precreate = await request.post(`${API_BASE}/auth/register`, {
      data: {
        email: duplicateEmail,
        password: 'secret123',
        displayName: 'Dup User',
        createOrganization: false
      }
    });
    expect(precreate.ok()).toBeTruthy();

    // Wait a bit to ensure backend processed the first registration
    await page.waitForTimeout(1000);

    const register = new RegisterPage(page);
    await register.goto();
    await register.fillForm({
      displayName: 'Bob Test',
      email: duplicateEmail,
      password: 'secret123',
      confirmPassword: 'secret123'
    });
    await register.submit();

    // Wait for error to appear with longer timeout
    await expect(register.errorAlert).toBeVisible({ timeout: 10000 });
    await register.expectError(/Registration failed|already in use|Conflict|email|duplicate/i);
  });
});


