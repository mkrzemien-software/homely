using Homely.API.Models.DTOs.Tasks;

namespace Homely.API.Services;

/// <summary>
/// Service interface for event management (scheduled task occurrences)
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get all events for a household
    /// </summary>
    Task<IEnumerable<EventDto>> GetHouseholdEventsAsync(Guid householdId, CancellationToken cancellationToken = default);

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
    /// </summary>
    Task<EventDto> CompleteEventAsync(Guid eventId, CompleteEventDto completeDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Postpone event to a new due date with a reason
    /// </summary>
    Task<EventDto> PostponeEventAsync(Guid eventId, PostponeEventDto postponeDto, CancellationToken cancellationToken = default);
}
