import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Login Page Object
 * Handles authentication interactions
 */
export class LoginPage extends BasePage {
  // Locators using data-testid convention
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly loginButton: Locator;
  private readonly errorMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.emailInput = page.getByTestId('login-email');
    this.passwordInput = page.getByTestId('login-password');
    this.loginButton = page.getByTestId('login-submit');
    this.errorMessage = page.getByTestId('login-error');
  }

  /**
   * Navigate to login page
   */
  async navigateToLogin() {
    await this.goto('/auth/login');
    await this.waitForPageLoad();
  }

  /**
   * Perform login with credentials
   */
  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }

  /**
   * Check if login was successful (redirected to dashboard)
   */
  async isLoginSuccessful(): Promise<boolean> {
    await this.page.waitForURL('**/dashboard');
    return this.page.url().includes('/dashboard');
  }

  /**
   * Get error message if login failed
   */
  async getErrorMessage(): Promise<string> {
    return await this.errorMessage.textContent() || '';
  }

  /**
   * Check if user is already logged in
   */
  async isLoggedIn(): Promise<boolean> {
    // Check for presence of user menu or logout button
    const userMenu = this.page.getByTestId('user-menu');
    return await userMenu.isVisible().catch(() => false);
  }
}
