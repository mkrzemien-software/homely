import { Component, input, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { TableModule } from 'primeng/table';
import { SkeletonModule } from 'primeng/skeleton';

// Services and Interfaces
import { SystemUser, UserActivity, SystemUsersService, UserRole, UserAccountStatus } from '../../../../../core/services/system-users.service';

@Component({
  selector: 'app-user-details-panel',
  imports: [
    CommonModule,
    CardModule,
    TagModule,
    DividerModule,
    TableModule,
    SkeletonModule
  ],
  templateUrl: './user-details-panel.component.html',
  styleUrl: './user-details-panel.component.scss'
})
export class UserDetailsPanelComponent implements OnInit {
  private systemUsersService = inject(SystemUsersService);

  // Input
  user = input.required<SystemUser>();

  // State
  activityHistory = signal<UserActivity[]>([]);
  isLoadingActivity = signal<boolean>(false);

  ngOnInit(): void {
    this.loadUserActivity();
  }

  /**
   * Load user activity history
   */
  private loadUserActivity(): void {
    const userId = this.user().id;
    this.isLoadingActivity.set(true);

    this.systemUsersService.getUserActivity(userId, 10).subscribe({
      next: (activities) => {
        this.activityHistory.set(activities);
        this.isLoadingActivity.set(false);
      },
      error: (error) => {
        console.error('Error loading user activity:', error);
        this.isLoadingActivity.set(false);
      }
    });
  }

  /**
   * Get role display label
   */
  getRoleLabel(role: UserRole): string {
    const labels: Record<UserRole, string> = {
      [UserRole.ADMIN]: 'Administrator',
      [UserRole.MEMBER]: 'Member',
      [UserRole.DASHBOARD]: 'Dashboard',
      [UserRole.SYSTEM_DEVELOPER]: 'System Developer'
    };
    return labels[role] || role;
  }

  /**
   * Get role severity for tag component
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
   * Get status severity for tag component
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
    return new Date(date).toLocaleString();
  }

  /**
   * Get user initials for avatar
   */
  getUserInitials(): string {
    const user = this.user();
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  }
}
