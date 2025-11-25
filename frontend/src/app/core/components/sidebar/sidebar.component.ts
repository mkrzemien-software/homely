import { Component, computed, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SidebarModule } from 'primeng/sidebar';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';
import { MenuModule } from 'primeng/menu';
import { ThemeService } from '../../../core/services/theme.service';
import { AuthService } from '../../../core/services/auth.service';
import { HouseholdService } from '../../../core/services/household.service';
import {
  MenuSection,
  MenuItem,
  UserRole,
  getMenuSections
} from './models/sidebar-menu.model';

/**
 * Sidebar Navigation Component
 *
 * Two-section sidebar menu:
 * - Section 1: Household Views (Dashboard, Tasks, Categories, etc.)
 * - Section 2: System Views (System Dashboard, Households, Users, etc.) - only for System Developer
 *
 * Features:
 * - Role-based menu filtering
 * - Premium feature indicators
 * - Responsive behavior (persistent desktop, collapsible tablet, hamburger mobile)
 * - Active state indicator
 * - Household switcher
 * - Keyboard navigation support
 */
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    SidebarModule,
    ButtonModule,
    TooltipModule,
    BadgeModule,
    DividerModule,
    MenuModule
  ],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent {
  private router = inject(Router);
  private themeService = inject(ThemeService);
  authService = inject(AuthService);  // Public for template access
  private householdService = inject(HouseholdService);

  // State signals
  isVisible = signal<boolean>(true);
  isCollapsed = signal<boolean>(false);
  isMobileView = signal<boolean>(false);

  // Theme signals
  currentTheme = this.themeService.appliedTheme;

  // User context signals - derived from AuthService
  currentHouseholdId = signal<string | null>(null);
  userRole = signal<UserRole>('admin'); // TODO: Get from user profile when role is added
  hasPremium = signal<boolean>(false); // TODO: Get from household subscription
  householdName = signal<string>('Loading...');

  constructor() {
    // Effect to sync with auth service user data
    effect(() => {
      const user = this.authService.currentUser();
      const isAuthenticated = this.authService.isAuthenticated();

      if (user && isAuthenticated) {
        // Only set householdId if it's not empty
        const householdId = user.householdId && user.householdId.trim() !== ''
          ? user.householdId
          : null;

        this.currentHouseholdId.set(householdId);

        // Set user role from auth service
        this.userRole.set((user.role as UserRole) || 'member');

        // Fetch household name
        if (householdId) {
          this.fetchHouseholdName(householdId);
        } else {
          this.householdName.set('No household assigned');
        }
      } else {
        this.currentHouseholdId.set(null);
        this.userRole.set('member');
        this.householdName.set('Not logged in');
      }
    });
  }

  // Computed menu sections based on user context
  menuSections = computed<MenuSection[]>(() => {
    return getMenuSections(
      this.currentHouseholdId(),
      this.userRole(),
      this.hasPremium()
    );
  });

  // Fetch household name from API
  private fetchHouseholdName(householdId: string): void {
    if (!householdId || householdId.trim() === '') {
      this.householdName.set('No household assigned');
      return;
    }

    // Call the API to get household details
    this.householdService.getById(householdId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.householdName.set(response.data.name);
        } else {
          // Fallback to short ID if API call fails
          const shortId = householdId.length > 8 ? householdId.substring(0, 8) : householdId;
          this.householdName.set(`Household ${shortId}`);
        }
      },
      error: (error) => {
        console.error('Error fetching household name:', error);
        // Fallback to short ID on error
        const shortId = householdId.length > 8 ? householdId.substring(0, 8) : householdId;
        this.householdName.set(`Household ${shortId}`);
      }
    });
  }

  // Check if current route is active
  isRouteActive(route: string): boolean {
    return this.router.url === route;
  }

  // Toggle sidebar visibility
  toggleSidebar(): void {
    this.isVisible.set(!this.isVisible());
  }

  // Toggle sidebar collapsed state (desktop only)
  toggleCollapsed(): void {
    this.isCollapsed.set(!this.isCollapsed());
  }

  // Close sidebar (mobile only)
  closeSidebar(): void {
    if (this.isMobileView()) {
      this.isVisible.set(false);
    }
  }

  // Navigate and close sidebar on mobile
  navigateTo(route: string): void {
    this.router.navigate([route]);
    this.closeSidebar();
  }

  // Toggle theme between light and dark
  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  // Logout user
  logout(): void {
    this.authService.logout();
  }

  // Handle window resize
  handleResize(): void {
    const width = window.innerWidth;

    if (width < 768) {
      // Mobile
      this.isMobileView.set(true);
      this.isCollapsed.set(false);
      this.isVisible.set(false);
    } else if (width < 1024) {
      // Tablet
      this.isMobileView.set(false);
      this.isCollapsed.set(true);
      this.isVisible.set(true);
    } else {
      // Desktop
      this.isMobileView.set(false);
      this.isVisible.set(true);
    }
  }

  // Initialize component
  ngOnInit(): void {
    this.handleResize();
    window.addEventListener('resize', () => this.handleResize());
  }

  ngOnDestroy(): void {
    window.removeEventListener('resize', () => this.handleResize());
  }
}
