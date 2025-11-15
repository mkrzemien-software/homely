/**
 * Event Models
 *
 * Models for events (concrete task occurrences) based on API plan
 * See: .ai/prd.md and .ai/api-plan.md
 */

import { Priority, TaskInterval, TaskCategory, TaskUser } from '../../tasks/models/task.model';

/**
 * Event status
 */
export enum EventStatus {
  PENDING = 'pending',
  COMPLETED = 'completed',
  POSTPONED = 'postponed',
  CANCELLED = 'cancelled'
}

/**
 * Task reference in event
 */
export interface EventTask {
  id: string;
  name: string;
  description: string;
  interval: TaskInterval | null;
  priority: Priority;
  category: TaskCategory;
}

/**
 * Event interface (Concrete task occurrence)
 * Represents a specific scheduled occurrence of a task with an assigned date
 */
export interface Event {
  /**
   * Unique identifier
   */
  id: string;

  /**
   * Task ID
   */
  taskId: string;

  /**
   * Task name
   */
  taskName: string;

  /**
   * Household ID
   */
  householdId: string;

  /**
   * Household name
   */
  householdName: string | null;

  /**
   * User ID assigned to this event
   */
  assignedTo: string;

  /**
   * Due date
   */
  dueDate: string;

  /**
   * Event status
   */
  status: EventStatus;

  /**
   * Event priority (inherited from task, can be changed)
   */
  priority: Priority;

  /**
   * Completion date (when status is completed)
   */
  completionDate: string | null;

  /**
   * Notes about completion
   */
  completionNotes: string | null;

  /**
   * Original date before postponement
   */
  postponedFromDate: string | null;

  /**
   * Postponement reason (when status is postponed)
   */
  postponeReason: string | null;

  /**
   * General notes
   */
  notes: string | null;

  /**
   * User ID who created the event
   */
  createdBy: string;

  /**
   * Creation timestamp
   */
  createdAt: string;

  /**
   * Last update timestamp
   */
  updatedAt: string;

  /**
   * Whether the event is overdue
   */
  isOverdue: boolean;

  /**
   * Days until due (negative if overdue)
   */
  daysUntilDue: number;
}

/**
 * API Response for events list with pagination
 */
export interface EventsResponse {
  data: Event[];
  pagination: {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    itemsPerPage: number;
  };
}

/**
 * DTO for creating a new event
 * Note: Events don't have their own title - they display the associated Task's name
 */
export interface CreateEventDto {
  /**
   * Household ID
   */
  householdId: string;

  /**
   * Task ID (template, required)
   * The event will display the task's name as its title
   */
  taskId: string;

  /**
   * Assigned user ID
   */
  assignedTo?: string;

  /**
   * Due date (ISO 8601 format)
   */
  dueDate: string;

  /**
   * Event notes
   */
  notes?: string;

  /**
   * Event priority (optional, defaults to task's priority)
   */
  priority?: Priority;

  /**
   * ID of user creating the event
   */
  createdBy: string;
}

/**
 * DTO for updating an existing event
 */
export interface UpdateEventDto {
  /**
   * Assigned user ID
   */
  assignedToId?: string;

  /**
   * Due date (ISO 8601 format)
   */
  dueDate?: string;

  /**
   * Event priority
   */
  priority?: Priority;

  /**
   * Additional notes
   */
  notes?: string;

  /**
   * Event status
   */
  status?: EventStatus;
}

/**
 * DTO for completing an event
 */
export interface CompleteEventDto {
  /**
   * Completion date (ISO 8601 format, defaults to today)
   */
  completionDate?: string;

  /**
   * Notes about completion
   */
  notes?: string;

  /**
   * Attachment URL (optional)
   */
  attachmentUrl?: string;
}

/**
 * DTO for postponing an event
 */
export interface PostponeEventDto {
  /**
   * New due date (ISO 8601 format)
   */
  newDueDate: string;

  /**
   * Postponement reason (required)
   */
  reason: string;
}

/**
 * DTO for cancelling an event
 */
export interface CancelEventDto {
  /**
   * Cancellation reason (required)
   */
  reason: string;
}

/**
 * Query parameters for GET /events
 */
export interface EventsQueryParams {
  /**
   * Filter by household ID
   */
  householdId?: string;

  /**
   * Filter by task ID
   */
  taskId?: string;

  /**
   * Filter by assigned user ID
   */
  assignedToId?: string;

  /**
   * Filter by category ID
   */
  categoryId?: number;

  /**
   * Filter by priority
   */
  priority?: Priority;

  /**
   * Filter by status
   */
  status?: EventStatus;

  /**
   * Filter by date range - start date (ISO 8601 format)
   */
  startDate?: string;

  /**
   * Filter by date range - end date (ISO 8601 format)
   */
  endDate?: string;

  /**
   * Filter overdue events (due date in the past and status pending/postponed)
   */
  isOverdue?: boolean;

  /**
   * Sort by field
   */
  sortBy?: 'dueDate' | 'status' | 'priority' | 'createdAt';

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
 * Helper function to check if event is overdue
 */
export function isEventOverdue(event: Event): boolean {
  if (event.status === EventStatus.COMPLETED || event.status === EventStatus.CANCELLED) {
    return false;
  }
  const dueDate = new Date(event.dueDate);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return dueDate < today;
}

/**
 * Helper function to check if event is today
 */
export function isEventToday(event: Event): boolean {
  const dueDate = new Date(event.dueDate);
  const today = new Date();
  dueDate.setHours(0, 0, 0, 0);
  today.setHours(0, 0, 0, 0);
  return dueDate.getTime() === today.getTime();
}

/**
 * Helper function to get event urgency level
 * Returns: 'overdue' | 'today' | 'upcoming'
 */
export function getEventUrgency(event: Event): 'overdue' | 'today' | 'upcoming' {
  if (isEventOverdue(event)) {
    return 'overdue';
  }
  if (isEventToday(event)) {
    return 'today';
  }
  return 'upcoming';
}

/**
 * Helper function to get urgency severity for PrimeNG components
 */
export function getUrgencySeverity(urgency: 'overdue' | 'today' | 'upcoming'): 'danger' | 'warn' | 'info' {
  const severities: Record<'overdue' | 'today' | 'upcoming', 'danger' | 'warn' | 'info'> = {
    overdue: 'danger',
    today: 'warn',
    upcoming: 'info'
  };
  return severities[urgency];
}

/**
 * Helper function to get event status label
 */
export function getEventStatusLabel(status: EventStatus): string {
  const labels: Record<EventStatus, string> = {
    [EventStatus.PENDING]: 'Oczekujące',
    [EventStatus.COMPLETED]: 'Wykonane',
    [EventStatus.POSTPONED]: 'Przełożone',
    [EventStatus.CANCELLED]: 'Anulowane'
  };
  return labels[status];
}

/**
 * Helper function to get event status severity for PrimeNG components
 */
export function getEventStatusSeverity(status: EventStatus): 'success' | 'info' | 'warn' | 'danger' {
  const severities: Record<EventStatus, 'success' | 'info' | 'warn' | 'danger'> = {
    [EventStatus.PENDING]: 'info',
    [EventStatus.COMPLETED]: 'success',
    [EventStatus.POSTPONED]: 'warn',
    [EventStatus.CANCELLED]: 'danger'
  };
  return severities[status];
}

/**
 * Helper function to format date as human-readable string
 */
export function formatEventDate(dateString: string): string {
  const date = new Date(dateString);
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const eventDate = new Date(date);
  eventDate.setHours(0, 0, 0, 0);

  const diffTime = eventDate.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (diffDays === 0) {
    return 'Dzisiaj';
  } else if (diffDays === 1) {
    return 'Jutro';
  } else if (diffDays === -1) {
    return 'Wczoraj';
  } else if (diffDays > 1 && diffDays <= 7) {
    return `Za ${diffDays} dni`;
  } else if (diffDays < -1 && diffDays >= -7) {
    return `${Math.abs(diffDays)} dni temu`;
  }

  // Format as DD.MM.YYYY for dates further away
  return date.toLocaleDateString('pl-PL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
}
