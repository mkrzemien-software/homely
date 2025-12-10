using Homely.API.Models.DTOs;
using Homely.API.Models.DTOs.Tasks;

namespace Homely.API.Services;

/// <summary>
/// Service interface for event management (scheduled task occurrences)
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get events with filtering, sorting, and pagination
    /// </summary>
    /// <param name="householdId">Household ID (required)</param>
    /// <param name="taskId">Optional filter by task ID</param>
    /// <param name="assignedTo">Optional filter by assigned user ID</param>
    /// <param name="categoryId">Optional filter by task category ID</param>
    /// <param name="status">Optional filter by status (pending, completed, postponed, cancelled)</param>
    /// <param name="priority">Optional filter by priority (low, medium, high)</param>
    /// <param name="startDate">Optional filter by start date (YYYY-MM-DD)</param>
    /// <param name="endDate">Optional filter by end date (YYYY-MM-DD)</param>
    /// <param name="isOverdue">Optional filter for overdue events only</param>
    /// <param name="sortBy">Sort field: dueDate, status, priority, createdAt (default: dueDate)</param>
    /// <param name="sortOrder">Sort order: asc or desc (default: asc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of events</returns>
    Task<PaginatedResponseDto<EventDto>> GetFilteredEventsAsync(
        Guid householdId,
        Guid? taskId = null,
        Guid? assignedTo = null,
        int? categoryId = null,
        string? status = null,
        string? priority = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool? isOverdue = null,
        string sortBy = "dueDate",
        string sortOrder = "asc",
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all events for a household with optional date filtering
    /// </summary>
    Task<IEnumerable<EventDto>> GetHouseholdEventsAsync(Guid householdId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events assigned to a specific user
    /// </summary>
    Task<IEnumerable<EventDto>> GetAssignedEventsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events by status for a household
    /// </summary>
    Task<IEnumerable<EventDto>> GetEventsByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming events for a household (within specified days)
    /// </summary>
    Task<IEnumerable<EventDto>> GetUpcomingEventsAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue events for a household
    /// </summary>
    Task<IEnumerable<EventDto>> GetOverdueEventsAsync(Guid householdId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get event by ID
    /// </summary>
    Task<EventDto?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new event
    /// </summary>
    Task<EventDto> CreateEventAsync(CreateEventDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing event
    /// </summary>
    Task<EventDto> UpdateEventAsync(Guid eventId, UpdateEventDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete event (soft delete)
    /// </summary>
    Task<bool> DeleteEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as completed. If event is recurring and has an associated task template,
    /// automatically creates the next recurring event based on task's interval.
    /// For premium households, archives the completion to events_history.
    /// </summary>
    /// <param name="eventId">The event ID to complete</param>
    /// <param name="completeDto">Completion details</param>
    /// <param name="completedBy">User ID who completed the event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed event</returns>
    Task<EventDto> CompleteEventAsync(Guid eventId, CompleteEventDto completeDto, Guid completedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Postpone event to a new due date with a reason
    /// </summary>
    Task<EventDto> PostponeEventAsync(Guid eventId, PostponeEventDto postponeDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel event with a reason (changes status to 'cancelled')
    /// </summary>
    Task<EventDto> CancelEventAsync(Guid eventId, CancelEventDto cancelDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a series of future events for a task template.
    /// Events are generated up to FutureYears (from configuration) into the future.
    /// Used when creating a new task or regenerating events.
    /// </summary>
    /// <param name="taskId">Task template ID</param>
    /// <param name="startDate">Starting date for event generation (typically today or last completion date)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of events generated</returns>
    Task<int> GenerateEventSeriesAsync(Guid taskId, DateOnly startDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerate events for a task template.
    /// Deletes all future (non-completed) events and generates a new series.
    /// Used when task interval is changed.
    /// </summary>
    /// <param name="taskId">Task template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of events generated</returns>
    Task<int> RegenerateEventsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count future pending events for a task template.
    /// </summary>
    /// <param name="taskId">Task template ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of future pending events</returns>
    Task<int> CountFutureEventsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refill events for all active tasks in a household.
    /// Checks each task and adds more events if below threshold.
    /// Used by the GitHub Actions workflow.
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total number of events generated across all tasks</returns>
    Task<int> RefillEventsForHouseholdAsync(Guid householdId, CancellationToken cancellationToken = default);
}
