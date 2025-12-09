import { Component, input, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from 'primeng/dropdown';
import { SystemUsersService, SystemUser } from '../../../../../core/services/system-users.service';
import { SystemHouseholdsService } from '../../../../../core/services/system-households.service';
import { UserHousehold, AddUserToHouseholdRequest } from '../../models/user-household.model';

interface HouseholdCheckbox {
  householdId: string;
  householdName: string;
  checked: boolean;
  role: 'admin' | 'member' | 'dashboard';
  planTypeName?: string;
}

@Component({
  selector: 'app-household-assignment',
  imports: [
    CommonModule,
    FormsModule,
    CheckboxModule,
    CardModule,
    MessageModule,
    ProgressSpinnerModule,
    DropdownModule
  ],
  templateUrl: './household-assignment.component.html',
  styleUrl: './household-assignment.component.scss'
})
export class HouseholdAssignmentComponent {
  // Services
  private systemUsersService = inject(SystemUsersService);
  private systemHouseholdsService = inject(SystemHouseholdsService);

  // Inputs
  user = input.required<SystemUser>();

  // State signals
  households = signal<HouseholdCheckbox[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  // Role options for dropdown
  roleOptions = [
    { label: 'Administrator', value: 'admin' },
    { label: 'Member', value: 'member' },
    { label: 'Dashboard', value: 'dashboard' }
  ];

  constructor() {
    // Load households when user changes
    effect(() => {
      const currentUser = this.user();
      if (currentUser) {
        this.loadHouseholds();
      }
    });
  }

  /**
   * Load all households and user's household assignments
   */
  private loadHouseholds(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const userId = this.user().id;

    // Load all households from system
    this.systemHouseholdsService.searchHouseholds({}).subscribe({
      next: (paginatedResponse) => {
        const allHouseholds = paginatedResponse.households;

        // Load user's households
        this.systemUsersService.getUserHouseholds(userId).subscribe({
          next: (userHouseholds: UserHousehold[]) => {
            // Map all households with checked status
            const householdCheckboxes: HouseholdCheckbox[] = allHouseholds.map(h => {
              const userHousehold = userHouseholds.find(uh => uh.householdId === h.id);
              return {
                householdId: h.id,
                householdName: h.name,
                checked: !!userHousehold,
                role: userHousehold?.role || 'member',
                planTypeName: h.planName
              };
            });

            this.households.set(householdCheckboxes);
            this.isLoading.set(false);
          },
          error: (error) => {
            this.errorMessage.set('Nie udało się załadować przypisań gospodarstw użytkownika');
            this.isLoading.set(false);
            console.error('Error loading user households:', error);
          }
        });
      },
      error: (error) => {
        this.errorMessage.set('Nie udało się załadować listy gospodarstw');
        this.isLoading.set(false);
        console.error('Error loading households:', error);
      }
    });
  }

  /**
   * Handle checkbox change
   */
  onHouseholdCheckboxChange(household: HouseholdCheckbox): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const userId = this.user().id;

    if (household.checked) {
      // Add user to household
      const request: AddUserToHouseholdRequest = {
        householdId: household.householdId,
        role: household.role
      };

      this.systemUsersService.addUserToHousehold(userId, request).subscribe({
        next: () => {
          this.successMessage.set(`Użytkownik został dodany do gospodarstwa "${household.householdName}"`);
          setTimeout(() => this.successMessage.set(null), 3000);
        },
        error: (error) => {
          household.checked = false; // Revert checkbox
          this.errorMessage.set(error.message || 'Nie udało się dodać użytkownika do gospodarstwa');
          console.error('Error adding user to household:', error);
        }
      });
    } else {
      // Remove user from household
      this.systemUsersService.removeUserFromHousehold(userId, household.householdId).subscribe({
        next: () => {
          this.successMessage.set(`Użytkownik został usunięty z gospodarstwa "${household.householdName}"`);
          setTimeout(() => this.successMessage.set(null), 3000);
        },
        error: (error) => {
          household.checked = true; // Revert checkbox
          this.errorMessage.set(error.message || 'Nie udało się usunąć użytkownika z gospodarstwa');
          console.error('Error removing user from household:', error);
        }
      });
    }
  }

  /**
   * Handle role change for a household
   */
  onRoleChange(household: HouseholdCheckbox): void {
    if (!household.checked) {
      return; // Only update role if user is assigned to the household
    }

    this.errorMessage.set(null);
    this.successMessage.set(null);

    const userId = this.user().id;

    // Remove and re-add with new role
    this.systemUsersService.removeUserFromHousehold(userId, household.householdId).subscribe({
      next: () => {
        const request: AddUserToHouseholdRequest = {
          householdId: household.householdId,
          role: household.role
        };

        this.systemUsersService.addUserToHousehold(userId, request).subscribe({
          next: () => {
            this.successMessage.set(`Rola użytkownika w gospodarstwie "${household.householdName}" została zaktualizowana`);
            setTimeout(() => this.successMessage.set(null), 3000);
          },
          error: (error) => {
            this.errorMessage.set('Nie udało się zaktualizować roli użytkownika');
            this.loadHouseholds(); // Reload to reset state
            console.error('Error updating user role:', error);
          }
        });
      },
      error: (error) => {
        this.errorMessage.set('Nie udało się zaktualizować roli użytkownika');
        this.loadHouseholds(); // Reload to reset state
        console.error('Error updating user role:', error);
      }
    });
  }
}
