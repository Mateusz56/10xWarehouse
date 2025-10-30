import { expect, type Page } from '@playwright/test';

export class LoginPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/login');
    await expect(this.page.getByRole('heading', { name: 'Sign in to your account' })).toBeVisible();
  }

  get emailInput() {
    return this.page.locator('#email');
  }

  get passwordInput() {
    return this.page.locator('#password');
  }

  get submitButton() {
    return this.page.getByRole('button', { name: /Sign In|Signing in.../ });
  }

  get errorAlert() {
    return this.page.locator('div.text-red-500');
  }

  async fill(email?: string, password?: string) {
    if (email) await this.emailInput.fill(email);
    if (password) await this.passwordInput.fill(password);
  }

  async submit() {
    await this.submitButton.click();
  }

  async expectError(text: RegExp | string) {
    await expect(this.errorAlert).toContainText(text);
  }
}


