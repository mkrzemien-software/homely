import { Component, input, output, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

// Services and Interfaces
import { SystemUser, UserRole, RoleUpdateRequest, SystemUsersService } from '../../../../../core/services/system-users.service';

interface RoleOption {
  label: string;
  value: UserRole;
  description: string;
}

@Component({
  selector: 'app-role-management-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    DropdownModule,
    ButtonModule,
    MessageModule
  ],
  templateUrl: './role-management-form.component.html',
  styleUrl: './role-management-form.component.scss'
})
export class RoleManagementFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private systemUsersService = inject(SystemUsersService);

  // Inputs
  user = input.required<SystemUser>();

  // Outputs
  roleUpdated = output<SystemUser>();

  // Form
  roleForm!: FormGroup;

  // State
  isSubmitting = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Role options
  roleOptions = signal<RoleOption[]>([
    {
      label: 'Administrator',
      value: UserRole.ADMIN,
      description: 'Full access to household management'
    },
    {
      label: 'Member',
      value: UserRole.MEMBER,
      description: 'Can manage assigned items and tasks'
    },
    {
      label: 'Dashboard',
      value: UserRole.DASHBOARD,
      description: 'Read-only access for display monitors'
    },
    {
      label: 'System Developer',
      value: UserRole.SYSTEM_DEVELOPER,
      description: 'Full system access and management'
    }
  ]);

  ngOnInit(): void {
    this.initializeForm();
  }

  /**
   * Initialize role management form
   */
  private initializeForm(): void {
    this.roleForm = this.fb.group({
      role: [this.user().role, Validators.required]
    });
  }

  /**
   * Submit role update
   */
  onSubmit(): void {
    if (this.roleForm.invalid || this.isSubmitting()) {
      return;
    }

    const newRole = this.roleForm.value.role;

    // Check if role actually changed
    if (newRole === this.user().role) {
      this.errorMessage.set('Role is the same as current role');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const request: RoleUpdateRequest = {
      userId: this.user().id,
      householdId: this.user().householdId,
      newRole: newRole
    };

    this.systemUsersService.updateUserRole(request).subscribe({
      next: (updatedUser) => {
        this.isSubmitting.set(false);
        this.successMessage.set('Role updated successfully');
        this.roleUpdated.emit(updatedUser);

        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (error) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.message || 'Failed to update role');
      }
    });
  }

  /**
   * Cancel role change and reset form
   */
  onCancel(): void {
    this.roleForm.patchValue({
      role: this.user().role
    });
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  /**
   * Check if role has changed from original
   */
  hasRoleChanged(): boolean {
    return this.roleForm.value.role !== this.user().role;
  }

  /**
   * Get description for currently selected role
   */
  getSelectedRoleDescription(): string {
    const selectedRole = this.roleForm.value.role;
    const option = this.roleOptions().find(opt => opt.value === selectedRole);
    return option?.description || '';
  }
}
