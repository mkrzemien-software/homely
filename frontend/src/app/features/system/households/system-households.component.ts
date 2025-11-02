import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { PaginatorModule } from 'primeng/paginator';
import { SidebarModule } from 'primeng/sidebar';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

// PrimeNG Services
import { MessageService } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';

// Components
import { CreateHouseholdDialogComponent } from './components/create-household-dialog/create-household-dialog.component';

// Services and Interfaces
import {
  SystemHouseholdsService,
  SystemHousehold,
  HouseholdSearchFilters,
  SubscriptionStatus,
  HouseholdStats
} from '../../../core/services/system-households.service';

@Component({
  selector: 'app-system-households',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    TagModule,
    CardModule,
    SkeletonModule,
    PaginatorModule,
    SidebarModule,
    DialogModule,
    InputTextModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    CreateHouseholdDialogComponent
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './system-households.component.html',
  styleUrl: './system-households.component.scss'
})
export class SystemHouseholdsComponent implements OnInit {
  private systemHouseholdsService = inject(SystemHouseholdsService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  // State signals
  households = this.systemHouseholdsService.households;
  selectedHousehold = this.systemHouseholdsService.selectedHousehold;
  householdStats = this.systemHouseholdsService.householdStats;
  isLoading = this.systemHouseholdsService.isLoading;
  totalHouseholds = this.systemHouseholdsService.totalHouseholds;

  // Local state
  currentPage = signal<number>(1);
  pageSize = signal<number>(20);
  currentFilters = signal<HouseholdSearchFilters>({});
  detailsSidebarVisible = signal<boolean>(false);
  createDialogVisible = signal<boolean>(false);
  searchTerm = '';

  // Computed values
  totalPages = computed(() => Math.ceil(this.totalHouseholds() / this.pageSize()));
  hasHouseholds = computed(() => this.households().length > 0);

  ngOnInit(): void {
    this.loadHouseholds();
    this.loadStats();
  }

  /**
   * Load households with current filters
   */
  loadHouseholds(): void {
    const filters: HouseholdSearchFilters = {
      ...this.currentFilters(),
      page: this.currentPage(),
      pageSize: this.pageSize()
    };

    this.systemHouseholdsService.searchHouseholds(filters).subscribe({
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Failed to load households'
        });
      }
    });
  }

  /**
   * Load household statistics
   */
  loadStats(): void {
    this.systemHouseholdsService.getHouseholdStats().subscribe({
      error: (error) => {
        console.error('Error loading stats:', error);
      }
    });
  }

  /**
   * Handle search event
   */
  onSearch(): void {
    const searchValue = this.searchTerm.trim();
    const filters: HouseholdSearchFilters = {
      ...this.currentFilters(),
      searchTerm: searchValue || undefined
    };

    this.currentFilters.set(filters);
    this.currentPage.set(1); // Reset to first page on new search
    this.loadHouseholds();
  }

  /**
   * Clear search and filters
   */
  clearSearch(): void {
    this.searchTerm = '';
    this.currentFilters.set({});
    this.currentPage.set(1);
    this.loadHouseholds();
  }

  /**
   * Handle page change
   */
  onPageChange(event: any): void {
    this.currentPage.set(event.page + 1); // PrimeNG uses 0-based index
    this.pageSize.set(event.rows);
    this.loadHouseholds();
  }

  /**
   * Select household and show details sidebar
   */
  viewHouseholdDetails(household: SystemHousehold): void {
    this.systemHouseholdsService.getHouseholdDetails(household.id).subscribe({
      next: () => {
        this.detailsSidebarVisible.set(true);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Failed to load household details'
        });
      }
    });
  }

  /**
   * Close details sidebar
   */
  closeDetailsSidebar(): void {
    this.detailsSidebarVisible.set(false);
    this.systemHouseholdsService.clearSelectedHousehold();
  }

  /**
   * Show create household dialog
   */
  showCreateDialog(): void {
    this.createDialogVisible.set(true);
  }

  /**
   * Hide create household dialog
   */
  hideCreateDialog(): void {
    this.createDialogVisible.set(false);
  }

  /**
   * Delete household
   */
  deleteHousehold(household: SystemHousehold, event: Event): void {
    event.stopPropagation();

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `Are you sure you want to delete household "${household.name}"?`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.systemHouseholdsService.deleteHousehold(household.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Household deleted successfully'
            });
            this.loadHouseholds();
            this.loadStats();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.message || 'Failed to delete household'
            });
          }
        });
      }
    });
  }

  /**
   * Get subscription status display label
   */
  getStatusLabel(status: SubscriptionStatus): string {
    const labels: Record<SubscriptionStatus, string> = {
      [SubscriptionStatus.FREE]: 'Free',
      [SubscriptionStatus.ACTIVE]: 'Active',
      [SubscriptionStatus.CANCELLED]: 'Cancelled',
      [SubscriptionStatus.EXPIRED]: 'Expired'
    };
    return labels[status] || status;
  }

  /**
   * Get subscription status severity for tag
   */
  getStatusSeverity(status: SubscriptionStatus): 'success' | 'info' | 'warning' | 'danger' {
    switch (status) {
      case SubscriptionStatus.ACTIVE:
        return 'success';
      case SubscriptionStatus.FREE:
        return 'info';
      case SubscriptionStatus.CANCELLED:
        return 'warning';
      case SubscriptionStatus.EXPIRED:
        return 'danger';
      default:
        return 'info';
    }
  }

  /**
   * Format date for display
   */
  formatDate(date: Date | null | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString();
  }

  /**
   * Refresh data
   */
  refresh(): void {
    this.loadHouseholds();
    this.loadStats();
    this.messageService.add({
      severity: 'info',
      summary: 'Refreshed',
      detail: 'Data has been refreshed'
    });
  }
}
