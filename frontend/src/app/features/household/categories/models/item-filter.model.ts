import { ItemPriority } from './item.model';

/**
 * Item Filter and Sort Models
 *
 * Based on PRD section 3.4.2:
 * - Sortowanie: po dacie, nazwie, priorytecie
 * - Filtrowanie po kategorii, osobie odpowiedzialnej
 */

/**
 * Sort field options
 */
export type ItemSortField = 'name' | 'lastServiceDate' | 'priority' | 'category' | 'nextService';

/**
 * Sort direction
 */
export type SortDirection = 'asc' | 'desc';

/**
 * Item sort configuration
 */
export interface ItemSort {
  field: ItemSortField;
  direction: SortDirection;
}

/**
 * Item filter configuration
 */
export interface ItemFilter {
  /**
   * Filter by category IDs
   */
  categoryIds?: number[];

  /**
   * Filter by assigned user IDs
   */
  assignedUserIds?: string[];

  /**
   * Filter by priority levels
   */
  priorities?: ItemPriority[];

  /**
   * Search by name or description
   */
  searchText?: string;

  /**
   * Show only active (non-deleted) items
   */
  activeOnly?: boolean;

  /**
   * Show items with upcoming service dates within X days
   */
  upcomingDays?: number;
}

/**
 * Default filter configuration
 */
export const DEFAULT_ITEM_FILTER: ItemFilter = {
  activeOnly: true
};

/**
 * Default sort configuration
 */
export const DEFAULT_ITEM_SORT: ItemSort = {
  field: 'name',
  direction: 'asc'
};

/**
 * Sort field labels for UI
 */
export const SORT_FIELD_LABELS: Record<ItemSortField, string> = {
  name: 'Nazwa',
  lastServiceDate: 'Data ostatniego serwisu',
  priority: 'Priorytet',
  category: 'Kategoria',
  nextService: 'Następny serwis'
};

/**
 * Priority order for sorting (high -> medium -> low)
 */
export const PRIORITY_ORDER: Record<ItemPriority, number> = {
  high: 3,
  medium: 2,
  low: 1
};

/**
 * Helper function to apply filters to items array
 */
export function applyItemFilters<T extends {
  categoryId?: number;
  assignedTo?: string;
  priority?: ItemPriority;
  name?: string;
  description?: string;
  deletedAt?: Date | string | null;
}>(items: T[], filter: ItemFilter): T[] {
  return items.filter(item => {
    // Active only filter
    if (filter.activeOnly && item.deletedAt) {
      return false;
    }

    // Category filter
    if (filter.categoryIds && filter.categoryIds.length > 0) {
      if (!item.categoryId || !filter.categoryIds.includes(item.categoryId)) {
        return false;
      }
    }

    // Assigned user filter
    if (filter.assignedUserIds && filter.assignedUserIds.length > 0) {
      if (!item.assignedTo || !filter.assignedUserIds.includes(item.assignedTo)) {
        return false;
      }
    }

    // Priority filter
    if (filter.priorities && filter.priorities.length > 0) {
      if (!item.priority || !filter.priorities.includes(item.priority)) {
        return false;
      }
    }

    // Search text filter
    if (filter.searchText && filter.searchText.trim()) {
      const searchLower = filter.searchText.toLowerCase();
      const nameMatch = item.name?.toLowerCase().includes(searchLower);
      const descMatch = item.description?.toLowerCase().includes(searchLower);

      if (!nameMatch && !descMatch) {
        return false;
      }
    }

    return true;
  });
}

/**
 * Helper function to apply sorting to items array
 */
export function applyItemSort<T extends {
  name?: string;
  lastServiceDate?: Date | string;
  priority?: ItemPriority;
  categoryId?: number;
  category?: { name: string };
}>(items: T[], sort: ItemSort): T[] {
  return [...items].sort((a, b) => {
    let comparison = 0;

    switch (sort.field) {
      case 'name':
        comparison = (a.name || '').localeCompare(b.name || '');
        break;

      case 'lastServiceDate':
        const dateA = a.lastServiceDate ? new Date(a.lastServiceDate).getTime() : 0;
        const dateB = b.lastServiceDate ? new Date(b.lastServiceDate).getTime() : 0;
        comparison = dateA - dateB;
        break;

      case 'priority':
        const priorityA = PRIORITY_ORDER[a.priority || 'low'];
        const priorityB = PRIORITY_ORDER[b.priority || 'low'];
        comparison = priorityB - priorityA; // High priority first
        break;

      case 'category':
        const categoryA = a.category?.name || '';
        const categoryB = b.category?.name || '';
        comparison = categoryA.localeCompare(categoryB);
        break;

      case 'nextService':
        // This would need calculation from lastServiceDate + interval
        // For now, just use lastServiceDate
        const nextA = a.lastServiceDate ? new Date(a.lastServiceDate).getTime() : 0;
        const nextB = b.lastServiceDate ? new Date(b.lastServiceDate).getTime() : 0;
        comparison = nextA - nextB;
        break;

      default:
        comparison = 0;
    }

    return sort.direction === 'asc' ? comparison : -comparison;
  });
}

/**
 * Group items by category
 */
export function groupItemsByCategory<T extends {
  categoryId: number;
  category?: { name: string };
}>(items: T[]): Map<number, T[]> {
  const grouped = new Map<number, T[]>();

  items.forEach(item => {
    const categoryId = item.categoryId;
    if (!grouped.has(categoryId)) {
      grouped.set(categoryId, []);
    }
    grouped.get(categoryId)!.push(item);
  });

  return grouped;
}
