import { Component, input, output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

// PrimeNG Components
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';

// Services and Interfaces
import { SystemUser, UserAccountStatus, SystemUsersService } from '../../../../../core/services/system-users.service';

@Component({
  selector: 'app-account-actions-toolbar',
  imports: [
    CommonModule,
    ToolbarModule,
    ButtonModule,
    ConfirmDialogModule,
    ToastModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './account-actions-toolbar.component.html',
  styleUrl: './account-actions-toolbar.component.scss'
})
export class AccountActionsToolbarComponent {
  private systemUsersService = inject(SystemUsersService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);

  // Inputs
  user = input.required<SystemUser>();

  // Outputs
  actionCompleted = output<void>();

  // State
  isProcessing = signal<boolean>(false);

  // Expose enum for template
  UserAccountStatus = UserAccountStatus;

  /**
   * Reset user password with confirmation
   */
  onResetPassword(): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to reset password for ${this.user().email}? An email with reset instructions will be sent to the user.`,
      header: 'Reset Password Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Yes, Reset Password',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.executePasswordReset();
      }
    });
  }

  /**
   * Execute password reset
   */
  private executePasswordReset(): void {
    this.isProcessing.set(true);

    this.systemUsersService.resetUserPassword({
      userId: this.user().id,
      sendEmail: true
    }).subscribe({
      next: (response) => {
        this.isProcessing.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: response.message || 'Password reset email sent successfully',
          life: 5000
        });
        this.actionCompleted.emit();
      },
      error: (error) => {
        this.isProcessing.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Failed to reset password',
          life: 5000
        });
      }
    });
  }

  /**
   * Unlock user account with confirmation
   */
  onUnlockAccount(): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to unlock account for ${this.user().email}? The user will be able to log in immediately.`,
      header: 'Unlock Account Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Yes, Unlock Account',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-success',
      accept: () => {
        this.executeUnlockAccount();
      }
    });
  }

  /**
   * Execute account unlock
   */
  private executeUnlockAccount(): void {
    this.isProcessing.set(true);

    this.systemUsersService.unlockUserAccount(this.user().id).subscribe({
      next: (response) => {
        this.isProcessing.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: response.message || 'Account unlocked successfully',
          life: 5000
        });
        this.actionCompleted.emit();
      },
      error: (error) => {
        this.isProcessing.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Failed to unlock account',
          life: 5000
        });
      }
    });
  }

  /**
   * Check if account is locked
   */
  isAccountLocked(): boolean {
    return this.user().status === UserAccountStatus.LOCKED;
  }

  /**
   * Check if any action is in progress
   */
  isActionInProgress(): boolean {
    return this.isProcessing();
  }
}
