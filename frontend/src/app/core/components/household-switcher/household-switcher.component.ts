import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DropdownModule } from 'primeng/dropdown';
import { FormsModule } from '@angular/forms';
import { AuthService, UserHousehold } from '../../services/auth.service';

interface HouseholdOption {
  label: string;
  value: string;
  role: string;
}

@Component({
  selector: 'app-household-switcher',
  imports: [
    CommonModule,
    DropdownModule,
    FormsModule
  ],
  templateUrl: './household-switcher.component.html',
  styleUrl: './household-switcher.component.scss'
})
export class HouseholdSwitcherComponent {
  private authService = inject(AuthService);

  // Get households from AuthService
  households = this.authService.userHouseholds;
  activeHouseholdId = this.authService.activeHouseholdId;

  // Computed dropdown options
  householdOptions = computed<HouseholdOption[]>(() => {
    return this.households().map(h => ({
      label: h.householdName,
      value: h.householdId,
      role: h.role
    }));
  });

  /**
   * Handle household selection change
   */
  onHouseholdChange(householdId: string): void {
    console.log('Household change event:', householdId); // Debug log
    console.log('Current active household ID:', this.activeHouseholdId()); // Debug log

    if (householdId && householdId !== this.activeHouseholdId()) {
      console.log('Setting active household to:', householdId); // Debug log
      this.authService.setActiveHousehold(householdId);
    } else {
      console.log('Household not changed - same as current or invalid'); // Debug log
    }
  }

  /**
   * Get role label in Polish
   */
  getRoleLabel(role: string): string {
    const roleLabels: Record<string, string> = {
      'admin': 'Administrator',
      'member': 'Członek',
      'dashboard': 'Dashboard',
      'system_developer': 'Developer'
    };
    return roleLabels[role] || role;
  }
}
