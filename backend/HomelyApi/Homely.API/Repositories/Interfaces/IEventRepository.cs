using Homely.API.Entities;
using Homely.API.Models.ViewModels;
using Homely.API.Repositories.Base;

namespace Homely.API.Repositories.Interfaces;

/// <summary>
/// Repository for managing Events (concrete scheduled occurrences of tasks).
/// Events are stored in the 'tasks' table but represent specific scheduled appointments/occurrences.
/// </summary>
public interface IEventRepository : IBaseRepository<EventEntity, Guid>
{
    /// <summary>
    /// Get all events for a specific household
    /// </summary>
    Task<IEnumerable<EventEntity>> GetHouseholdEventsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events assigned to a specific user across all their households
    /// </summary>
    Task<IEnumerable<EventEntity>> GetAssignedEventsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events by status for a household
    /// </summary>
    /// <param name="status">pending, completed, postponed, cancelled</param>
    Task<IEnumerable<EventEntity>> GetByStatusAsync(
        Guid householdId,
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming events for dashboard display
    /// </summary>
    /// <param name="days">Number of days to look ahead (default 7)</param>
    Task<IEnumerable<EventEntity>> GetUpcomingEventsAsync(
        Guid householdId,
        int days = 7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events due within a date range
    /// </summary>
    Task<IEnumerable<EventEntity>> GetDueEventsAsync(
        Guid householdId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue events (past due date and still pending)
    /// </summary>
    Task<IEnumerable<EventEntity>> GetOverdueEventsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all events for a specific task template
    /// </summary>
    Task<IEnumerable<EventEntity>> GetTaskEventsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get event with full details including task template, category, household
    /// </summary>
    Task<EventEntity?> GetWithDetailsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has access to the event (through household membership)
    /// </summary>
    Task<bool> CanUserAccessEventAsync(
        Guid eventId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark event as completed and automatically generate next event if task has interval
    /// </summary>
    /// <returns>Tuple of (completedEvent, nextEvent). nextEvent is null if task has no interval.</returns>
    Task<(EventEntity completedEvent, EventEntity? nextEvent)> CompleteEventAsync(
        Guid eventId,
        DateOnly completionDate,
        string? completionNotes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Postpone event to a new due date
    /// </summary>
    Task<EventEntity> PostponeEventAsync(
        Guid eventId,
        DateOnly newDueDate,
        string postponeReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel event (does not generate new event)
    /// </summary>
    Task<EventEntity> CancelEventAsync(
        Guid eventId,
        string cancelReason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get events with filtering, sorting, and pagination
    /// </summary>
    Task<PagedResult<EventEntity>> GetFilteredEventsAsync(
        Guid householdId,
        Guid? taskId = null,
        Guid? assignedTo = null,
        Guid? categoryId = null,
        string? status = null,
        string? priority = null,
        DateOnly? dueDateFrom = null,
        DateOnly? dueDateTo = null,
        bool? isOverdue = null,
        string sortBy = "dueDate",
        bool ascending = true,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count events by status for a household
    /// </summary>
    Task<Dictionary<string, int>> GetEventCountsByStatusAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get statistics for dashboard
    /// </summary>
    Task<EventStatisticsViewModel> GetStatisticsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default);
}
