import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Category Page Object
 * Handles category and subcategory management interactions
 */
export class CategoryPage extends BasePage {
  // Navigation locators
  private readonly categoriesMenuItem: Locator;

  // Category Type (main category) locators
  private readonly createCategoryTypeButton: Locator;
  private readonly categoryTypeNameInput: Locator;
  private readonly categoryTypeDescriptionInput: Locator;
  private readonly categoryTypeSubmitButton: Locator;

  // Category (subcategory) locators
  private readonly createCategoryButton: Locator;
  private readonly categoryNameInput: Locator;
  private readonly categoryDescriptionInput: Locator;
  private readonly categoryTypeSelect: Locator;
  private readonly categorySubmitButton: Locator;

  // Toast notification locator (global, app-level)
  private readonly toastMessage: Locator;

  // List locators
  private readonly categoryTypeList: Locator;
  private readonly categoryList: Locator;

  constructor(page: Page) {
    super(page);

    // Navigation
    this.categoriesMenuItem = page.getByTestId('menu-categories');

    // Category Type elements
    this.createCategoryTypeButton = page.getByTestId('create-category-type-btn');
    this.categoryTypeNameInput = page.getByTestId('category-type-name');
    this.categoryTypeDescriptionInput = page.getByTestId('category-type-description');
    this.categoryTypeSubmitButton = page.getByTestId('category-type-submit');

    // Category (subcategory) elements
    this.createCategoryButton = page.getByTestId('create-category-btn');
    this.categoryNameInput = page.getByTestId('category-name');
    this.categoryDescriptionInput = page.getByTestId('category-description');
    this.categoryTypeSelect = page.getByTestId('category-type-select');
    this.categorySubmitButton = page.getByTestId('category-submit');

    // Toast notification (PrimeNG structure)
    this.toastMessage = page.locator('.p-toast-message-content');

    // Lists
    this.categoryTypeList = page.getByTestId('category-type-list');
    this.categoryList = page.getByTestId('category-list');
  }

  /**
   * Navigate to categories page
   */
  async navigateToCategories() {
    // Wait for menu item to be visible and clickable
    await this.categoriesMenuItem.waitFor({ state: 'visible' });
    await this.categoriesMenuItem.click();

    // Wait for navigation to complete
    await this.page.waitForURL('**/categories');
    await this.waitForPageLoad();
  }

  /**
   * Create a new category type (main category)
   */
  async createCategoryType(name: string, description?: string) {
    await this.createCategoryTypeButton.click();
    await this.categoryTypeNameInput.fill(name);

    if (description) {
      await this.categoryTypeDescriptionInput.fill(description);
    }

    await this.categoryTypeSubmitButton.click();
  }

  /**
   * Verify category type was created successfully
   */
  async isCategoryTypeCreated(name: string): Promise<boolean> {
    // Wait for toast message to appear (indicates operation completed)
    await this.toastMessage.waitFor({ state: 'visible', timeout: 5000 });

    // Find category type by text in the category name span
    const categoryTypeItem = this.categoryTypeList
      .locator('.category-name')
      .filter({ hasText: name })
      .first();

    try {
      // Wait for the element to be visible
      await categoryTypeItem.waitFor({ state: 'visible', timeout: 10000 });
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Create a new category (subcategory)
   */
  async createCategory(name: string, categoryTypeName: string, description?: string) {
    await this.createCategoryButton.click();
    await this.categoryNameInput.fill(name);

    // Select category type from dropdown
    await this.categoryTypeSelect.click();
    const option = this.page.getByTestId(`category-type-option-${categoryTypeName}`);
    await option.click();

    if (description) {
      await this.categoryDescriptionInput.fill(description);
    }

    await this.categorySubmitButton.click();
  }

  /**
   * Verify category was created successfully
   */
  async isCategoryCreated(name: string): Promise<boolean> {
    // Wait for toast message to appear (indicates operation completed)
    await this.toastMessage.waitFor({ state: 'visible', timeout: 5000 });

    // Find category by text in the item name heading
    const categoryItem = this.categoryList
      .locator('.item-name')
      .filter({ hasText: name })
      .first();

    try {
      // Wait for the element to be visible
      await categoryItem.waitFor({ state: 'visible', timeout: 10000 });
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Get success message text from toast
   */
  async getSuccessMessage(): Promise<string> {
    // Wait for toast to be visible
    await this.toastMessage.waitFor({ state: 'visible', timeout: 5000 });

    // Get the detail text from the toast
    const detailElement = this.toastMessage.locator('.p-toast-detail');
    return await detailElement.textContent() || '';
  }

  /**
   * Get count of category types
   */
  async getCategoryTypeCount(): Promise<number> {
    // Wait for list to be stable
    await this.categoryTypeList.waitFor({ state: 'visible', timeout: 5000 });

    // Count accordion tabs (PrimeNG renders them with .p-accordiontab class)
    const items = this.categoryTypeList.locator('.p-accordiontab');

    // Wait for at least one item or timeout
    try {
      await items.first().waitFor({ state: 'attached', timeout: 5000 });
      return await items.count();
    } catch {
      // No items yet, return 0
      return 0;
    }
  }

  /**
   * Get count of categories
   */
  async getCategoryCount(): Promise<number> {
    // Wait for list to be stable
    await this.categoryList.waitFor({ state: 'visible', timeout: 5000 });

    // Count p-card elements (categories are rendered as p-card components)
    const items = this.categoryList.locator('.item-card');

    // Wait for at least one item or timeout
    try {
      await items.first().waitFor({ state: 'attached', timeout: 5000 });
      return await items.count();
    } catch {
      // No items yet, return 0
      return 0;
    }
  }
}
