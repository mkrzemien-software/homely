/**
 * Task Models
 *
 * Models for tasks (task templates) based on API plan
 * See: .ai/api-plan.md sections:
 * - GET /tasks
 * - POST /tasks
 * - PUT /tasks/{id}
 * - DELETE /tasks/{id}
 */

/**
 * Priority levels for tasks and events
 */
export enum Priority {
  LOW = 'low',
  MEDIUM = 'medium',
  HIGH = 'high'
}

/**
 * Interval configuration for recurring tasks
 */
export interface TaskInterval {
  /**
   * Number of years
   */
  years: number;

  /**
   * Number of months
   */
  months: number;

  /**
   * Number of weeks
   */
  weeks: number;

  /**
   * Number of days
   */
  days: number;
}

/**
 * Category reference in task
 */
export interface TaskCategory {
  id: number;
  name: string;
  categoryType: {
    id: number;
    name: string;
  };
}

/**
 * User reference in task
 */
export interface TaskUser {
  id: string;
  firstName: string;
  lastName: string;
}

/**
 * Task interface (Task Template)
 * Represents a task template from which events (occurrences) are created
 */
export interface Task {
  /**
   * Unique identifier
   */
  id: string;

  /**
   * Household ID
   */
  householdId: string;

  /**
   * Category (subcategory) details
   */
  category: TaskCategory;

  /**
   * Task name
   * Example: "Przegląd roczny", "Wymiana oleju"
   */
  name: string;

  /**
   * Task description
   */
  description: string;

  /**
   * Recurrence interval (optional)
   * If provided, events will be automatically generated on completion
   */
  interval: TaskInterval | null;

  /**
   * Task priority
   */
  priority: Priority;

  /**
   * Additional notes
   */
  notes: string | null;

  /**
   * Whether task is active
   */
  isActive: boolean;

  /**
   * User who created the task
   */
  createdBy: TaskUser;

  /**
   * Creation timestamp
   */
  createdAt: string;

  /**
   * Last update timestamp
   */
  updatedAt?: string;
}

/**
 * API Response for tasks list with pagination
 */
export interface TasksResponse {
  data: Task[];
  pagination: {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    itemsPerPage: number;
  };
}

/**
 * DTO for creating a new task
 */
export interface CreateTaskDto {
  /**
   * Household ID
   */
  householdId: string;

  /**
   * Category (subcategory) ID
   */
  categoryId: number;

  /**
   * Task name (max 100 characters)
   */
  name: string;

  /**
   * Task description
   */
  description?: string;

  /**
   * Interval - years value
   */
  yearsValue?: number;

  /**
   * Interval - months value
   */
  monthsValue?: number;

  /**
   * Interval - weeks value
   */
  weeksValue?: number;

  /**
   * Interval - days value
   */
  daysValue?: number;

  /**
   * Start date for the first event in the series (ISO string)
   */
  startDate?: string;

  /**
   * Task priority
   */
  priority: Priority;

  /**
   * Additional notes
   */
  notes?: string;

  /**
   * Default user assignment for events generated from this task template (optional)
   */
  assignedTo?: string;

  /**
   * ID of user creating the task
   */
  createdBy: string;
}

/**
 * DTO for updating an existing task
 */
export interface UpdateTaskDto {
  /**
   * Category (subcategory) ID
   */
  categoryId?: number;

  /**
   * Task name (max 100 characters)
   */
  name?: string;

  /**
   * Task description
   */
  description?: string;

  /**
   * Interval - years value
   */
  yearsValue?: number;

  /**
   * Interval - months value
   */
  monthsValue?: number;

  /**
   * Interval - weeks value
   */
  weeksValue?: number;

  /**
   * Interval - days value
   */
  daysValue?: number;

  /**
   * Task priority
   */
  priority?: Priority;

  /**
   * Additional notes
   */
  notes?: string;

  /**
   * Whether task is active
   */
  isActive?: boolean;
}

/**
 * Query parameters for GET /tasks
 */
export interface TasksQueryParams {
  /**
   * Filter by household ID
   */
  householdId?: string;

  /**
   * Filter by category (subcategory) ID
   */
  categoryId?: number;

  /**
   * Filter by priority
   */
  priority?: Priority;

  /**
   * Filter by tasks with interval
   */
  hasInterval?: boolean;

  /**
   * Filter by active status
   */
  isActive?: boolean;

  /**
   * Sort by field
   */
  sortBy?: 'name' | 'priority' | 'createdAt';

  /**
   * Sort order
   */
  sortOrder?: 'asc' | 'desc';

  /**
   * Page number (default: 1)
   */
  page?: number;

  /**
   * Items per page (default: 20, max: 100)
   */
  limit?: number;
}

/**
 * Helper function to check if task has interval
 */
export function hasInterval(task: Task): boolean {
  if (!task.interval) return false;
  return (
    task.interval.years > 0 ||
    task.interval.months > 0 ||
    task.interval.weeks > 0 ||
    task.interval.days > 0
  );
}

/**
 * Helper function to format interval as human-readable string
 */
export function formatInterval(interval: TaskInterval | null): string {
  if (!interval) return 'Jednorazowe';

  const parts: string[] = [];
  if (interval.years > 0) {
    parts.push(`${interval.years} ${interval.years === 1 ? 'rok' : interval.years < 5 ? 'lata' : 'lat'}`);
  }
  if (interval.months > 0) {
    parts.push(`${interval.months} ${interval.months === 1 ? 'miesiąc' : interval.months < 5 ? 'miesiące' : 'miesięcy'}`);
  }
  if (interval.weeks > 0) {
    parts.push(`${interval.weeks} ${interval.weeks === 1 ? 'tydzień' : interval.weeks < 5 ? 'tygodnie' : 'tygodni'}`);
  }
  if (interval.days > 0) {
    parts.push(`${interval.days} ${interval.days === 1 ? 'dzień' : 'dni'}`);
  }

  return parts.length > 0 ? parts.join(', ') : 'Jednorazowe';
}

/**
 * Helper function to get priority label
 */
export function getPriorityLabel(priority: Priority): string {
  const labels: Record<Priority, string> = {
    [Priority.LOW]: 'Niski',
    [Priority.MEDIUM]: 'Średni',
    [Priority.HIGH]: 'Wysoki'
  };
  return labels[priority];
}

/**
 * Helper function to get priority severity for PrimeNG components
 */
export function getPrioritySeverity(priority: Priority): 'success' | 'warn' | 'danger' {
  const severities: Record<Priority, 'success' | 'warn' | 'danger'> = {
    [Priority.LOW]: 'success',
    [Priority.MEDIUM]: 'warn',
    [Priority.HIGH]: 'danger'
  };
  return severities[priority];
}
