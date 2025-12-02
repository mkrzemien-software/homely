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

  // View mode toggle button
  private readonly viewModeToggleButton: Locator;

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

    // View mode toggle
    this.viewModeToggleButton = page.locator('.toolbar-left button').filter({ hasText: /Lista|Grupowane/ });
  }

  /**
   * Get current view mode based on button text
   */
  private async getCurrentViewMode(): Promise<'grouped' | 'list'> {
    const buttonText = await this.viewModeToggleButton.textContent();
    // If button says "Lista", current mode is "grouped"
    // If button says "Grupowane", current mode is "list"
    return buttonText?.includes('Lista') ? 'grouped' : 'list';
  }

  /**
   * Switch to list view if not already in list view
   */
  async ensureListView(): Promise<void> {
    const currentMode = await this.getCurrentViewMode();
    if (currentMode === 'grouped') {
      await this.viewModeToggleButton.click();
      // Wait for view to switch
      await this.page.waitForTimeout(500);
    }
  }

  /**
   * Navigate to categories page
   */
  async navigateToCategories() {
    // Playwright auto-waits for visibility before click
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
    // Use .last() to get the most recent toast in case multiple toasts are visible
    await this.toastMessage.last().waitFor({ state: 'visible' });

    // Find category type by text in the category name span
    const categoryTypeItem = this.categoryTypeList
      .locator('.category-name')
      .filter({ hasText: name })
      .first();

    try {
      // Wait for the element to be visible
      await categoryTypeItem.waitFor({ state: 'visible' });
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

    // Wait for PrimeNG overlay to open
    const overlay = this.page.locator('.p-select-overlay');
    await overlay.waitFor({ state: 'visible', timeout: 5000 });

    // Find the option by text within the overlay
    const option = overlay.locator('.p-select-option').filter({ hasText: categoryTypeName }).first();
    await option.waitFor({ state: 'visible', timeout: 5000 });
    await option.click();

    // Wait for overlay to close
    await overlay.waitFor({ state: 'hidden', timeout: 3000 });

    if (description) {
      await this.categoryDescriptionInput.fill(description);
    }

    await this.categorySubmitButton.click();
  }

  /**
   * Verify category was created successfully
   * Works in both grouped and list view modes
   */
  async isCategoryCreated(name: string): Promise<boolean> {
    // Wait for toast message to appear (indicates operation completed)
    // Use .last() to get the most recent toast in case multiple toasts are visible
    await this.toastMessage.last().waitFor({ state: 'visible', timeout: 5000 });

    // Find category by text in the item name heading
    // This selector works in both grouped and list views
    const categoryItem = this.page
      .locator('.item-name')
      .filter({ hasText: name })
      .first();

    try {
      // Wait for the element to be visible
      await categoryItem.waitFor({ state: 'visible', timeout: 5000 });
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
    // Use .last() to get the most recent toast in case multiple toasts are visible
    await this.toastMessage.last().waitFor({ state: 'visible' });

    // Get the detail text from the toast (use .last() here as well)
    const detailElement = this.toastMessage.last().locator('.p-toast-detail');
    return await detailElement.textContent() || '';
  }

  /**
   * Get count of category types
   */
  async getCategoryTypeCount(): Promise<number> {
    // Wait for list to be stable
    await this.categoryTypeList.waitFor({ state: 'visible' });

    // Count accordion tabs - PrimeNG v19 uses p-accordionTab component which renders as p-accordiontab element
    // Try multiple selectors as PrimeNG structure may vary
    const selectors = [
      'p-accordiontab',
      '.p-accordiontab',
      '[data-testid^="category-type-"]'
    ];

    for (const selector of selectors) {
      const items = this.categoryTypeList.locator(selector);
      const count = await items.count();
      if (count > 0) {
        return count;
      }
    }

    // If no items found with any selector, return 0
    return 0;
  }

  /**
   * Get count of categories
   * Works in both grouped and list view modes
   */
  async getCategoryCount(): Promise<number> {
    // Count all p-card elements with item-card class
    // This works in both grouped and list views
    const items = this.page.locator('.item-card');

    // Wait for at least one item or timeout
    try {
      await items.first().waitFor({ state: 'attached', timeout: 3000 });
      return await items.count();
    } catch {
      // No items yet, return 0
      return 0;
    }
  }
}
