/**
 * Dashboard Models
 *
 * Models for dashboard data based on API plan
 * Endpoint: GET /dashboard/upcoming-events
 * See: .ai/api-plan.md section "Dashboard"
 */

import { Priority, EventStatus } from '../../events/models/event.model';

/**
 * Dashboard Event
 * Simplified event model for dashboard display
 */
export interface DashboardEvent {
  /**
   * Event ID
   */
  id: string;

  /**
   * Due date (ISO 8601 format)
   */
  dueDate: string;

  /**
   * Urgency status (calculated by backend)
   */
  urgencyStatus: 'overdue' | 'today' | 'upcoming';

  /**
   * Task information
   */
  task: {
    /**
     * Task name (displayed as event title)
     */
    name: string;

    /**
     * Category information
     */
    category: {
      /**
       * Category name (subcategory)
       */
      name: string;

      /**
       * Category type
       */
      categoryType: {
        /**
         * Category type name
         */
        name: string;
      };
    };
  };

  /**
   * Assigned user information
   */
  assignedTo: {
    /**
     * User first name
     */
    firstName: string;

    /**
     * User last name
     */
    lastName: string;
  };

  /**
   * Event priority
   */
  priority: Priority;

  /**
   * Event status
   */
  status: EventStatus;
}

/**
 * Dashboard Events Summary
 * Statistics about upcoming events
 */
export interface DashboardEventsSummary {
  /**
   * Number of overdue events
   */
  overdue: number;

  /**
   * Number of events due today
   */
  today: number;

  /**
   * Number of events due this week
   */
  thisWeek: number;
}

/**
 * Dashboard Upcoming Events Response
 * Response from GET /dashboard/upcoming-events
 */
export interface DashboardUpcomingEventsResponse {
  /**
   * Array of upcoming events
   */
  data: DashboardEvent[];

  /**
   * Summary statistics
   */
  summary: DashboardEventsSummary;
}

/**
 * Query parameters for GET /dashboard/upcoming-events
 */
export interface DashboardUpcomingEventsParams {
  /**
   * Number of days to fetch (7, 14, or 30)
   * Default: 7
   */
  days?: 7 | 14 | 30;

  /**
   * Household ID to filter by
   */
  householdId?: string;

  /**
   * Start date for range (ISO 8601 format)
   * Defaults to today
   */
  startDate?: string;

  /**
   * Include completed events in the response
   * Default: false
   */
  includeCompleted?: boolean;
}

/**
 * Dashboard Statistics Response
 * Response from GET /dashboard/statistics
 */
export interface DashboardStatisticsResponse {
  /**
   * Tasks statistics
   */
  tasks: {
    /**
     * Total number of tasks
     */
    total: number;

    /**
     * Tasks grouped by category
     */
    byCategory: Array<{
      /**
       * Category name
       */
      categoryName: string;

      /**
       * Number of tasks in this category
       */
      count: number;
    }>;
  };

  /**
   * Events statistics
   */
  events: {
    /**
     * Number of pending events
     */
    pending: number;

    /**
     * Number of overdue events
     */
    overdue: number;

    /**
     * Number of events completed this month
     */
    completedThisMonth: number;
  };

  /**
   * Plan usage statistics
   */
  planUsage: {
    /**
     * Number of tasks used
     */
    tasksUsed: number;

    /**
     * Maximum tasks allowed by plan
     */
    tasksLimit: number;

    /**
     * Number of household members
     */
    membersUsed: number;

    /**
     * Maximum members allowed by plan
     */
    membersLimit: number;
  };
}

/**
 * Helper function to get urgency severity for PrimeNG components
 */
export function getDashboardUrgencySeverity(
  urgency: 'overdue' | 'today' | 'upcoming'
): 'danger' | 'warn' | 'info' {
  const severities: Record<'overdue' | 'today' | 'upcoming', 'danger' | 'warn' | 'info'> = {
    overdue: 'danger',
    today: 'warn',
    upcoming: 'info'
  };
  return severities[urgency];
}

/**
 * Helper function to get urgency label in Polish
 */
export function getDashboardUrgencyLabel(urgency: 'overdue' | 'today' | 'upcoming'): string {
  const labels: Record<'overdue' | 'today' | 'upcoming', string> = {
    overdue: 'Przekroczony termin',
    today: 'Dzisiaj',
    upcoming: 'Nadchodzące'
  };
  return labels[urgency];
}

/**
 * Helper function to format assigned user name
 */
export function formatAssignedUser(assignedTo: DashboardEvent['assignedTo']): string {
  return `${assignedTo.firstName} ${assignedTo.lastName}`;
}

/**
 * Helper function to format category path
 * Returns: "CategoryType > Category"
 */
export function formatCategoryPath(category: DashboardEvent['task']['category']): string {
  return `${category.categoryType.name} > ${category.name}`;
}
