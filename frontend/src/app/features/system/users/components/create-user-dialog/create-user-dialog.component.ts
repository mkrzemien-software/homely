import { Component, inject, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// PrimeNG Components
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';

// Services
import { SystemUsersService, CreateUserRequest, UserRole } from '../../../../../core/services/system-users.service';
import { SystemHouseholdsService } from '../../../../../core/services/system-households.service';

/**
 * CreateUserDialogComponent
 *
 * Dialog component for creating new users in the system.
 * Only accessible to system developers/admins.
 *
 * Features:
 * - Email, firstName, lastName, password fields with validation
 * - Optional household assignment
 * - Optional role assignment (defaults to MEMBER)
 * - Real-time validation feedback
 * - Loading states during submission
 * - Success/error message handling
 *
 * @example
 * <app-create-user-dialog
 *   (userCreated)="onUserCreated()"
 *   (dialogClosed)="onDialogClosed()">
 * </app-create-user-dialog>
 */
@Component({
  selector: 'app-create-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    SelectModule,
    MessageModule
  ],
  templateUrl: './create-user-dialog.component.html',
  styleUrl: './create-user-dialog.component.scss'
})
export class CreateUserDialogComponent {
  private fb = inject(FormBuilder);
  private systemUsersService = inject(SystemUsersService);
  private systemHouseholdsService = inject(SystemHouseholdsService);

  // Signals
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Output events
  userCreated = output<void>();
  dialogClosed = output<void>();

  // Form
  createUserForm: FormGroup;

  // Role options
  roleOptions = signal([
    { label: 'Administrator', value: UserRole.ADMIN },
    { label: 'Członek', value: UserRole.MEMBER },
    { label: 'Dashboard', value: UserRole.DASHBOARD }
  ]);

  // Household options (loaded from service)
  householdOptions = signal<Array<{ label: string; value: string }>>([]);

  constructor() {
    this.createUserForm = this.fb.group({
      email: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
      householdId: [''],
      role: [UserRole.MEMBER]
    });

    // Load households for dropdown
    this.loadHouseholds();
  }

  /**
   * Load households for dropdown selection
   */
  private loadHouseholds(): void {
    this.systemHouseholdsService.searchHouseholds({}).subscribe({
      next: (response) => {
        const options = response.households.map(h => ({
          label: h.name,
          value: h.id
        }));
        // Add empty option for "no household"
        this.householdOptions.set([
          { label: 'Brak przypisania', value: '' },
          ...options
        ]);
      },
      error: (error) => {
        console.error('Error loading households:', error);
        // Set empty array if loading fails
        this.householdOptions.set([{ label: 'Brak przypisania', value: '' }]);
      }
    });
  }

  /**
   * Submit form to create new user
   */
  onSubmit(): void {
    if (this.createUserForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.createUserForm.controls).forEach(key => {
        this.createUserForm.controls[key].markAsTouched();
      });
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const formValue = this.createUserForm.value;
    const request: CreateUserRequest = {
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      password: formValue.password,
      householdId: formValue.householdId || undefined,
      role: formValue.role || UserRole.MEMBER
    };

    this.systemUsersService.createUser(request).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.successMessage.set('Użytkownik został pomyślnie utworzony!');
        this.createUserForm.reset({
          role: UserRole.MEMBER,
          householdId: ''
        });

        // Emit success event
        setTimeout(() => {
          this.userCreated.emit();
          this.closeDialog();
        }, 1500);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Nie udało się utworzyć użytkownika');
      }
    });
  }

  /**
   * Close dialog and reset form
   */
  closeDialog(): void {
    this.createUserForm.reset({
      role: UserRole.MEMBER,
      householdId: ''
    });
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.dialogClosed.emit();
  }

  /**
   * Get form control for template access
   */
  get controls() {
    return this.createUserForm.controls;
  }
}
