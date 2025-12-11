import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// PrimeNG Components
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { PaginatorModule } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';

// Custom Components
import { GlobalUserSearchComponent } from './components/global-user-search/global-user-search.component';
import { UserDetailsPanelComponent } from './components/user-details-panel/user-details-panel.component';
// import { RoleManagementFormComponent } from './components/role-management-form/role-management-form.component';
import { AccountActionsToolbarComponent } from './components/account-actions-toolbar/account-actions-toolbar.component';
import { CreateUserDialogComponent } from './components/create-user-dialog/create-user-dialog.component';
import { HouseholdAssignmentComponent } from './components/household-assignment/household-assignment.component';

// Services and Interfaces
import { SystemUsersService, SystemUser, UserSearchFilters, UserRole, UserAccountStatus } from '../../../core/services/system-users.service';
// import { Divider } from "primeng/divider";

@Component({
  selector: 'app-system-users',
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    ButtonModule,
    TagModule,
    CardModule,
    SkeletonModule,
    PaginatorModule,
    DialogModule,
    GlobalUserSearchComponent,
    UserDetailsPanelComponent,
    // RoleManagementFormComponent,
    AccountActionsToolbarComponent,
    CreateUserDialogComponent,
    HouseholdAssignmentComponent,
    // Divider
],
  templateUrl: './system-users.component.html',
  styleUrl: './system-users.component.scss'
})
export class SystemUsersComponent implements OnInit {
  private systemUsersService = inject(SystemUsersService);

  // State signals
  users = this.systemUsersService.users;
  selectedUser = this.systemUsersService.selectedUser;
  isLoading = this.systemUsersService.isLoading;
  totalUsers = this.systemUsersService.totalUsers;

  // Local state
  currentPage = signal<number>(1);
  pageSize = signal<number>(20);
  currentFilters = signal<UserSearchFilters>({});
  detailsDialogVisible = signal<boolean>(false);
  createDialogVisible = signal<boolean>(false);

  // Computed values
  totalPages = computed(() => Math.ceil(this.totalUsers() / this.pageSize()));
  hasUsers = computed(() => this.users().length > 0);

  ngOnInit(): void {
    this.loadUsers();
  }

  /**
   * Load users with current filters
   */
  loadUsers(): void {
    const filters: UserSearchFilters = {
      ...this.currentFilters(),
      page: this.currentPage(),
      pageSize: this.pageSize()
    };

    this.systemUsersService.searchUsers(filters).subscribe({
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }

  /**
   * Handle search event from search component
   */
  onSearch(filters: UserSearchFilters): void {
    this.currentFilters.set(filters);
    this.currentPage.set(1); // Reset to first page on new search
    this.loadUsers();
  }

  /**
   * Handle page change
   */
  onPageChange(event: any): void {
    this.currentPage.set(event.page + 1); // PrimeNG uses 0-based index
    this.pageSize.set(event.rows);
    this.loadUsers();
  }

  /**
   * Select user and show details dialog
   */
  selectUser(user: SystemUser): void {
    this.systemUsersService.selectedUser.set(user);
    this.detailsDialogVisible.set(true);
  }

  /**
   * Close details dialog
   */
  closeDetailsDialog(): void {
    this.detailsDialogVisible.set(false);
    this.systemUsersService.clearSelectedUser();
  }

  /**
   * Handle role update
   */
  onRoleUpdated(updatedUser: SystemUser): void {
    // Service already updates the list, just reload to be sure
    this.loadUsers();
  }

  /**
   * Handle account action completion
   */
  onActionCompleted(): void {
    // Reload users to get updated status
    this.loadUsers();
  }

  /**
   * Get role display label
   */
  getRoleLabel(role: UserRole): string {
    const labels: Record<UserRole, string> = {
      [UserRole.ADMIN]: 'Admin',
      [UserRole.MEMBER]: 'Member',
      [UserRole.DASHBOARD]: 'Dashboard',
      [UserRole.SYSTEM_DEVELOPER]: 'System Dev'
    };
    return labels[role] || role;
  }

  /**
   * Get role severity for tag
   */
  getRoleSeverity(role: UserRole): 'success' | 'info' | 'warning' | 'danger' {
    switch (role) {
      case UserRole.SYSTEM_DEVELOPER:
        return 'danger';
      case UserRole.ADMIN:
        return 'success';
      case UserRole.MEMBER:
        return 'info';
      case UserRole.DASHBOARD:
        return 'warning';
      default:
        return 'info';
    }
  }

  /**
   * Get status display label
   */
  getStatusLabel(status: UserAccountStatus): string {
    const labels: Record<UserAccountStatus, string> = {
      [UserAccountStatus.ACTIVE]: 'Active',
      [UserAccountStatus.INACTIVE]: 'Inactive',
      [UserAccountStatus.LOCKED]: 'Locked',
      [UserAccountStatus.PENDING]: 'Pending'
    };
    return labels[status] || status;
  }

  /**
   * Get status severity for tag
   */
  getStatusSeverity(status: UserAccountStatus): 'success' | 'info' | 'warning' | 'danger' {
    switch (status) {
      case UserAccountStatus.ACTIVE:
        return 'success';
      case UserAccountStatus.INACTIVE:
        return 'warning';
      case UserAccountStatus.LOCKED:
        return 'danger';
      case UserAccountStatus.PENDING:
        return 'info';
      default:
        return 'info';
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: Date | null): string {
    if (!date) return 'Never';
    return new Date(date).toLocaleDateString();
  }

  /**
   * Show create user dialog
   */
  showCreateDialog(): void {
    this.createDialogVisible.set(true);
  }

  /**
   * Hide create user dialog
   */
  hideCreateDialog(): void {
    this.createDialogVisible.set(false);
  }

  /**
   * Handle user created event
   */
  onUserCreated(): void {
    // Reload users to show the new user
    this.loadUsers();
    this.hideCreateDialog();
  }
}
