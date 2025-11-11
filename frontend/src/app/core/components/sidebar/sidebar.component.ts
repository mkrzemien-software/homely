import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SidebarModule } from 'primeng/sidebar';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';
import { MenuModule } from 'primeng/menu';
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

  // State signals
  isVisible = signal<boolean>(true);
  isCollapsed = signal<boolean>(false);
  isMobileView = signal<boolean>(false);

  // User context signals (TODO: Connect to actual auth service)
  currentHouseholdId = signal<string | null>('test-household');
  userRole = signal<UserRole>('admin');
  hasPremium = signal<boolean>(false);
  householdName = signal<string>('Moje Gospodarstwo');

  // Computed menu sections based on user context
  menuSections = computed<MenuSection[]>(() => {
    return getMenuSections(
      this.currentHouseholdId(),
      this.userRole(),
      this.hasPremium()
    );
  });

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
