import { Component, signal, output, input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Components
import { DropdownModule } from 'primeng/dropdown';

/**
 * Date range option interface
 */
export interface DateRangeOption {
  /**
   * Display label
   */
  label: string;

  /**
   * Number of days
   */
  value: 7 | 14 | 30;

  /**
   * Icon (optional)
   */
  icon?: string;
}

/**
 * DateRangeSelectorComponent
 *
 * Dropdown component for selecting date range (7, 14, or 30 days).
 * Used in dashboard events list to filter upcoming events.
 *
 * Based on UI Plan (ui-plan.md line 232):
 * - Dropdown wyboru zakresu: 7 dni (domyślnie), 14 dni, miesiąc
 *
 * Features:
 * - PrimeNG Dropdown with 3 predefined options
 * - Default value: 7 days
 * - Emits change event when selection changes
 * - Responsive design
 * - Accessible (keyboard navigation, ARIA labels)
 *
 * @example
 * <app-date-range-selector
 *   [selectedDays]="selectedDays()"
 *   (rangeChange)="onRangeChange($event)">
 * </app-date-range-selector>
 */
@Component({
  selector: 'app-date-range-selector',
  imports: [CommonModule, FormsModule, DropdownModule],
  templateUrl: './date-range-selector.component.html',
  styleUrl: './date-range-selector.component.scss'
})
export class DateRangeSelectorComponent implements OnInit {
  /**
   * Currently selected number of days (input)
   * Default: 7
   */
  selectedDays = input<7 | 14 | 30>(7);

  /**
   * Output event when range changes
   * Emits the new number of days
   */
  rangeChange = output<7 | 14 | 30>();

  /**
   * Currently selected option (internal state)
   */
  selectedOption = signal<DateRangeOption | null>(null);

  /**
   * Available date range options
   */
  readonly dateRangeOptions: DateRangeOption[] = [
    {
      label: '7 dni',
      value: 7,
      icon: 'pi pi-calendar'
    },
    {
      label: '14 dni',
      value: 14,
      icon: 'pi pi-calendar'
    },
    {
      label: 'Miesiąc (30 dni)',
      value: 30,
      icon: 'pi pi-calendar'
    }
  ];

  ngOnInit(): void {
    // Set initial selected option based on input
    const initial = this.dateRangeOptions.find(opt => opt.value === this.selectedDays());
    if (initial) {
      this.selectedOption.set(initial);
    }
  }

  /**
   * Handle dropdown change event
   * Emits new value to parent component
   *
   * @param event - Dropdown change event
   */
  onRangeChange(event: any): void {
    const selectedOption = event.value as DateRangeOption;
    if (selectedOption) {
      this.selectedOption.set(selectedOption);
      this.rangeChange.emit(selectedOption.value);
    }
  }

  /**
   * Get current selected days value
   * Used for displaying in UI
   */
  getCurrentDays(): number {
    return this.selectedOption()?.value || this.selectedDays();
  }

  /**
   * Get current selected label
   * Used for accessibility
   */
  getCurrentLabel(): string {
    return this.selectedOption()?.label || '7 dni';
  }
}
