import { test, expect } from '@playwright/test';
import { LoginPage } from './page-objects/login.page';
import { CategoryPage } from './page-objects/category.page';
import { TEST_USERS, TEST_CATEGORY_TYPES, TEST_CATEGORIES, generateUniqueName } from './fixtures/test-data';

/**
 * E2E Test: Category Management
 * Tests the complete flow of logging in and creating categories and subcategories
 */
test.describe('Category Management', () => {
  let loginPage: LoginPage;
  let categoryPage: CategoryPage;

  // Setup - runs before each test
  test.beforeEach(async ({ page }) => {
    // Arrange: Initialize page objects
    loginPage = new LoginPage(page);
    categoryPage = new CategoryPage(page);

    // Navigate to login page
    await loginPage.navigateToLogin();
  });

  test('should login and create category type', async ({ page }) => {
    /**
     * ARRANGE
     * Prepare test data and initial state
     * Generate unique names with timestamps to avoid conflicts
     */
    const user = TEST_USERS.admin;
    const categoryType = {
      name: generateUniqueName(TEST_CATEGORY_TYPES.technicalInspections.name),
      description: TEST_CATEGORY_TYPES.technicalInspections.description,
    };

    /**
     * ACT - Step 1: Login
     * User authenticates with valid credentials
     */
    await test.step('Login with admin credentials', async () => {
      await loginPage.login(user.email, user.password);

      // Wait for redirect to dashboard
      await page.waitForURL('**/dashboard');
    });

    /**
     * ASSERT - Step 1: Verify login successful
     */
    await test.step('Verify login successful', async () => {
      const isLoggedIn = await loginPage.isLoginSuccessful();
      expect(isLoggedIn).toBeTruthy();
      await expect(page).toHaveURL(/.*\/dashboard$/);
    });

    /**
     * ACT - Step 2: Navigate to Categories
     */
    await test.step('Navigate to Categories page', async () => {
      await categoryPage.navigateToCategories();
    });

    /**
     * ASSERT - Step 2: Verify on categories page
     */
    await test.step('Verify on categories page', async () => {
      await expect(page).toHaveURL(/.*\/categories$/);
    });

    /**
     * ACT - Step 3: Create Category Type (Main Category)
     */
    await test.step('Create a new category type', async () => {
      await categoryPage.createCategoryType(
        categoryType.name,
        categoryType.description
      );
    });

    /**
     * ASSERT - Step 3: Verify category type created
     */
    await test.step('Verify category type was created successfully', async () => {
      const isCreated = await categoryPage.isCategoryTypeCreated(categoryType.name);
      expect(isCreated).toBeTruthy();

      // Verify success message displayed (Polish: "Nowa kategoria została utworzona pomyślnie")
      const successMessage = await categoryPage.getSuccessMessage();
      expect(successMessage).toContain('utworzona pomyślnie');

      // Verify category type count increased
      const count = await categoryPage.getCategoryTypeCount();
      expect(count).toBeGreaterThan(0);
    });
  });

  test('should login and create subcategory', async ({ page }) => {
    /**
     * ARRANGE
     * Prepare test data and initial state
     * Generate unique names with timestamps to avoid conflicts
     */
    const user = TEST_USERS.admin;
    const categoryType = {
      name: generateUniqueName(TEST_CATEGORY_TYPES.technicalInspections.name),
      description: TEST_CATEGORY_TYPES.technicalInspections.description,
    };
    const category = {
      name: generateUniqueName(TEST_CATEGORIES.carInspection.name),
      description: TEST_CATEGORIES.carInspection.description,
      categoryType: categoryType.name, // Use the unique category type name
    };

    /**
     * ACT - Step 1: Login
     * User authenticates with valid credentials
     */
    await test.step('Login with admin credentials', async () => {
      await loginPage.login(user.email, user.password);

      // Wait for redirect to dashboard
      await page.waitForURL('**/dashboard');
    });

    /**
     * ASSERT - Step 1: Verify login successful
     */
    await test.step('Verify login successful', async () => {
      const isLoggedIn = await loginPage.isLoginSuccessful();
      expect(isLoggedIn).toBeTruthy();
      await expect(page).toHaveURL(/.*\/dashboard$/);
    });

    /**
     * ACT - Step 2: Navigate to Categories
     */
    await test.step('Navigate to Categories page', async () => {
      await categoryPage.navigateToCategories();
    });

    /**
     * ASSERT - Step 2: Verify on categories page
     */
    await test.step('Verify on categories page', async () => {
      await expect(page).toHaveURL(/.*\/categories$/);
    });

    /**
     * ACT - Step 3: Create Category Type (Main Category)
     * This is required as a prerequisite for creating a subcategory
     */
    await test.step('Create a new category type', async () => {
      await categoryPage.createCategoryType(
        categoryType.name,
        categoryType.description
      );
    });

    /**
     * ASSERT - Step 3: Verify category type created
     */
    await test.step('Verify category type was created successfully', async () => {
      const isCreated = await categoryPage.isCategoryTypeCreated(categoryType.name);
      expect(isCreated).toBeTruthy();
    });

    /**
     * ACT - Step 4: Create Subcategory under Category Type
     */
    await test.step('Create a new subcategory', async () => {
      await categoryPage.createCategory(
        category.name,
        category.categoryType,
        category.description
      );
    });

    /**
     * ASSERT - Step 4: Verify subcategory created
     */
    await test.step('Verify subcategory was created successfully', async () => {
      const isCreated = await categoryPage.isCategoryCreated(category.name);
      expect(isCreated).toBeTruthy();

      // Verify success message displayed (Polish: "Podkategoria została utworzona pomyślnie")
      const successMessage = await categoryPage.getSuccessMessage();
      expect(successMessage).toContain('utworzona pomyślnie');

      // Verify category count increased
      const count = await categoryPage.getCategoryCount();
      expect(count).toBeGreaterThan(0);
    });
  });

  // Teardown - runs after each test
  test.afterEach(async ({ page }, testInfo) => {
    // If test failed, take screenshot and save trace
    if (testInfo.status !== testInfo.expectedStatus) {
      await page.screenshot({
        path: `test-results/failure-${testInfo.title}-${Date.now()}.png`,
      });
    }
  });
});
