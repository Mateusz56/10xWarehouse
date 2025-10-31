import { test, expect } from '@playwright/test';
import { LoginPage } from './page-objects/LoginPage';

const E2E_EMAIL = 'e2e@example.e2e';
const E2E_PASSWORD = 'e2ee2e';

test('Happy path: login, org, product, warehouse, locations, add+move stock, view log', async ({ page }) => {
  test.setTimeout(120000); // 2 minutes for this long-running test
  // Login
  const login = new LoginPage(page);
  await login.goto();
  await login.fill(E2E_EMAIL, E2E_PASSWORD);
  await login.submit();
  await page.waitForURL('**/');

  // Create organization via sidebar button
  const orgName = `E2E Org ${Date.now()}`;
  await page.getByRole('button', { name: 'Create Organization' }).click();
  await expect(page.getByRole('heading', { name: 'Create Organization' })).toBeVisible();
  await page.getByPlaceholder('Acme Inc.').fill(orgName);
  
  await page.getByRole('button', { name: 'Create' }).click();

  // Wait for page reload after organization creation
  await page.waitForLoadState('networkidle');
  
  // Wait for organizations to load by polling for the switcher to become enabled
  // or for the organization name to appear
  const orgTrigger = page.locator('[data-slot="select-trigger"]').first();
  await page.waitForFunction(
    ({ orgName }) => {
      const trigger = document.querySelector('[data-slot="select-trigger"]') as HTMLElement;
      if (!trigger) return false;
      
      const isDisabled = trigger.hasAttribute('disabled') || 
                         trigger.getAttribute('data-disabled') !== null;
      
      // If not disabled, orgs are loaded
      if (!isDisabled) return true;
      
      // Check if org name appears (might be auto-selected)
      const text = trigger.textContent || '';
      return text.includes(orgName);
    },
    { orgName },
    { timeout: 30000, polling: 500 }
  );

  // Wait for organization switcher to be enabled (organizations loaded)
  await expect(orgTrigger).toBeEnabled({ timeout: 10000 });
  
  // Check if the org is already selected, if not, select it
  const currentValue = await orgTrigger.textContent();
  if (!currentValue?.includes(orgName)) {
    await orgTrigger.click();
    const orgOption = page.locator('[data-slot="select-item"]').filter({ hasText: orgName });
    await orgOption.waitFor({ state: 'visible' });
    await orgOption.click();
    // Wait for organization switch to complete
    await page.waitForLoadState('networkidle');
  }

  // Create product
  const productName = `E2E Product ${Date.now()}`;
  await page.getByRole('link', { name: 'Products' }).click();
  await expect(page.getByRole('heading', { name: 'Products', exact: true })).toBeVisible();
  await expect(page.locator('#CreateProduct')).toBeVisible();
  await page.locator('#CreateProduct').click();
  await expect(page.getByRole('heading', { name: 'Create Product Template' })).toBeVisible();
  await page.getByPlaceholder('Enter product name').fill(productName);
  await page.getByRole('button', { name: 'Create Product' }).click();

  // Create warehouse
  const warehouseName = `E2E Warehouse ${Date.now()}`;
  await page.getByRole('link', { name: 'Warehouses' }).click();
  await expect(page.getByRole('heading', { name: 'Warehouses', exact: true })).toBeVisible();
  await page.locator('#CreateWarehouse').click();
  await expect(page.getByRole('heading', { name: 'Create Warehouse' })).toBeVisible();
  await page.getByPlaceholder('Main Warehouse').fill(warehouseName);
  await page.getByRole('button', { name: 'Create Warehouse' }).click();

  // Open warehouse details
  await page.getByText(warehouseName, { exact: true }).first().click();
  await expect(page.getByRole('heading', { name: 'Locations', exact: true })).toBeVisible();

  // Create two locations
  const loc1 = 'Loc A';
  const loc2 = 'Loc B';
  for (const name of [loc1, loc2]) {
    await page.locator('#CreateLocation').click();
    await expect(page.getByRole('heading', { name: 'Create New Location' })).toBeVisible();
    await page.getByLabel('Name *').fill(name);
    await page.getByRole('button', { name: 'Create Location' }).click();
  }

  // Add stock to first location
  await page.getByRole('link', { name: 'Inventory' }).click();
  await expect(page.getByRole('heading', { name: 'Inventory Summary' })).toBeVisible();
  await page.locator('#AddStock').click();
  await expect(page.getByRole('heading', { name: 'Add Stock' })).toBeVisible();
  await page.getByText('Select a product').click();
  await page.locator('[data-slot="select-item"]').filter({ hasText: productName }).first().click();
  await page.getByText('Select a location').click();
  await page.locator('[data-slot="select-item"]').filter({ hasText: loc1 }).first().click();
  await page.getByLabel('Quantity to Add').fill('5');
  await page.getByRole('button', { name: 'Add Stock' }).click();

  // Move stock to second location
  // Find the card with productName and loc1 then click Move
  const card = page.locator('.rounded-lg.border').filter({ hasText: productName }).filter({ hasText: loc1 });
  await expect(card).toBeVisible();
  await card.getByRole('button', { name: 'Move' }).click();
  await expect(page.getByRole('heading', { name: 'Move Stock' })).toBeVisible();
  await page.getByText('Select destination location').click();
  await page.locator('[data-slot="select-item"]').filter({ hasText: loc2 }).first().click();
  await page.getByLabel('Quantity to Move').fill('3');
  await page.getByRole('button', { name: 'Move Stock' }).click();

  // Verify movements list shows entries
  await page.getByRole('link', { name: 'Stock Movements' }).click();
  await expect(page.getByRole('heading', { name: 'Stock Movement Log' })).toBeVisible();
  await expect(page.getByRole('cell', { name: productName }).first()).toBeVisible();
  await expect(page.getByText('MoveAdd')).toBeVisible();
  await expect(page.getByText('MoveSubtract')).toBeVisible();
});


