import { expect, type Page } from '@playwright/test';

export class RegisterPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/register');
    await expect(this.page.getByRole('heading', { name: 'Create your account' })).toBeVisible();
  }

  get displayNameInput() {
    return this.page.locator('#displayName');
  }

  get emailInput() {
    return this.page.locator('#email');
  }

  get passwordInput() {
    return this.page.locator('#password');
  }

  get confirmPasswordInput() {
    return this.page.locator('#confirmPassword');
  }

  get createOrganizationCheckbox() {
    return this.page.locator('#createOrganization');
  }

  get organizationNameInput() {
    return this.page.locator('#organizationName');
  }

  get submitButton() {
    return this.page.getByRole('button', { name: /Create Account|Creating account.../ });
  }

  get errorAlert() {
    return this.page.locator('div.text-red-500');
  }

  get successAlert() {
    return this.page.locator('div.text-green-500');
  }

  async fillForm(options: {
    displayName?: string;
    email?: string;
    password?: string;
    confirmPassword?: string;
    createOrganization?: boolean;
    organizationName?: string;
  }) {
    const {
      displayName = '',
      email = '',
      password = '',
      confirmPassword = '',
      createOrganization = false,
      organizationName = ''
    } = options;

    if (displayName) await this.displayNameInput.fill(displayName);
    if (email) await this.emailInput.fill(email);
    if (password) await this.passwordInput.fill(password);
    if (confirmPassword) await this.confirmPasswordInput.fill(confirmPassword);

    if (createOrganization) {
      await this.createOrganizationCheckbox.check();
      if (organizationName) {
        await this.organizationNameInput.fill(organizationName);
      }
    }
  }

  async submit() {
    await this.submitButton.click();
  }

  async expectError(text: RegExp | string) {
    await expect(this.errorAlert).toContainText(text);
  }

  async expectSuccess() {
    await expect(this.successAlert).toContainText('Registration successful');
  }
}


