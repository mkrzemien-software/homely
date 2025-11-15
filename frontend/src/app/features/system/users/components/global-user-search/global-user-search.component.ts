import { Component, inject, output, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';

// PrimeNG Components
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';

// Services and Interfaces
import { UserRole, UserAccountStatus, UserSearchFilters } from '../../../../../core/services/system-users.service';

interface DropdownOption {
  label: string;
  value: string | null;
}

@Component({
  selector: 'app-global-user-search',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    SelectModule,
    IconFieldModule,
    InputIconModule
  ],
  templateUrl: './global-user-search.component.html',
  styleUrl: './global-user-search.component.scss'
})
export class GlobalUserSearchComponent implements OnInit {
  private fb = inject(FormBuilder);

  // Output events
  search = output<UserSearchFilters>();

  // Form
  searchForm!: FormGroup;

  // Dropdown options
  roleOptions = signal<DropdownOption[]>([
    { label: 'All Roles', value: null },
    { label: 'Administrator', value: UserRole.ADMIN },
    { label: 'Member', value: UserRole.MEMBER },
    { label: 'Dashboard', value: UserRole.DASHBOARD },
    { label: 'System Developer', value: UserRole.SYSTEM_DEVELOPER }
  ]);

  statusOptions = signal<DropdownOption[]>([
    { label: 'All Statuses', value: null },
    { label: 'Active', value: UserAccountStatus.ACTIVE },
    { label: 'Inactive', value: UserAccountStatus.INACTIVE },
    { label: 'Locked', value: UserAccountStatus.LOCKED },
    { label: 'Pending', value: UserAccountStatus.PENDING }
  ]);

  ngOnInit(): void {
    this.initializeForm();
    this.setupSearchListener();
  }

  /**
   * Initialize search form
   */
  private initializeForm(): void {
    this.searchForm = this.fb.group({
      searchTerm: [''],
      role: [null],
      status: [null]
    });
  }

  /**
   * Setup listener for form changes with debounce
   */
  private setupSearchListener(): void {
    this.searchForm.valueChanges
      .pipe(
        debounceTime(400),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.performSearch();
      });
  }

  /**
   * Perform search with current form values
   */
  performSearch(): void {
    const filters: UserSearchFilters = {
      searchTerm: this.searchForm.value.searchTerm || undefined,
      role: this.searchForm.value.role || undefined,
      status: this.searchForm.value.status || undefined,
      page: 1,
      pageSize: 20
    };

    this.search.emit(filters);
  }

  /**
   * Reset search form
   */
  resetSearch(): void {
    this.searchForm.reset({
      searchTerm: '',
      role: null,
      status: null
    });
    this.performSearch();
  }

  /**
   * Check if form has any filters applied
   */
  hasFilters(): boolean {
    return !!(
      this.searchForm.value.searchTerm ||
      this.searchForm.value.role ||
      this.searchForm.value.status
    );
  }
}
