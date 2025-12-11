/**
 * Item Model
 *
 * Represents a device or visit that needs to be tracked in the household.
 * Based on PRD section 3.2 - Zarządzanie urządzeniami i wizytami
 */

/**
 * Priority levels for items
 */
export type ItemPriority = 'low' | 'medium' | 'high';

/**
 * Interval unit for recurring tasks
 */
export type IntervalUnit = 'days' | 'weeks' | 'months' | 'years';

/**
 * Main Item interface
 */
export interface Item {
  /**
   * Unique identifier (UUID from backend)
   */
  id: string;

  /**
   * Household ID this item belongs to
   */
  householdId: string;

  /**
   * Category ID (technical inspections, waste collection, medical visits) - UUID
   */
  categoryId: string;

  /**
   * Category details (populated from backend)
   */
  category?: Category;

  /**
   * Name of the device/visit type
   * Example: "Kocioł gazowy", "Pralka", "Wizyta u dentysty"
   */
  name: string;

  /**
   * Description/notes
   */
  description?: string;

  /**
   * User ID of the responsible household member
   */
  assignedTo?: string;

  /**
   * Assigned member details (populated from backend)
   */
  assignedMember?: HouseholdMember;

  /**
   * Priority level
   */
  priority: ItemPriority;

  /**
   * Interval value (combined with intervalUnit)
   * Example: 6 months, 2 years, 30 days
   */
  yearsValue?: number;
  monthsValue?: number;
  weeksValue?: number;
  daysValue?: number;

  /**
   * Date of last service/visit
   */
  lastServiceDate?: Date | string;

  /**
   * Date when item was created
   */
  createdAt: Date | string;

  /**
   * Date when item was last updated
   */
  updatedAt: Date | string;

  /**
   * Soft delete timestamp
   */
  deletedAt?: Date | string | null;
}

/**
 * Category interface
 */
export interface Category {
  id: string;
  categoryTypeId: string;
  categoryTypeName?: string;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: Date | string;
  updatedAt: Date | string;
}

/**
 * Household Member interface (simplified)
 */
export interface HouseholdMember {
  id: string;
  userId: string;
  householdId: string;
  role: 'admin' | 'member' | 'dashboard';
  displayName?: string;
  email?: string;
}

/**
 * DTO for creating a new item
 */
export interface CreateItemDto {
  householdId: string;
  categoryId: string;
  name: string;
  description?: string;
  assignedTo?: string;
  priority: ItemPriority;
  yearsValue?: number;
  monthsValue?: number;
  weeksValue?: number;
  daysValue?: number;
  lastServiceDate?: string;
}

/**
 * DTO for updating an existing item
 */
export interface UpdateItemDto {
  categoryId?: string;
  name?: string;
  description?: string;
  assignedTo?: string;
  priority?: ItemPriority;
  yearsValue?: number;
  monthsValue?: number;
  weeksValue?: number;
  daysValue?: number;
  lastServiceDate?: string;
}

/**
 * Item with grouped category information
 */
export interface ItemWithCategory extends Item {
  categoryName: string;
  categoryColor: string;
}

/**
 * Items grouped by category
 */
export interface ItemsByCategory {
  categoryId: string;
  categoryName: string;
  categoryColor: string;
  categoryIcon: string;
  items: Item[];
  itemCount: number;
}

/**
 * Helper function to get priority label
 */
export function getPriorityLabel(priority: ItemPriority): string {
  const labels: Record<ItemPriority, string> = {
    low: 'Niski',
    medium: 'Średni',
    high: 'Wysoki'
  };
  return labels[priority];
}

/**
 * Helper function to get priority color
 */
export function getPriorityColor(priority: ItemPriority): string {
  const colors: Record<ItemPriority, string> = {
    low: 'success',
    medium: 'warning',
    high: 'danger'
  };
  return colors[priority];
}

/**
 * Helper function to format interval
 */
export function formatInterval(item: Item): string {
  const parts: string[] = [];

  if (item.yearsValue && item.yearsValue > 0) {
    parts.push(`${item.yearsValue} ${item.yearsValue === 1 ? 'rok' : 'lat'}`);
  }
  if (item.monthsValue && item.monthsValue > 0) {
    parts.push(`${item.monthsValue} ${item.monthsValue === 1 ? 'miesiąc' : 'miesięcy'}`);
  }
  if (item.weeksValue && item.weeksValue > 0) {
    parts.push(`${item.weeksValue} ${item.weeksValue === 1 ? 'tydzień' : 'tygodni'}`);
  }
  if (item.daysValue && item.daysValue > 0) {
    parts.push(`${item.daysValue} ${item.daysValue === 1 ? 'dzień' : 'dni'}`);
  }

  return parts.length > 0 ? parts.join(', ') : 'Brak interwału';
}

/**
 * Helper function to calculate next service date
 */
export function calculateNextServiceDate(item: Item): Date | null {
  if (!item.lastServiceDate) {
    return null;
  }

  const lastDate = new Date(item.lastServiceDate);
  const nextDate = new Date(lastDate);

  if (item.yearsValue && item.yearsValue > 0) {
    nextDate.setFullYear(nextDate.getFullYear() + item.yearsValue);
  }
  if (item.monthsValue && item.monthsValue > 0) {
    nextDate.setMonth(nextDate.getMonth() + item.monthsValue);
  }
  if (item.weeksValue && item.weeksValue > 0) {
    nextDate.setDate(nextDate.getDate() + (item.weeksValue * 7));
  }
  if (item.daysValue && item.daysValue > 0) {
    nextDate.setDate(nextDate.getDate() + item.daysValue);
  }

  return nextDate;
}
