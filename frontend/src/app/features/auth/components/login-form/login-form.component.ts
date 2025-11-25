import { Component, inject, signal, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageModule } from 'primeng/message';

// Services
import { AuthService, LoginCredentials } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    CheckboxModule,
    MessageModule
  ],
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.scss'
})
export class LoginFormComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  // Signals for component state
  isLoading = this.authService.isLoading;
  errorMessage = signal<string | null>(null);

  // Output event for successful login (optional, for parent component)
  loginSuccess = output<void>();

  // Form group
  loginForm: FormGroup;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      rememberMe: [false]
    });
  }

  /**
   * Handle form submission
   */
  onSubmit(): void {
    // Clear previous error message
    this.errorMessage.set(null);

    // Validate form
    if (this.loginForm.invalid) {
      this.markFormGroupTouched(this.loginForm);
      return;
    }

    // Get form values
    const credentials: LoginCredentials = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
      rememberMe: this.loginForm.value.rememberMe
    };

    // Call auth service
    this.authService.login(credentials).subscribe({
      next: (response) => {
        // Login successful
        this.loginSuccess.emit();

        // Use user data from response directly (signal may not be updated yet due to queueMicrotask)
        const user = response.data?.user;
        if (user?.householdId) {
          this.router.navigate([`/${user.householdId}/dashboard`]);
        } else {
          // If user has no household, redirect to a no-household page or error
          console.warn('User logged in but has no household assigned');
          this.router.navigate(['/auth/login']);
          this.errorMessage.set('Nie masz przypisanego gospodarstwa domowego. Skontaktuj się z administratorem.');
        }
      },
      error: (error: Error) => {
        // Display error message
        this.errorMessage.set(error.message);

        // Clear password field on error
        this.loginForm.patchValue({ password: '' });

        // Focus on password field
        setTimeout(() => {
          const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement;
          passwordInput?.focus();
        }, 100);
      }
    });
  }

  /**
   * Check if field has error and is touched
   */
  hasError(fieldName: string, errorType?: string): boolean {
    const field = this.loginForm.get(fieldName);
    if (!field) return false;

    if (errorType) {
      return !!(field.hasError(errorType) && (field.dirty || field.touched));
    }

    return !!(field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for field
   */
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.hasError('email')) {
      return 'Please enter a valid email address';
    }

    if (field.hasError('minlength')) {
      const minLength = field.errors['minlength'].requiredLength;
      return `Password must be at least ${minLength} characters`;
    }

    return 'Invalid field';
  }

  /**
   * Get field label for error messages
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      email: 'Email',
      password: 'Password'
    };
    return labels[fieldName] || fieldName;
  }

  /**
   * Mark all fields in form group as touched to show validation errors
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Handle Enter key press to submit form
   */
  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && this.loginForm.valid) {
      this.onSubmit();
    }
  }

  /**
   * Clear error message
   */
  clearError(): void {
    this.errorMessage.set(null);
  }
}
