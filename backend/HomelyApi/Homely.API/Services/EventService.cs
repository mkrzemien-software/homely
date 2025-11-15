using Homely.API.Models.DTOs.Tasks;
using Homely.API.Repositories.Base;
using Homely.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for event management (scheduled task occurrences)
/// </summary>
public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IUnitOfWork unitOfWork,
        ILogger<EventService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetHouseholdEventsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _unitOfWork.Events.GetHouseholdEventsAsync(householdId, cancellationToken);
            return events
                .Where(e => e.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetAssignedEventsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _unitOfWork.Events.GetAssignedEventsAsync(userId, cancellationToken);
            return events
                .Where(e => e.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetEventsByStatusAsync(Guid householdId, string status, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _unitOfWork.Events.GetByStatusAsync(householdId, status, cancellationToken);
            return events
                .Where(e => e.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events with status {Status} for household {HouseholdId}", status, householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetUpcomingEventsAsync(Guid householdId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var toDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
            var events = await _unitOfWork.Events.GetUpcomingEventsAsync(householdId, days, cancellationToken);

            return events
                .Where(e => e.DeletedAt == null && e.Status == "pending")
                .OrderBy(e => e.DueDate)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming events for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetOverdueEventsAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _unitOfWork.Events.GetOverdueEventsAsync(householdId, cancellationToken);
            return events
                .Where(e => e.DeletedAt == null)
                .Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue events for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetWithDetailsAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                return null;
            }

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto> CreateEventAsync(CreateEventDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the task to inherit priority if not specified
            var task = await _unitOfWork.Tasks.GetByIdAsync(createDto.TaskId, cancellationToken);
            if (task == null || task.DeletedAt != null)
            {
                throw new InvalidOperationException($"Task template with ID {createDto.TaskId} not found");
            }

            var eventEntity = new EventEntity
            {
                TaskId = createDto.TaskId,
                HouseholdId = createDto.HouseholdId,
                AssignedTo = createDto.AssignedTo,
                DueDate = createDto.DueDate,
                Status = "pending",
                Priority = createDto.Priority ?? task.Priority,
                Notes = createDto.Notes,
                CreatedBy = createDto.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Events.AddAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event created with ID {EventId} for task {TaskId} in household {HouseholdId}",
                eventEntity.Id, task.Id, eventEntity.HouseholdId);

            // Load task navigation property for DTO mapping
            eventEntity.Task = task;

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto> UpdateEventAsync(Guid eventId, UpdateEventDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetWithDetailsAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            eventEntity.AssignedTo = updateDto.AssignedTo;
            eventEntity.DueDate = updateDto.DueDate;
            eventEntity.Status = updateDto.Status;
            eventEntity.Priority = updateDto.Priority;
            eventEntity.Notes = updateDto.Notes;
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event {EventId} updated successfully", eventId);

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                return false;
            }

            // Soft delete
            eventEntity.DeletedAt = DateTimeOffset.UtcNow;
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event {EventId} soft deleted successfully", eventId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto> CompleteEventAsync(Guid eventId, CompleteEventDto completeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Start transaction for atomic completion and recurring event creation
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var eventEntity = await _unitOfWork.Events.GetWithDetailsAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            if (eventEntity.Status == "completed")
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new InvalidOperationException($"Event {eventId} is already completed");
            }

            // Mark event as completed
            eventEntity.Status = "completed";
            eventEntity.CompletionDate = completeDto.CompletionDate;
            eventEntity.CompletionNotes = completeDto.CompletionNotes;
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);

            _logger.LogInformation("Event {EventId} marked as completed", eventId);

            // If task template has an interval, create next recurring event
            if (eventEntity.TaskId.HasValue)
            {
                var task = await _unitOfWork.Tasks.GetByIdAsync(eventEntity.TaskId.Value, cancellationToken);
                if (task != null && task.DeletedAt == null)
                {
                    // Check if task has any interval values set
                    bool hasInterval = (task.YearsValue.HasValue && task.YearsValue.Value > 0) ||
                                     (task.MonthsValue.HasValue && task.MonthsValue.Value > 0) ||
                                     (task.WeeksValue.HasValue && task.WeeksValue.Value > 0) ||
                                     (task.DaysValue.HasValue && task.DaysValue.Value > 0);

                    if (hasInterval)
                    {
                        await CreateNextRecurringEventAsync(eventEntity, completeDto.CompletionDate, cancellationToken);
                    }
                }
            }

            // TODO: Archive to task_history if household has premium plan
            // This would require checking household's plan type and adding to tasks_history table

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto> PostponeEventAsync(Guid eventId, PostponeEventDto postponeDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            if (eventEntity.Status == "completed")
            {
                throw new InvalidOperationException($"Cannot postpone completed event {eventId}");
            }

            // Validate new due date is in the future
            if (postponeDto.NewDueDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("New due date must be in the future");
            }

            // Save original due date if not already postponed
            if (eventEntity.PostponedFromDate == null)
            {
                eventEntity.PostponedFromDate = eventEntity.DueDate;
            }

            // Update event with new due date and reason
            eventEntity.DueDate = postponeDto.NewDueDate;
            eventEntity.PostponeReason = postponeDto.PostponeReason;
            eventEntity.Status = "postponed";
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event {EventId} postponed to {NewDueDate}", eventId, postponeDto.NewDueDate);

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing event {EventId}", eventId);
            throw;
        }
    }

    /// <summary>
    /// Creates next recurring event based on task template's interval settings.
    /// This method is called within a transaction context and does not commit changes.
    /// </summary>
    private async Task CreateNextRecurringEventAsync(EventEntity completedEvent, DateOnly completionDate, CancellationToken cancellationToken)
    {
        try
        {
            if (!completedEvent.TaskId.HasValue)
            {
                return;
            }

            // Get task template with interval settings
            var taskTemplate = await _unitOfWork.Tasks.GetByIdAsync(completedEvent.TaskId.Value, cancellationToken);

            if (taskTemplate == null || taskTemplate.DeletedAt != null)
            {
                _logger.LogWarning("Cannot create recurring event: Task template {TaskId} not found", completedEvent.TaskId.Value);
                return;
            }

            // Calculate next due date based on task template's interval
            var nextDueDate = CalculateNextDueDate(completionDate, taskTemplate);

            // Create new event
            var newEvent = new EventEntity
            {
                TaskId = completedEvent.TaskId,
                HouseholdId = completedEvent.HouseholdId,
                AssignedTo = completedEvent.AssignedTo,
                DueDate = nextDueDate,
                Status = "pending",
                Priority = taskTemplate.Priority, // Use task template's priority
                Notes = null, // New event starts with no notes
                CreatedBy = completedEvent.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Events.AddAsync(newEvent, cancellationToken);
            // Note: SaveChanges is NOT called here - transaction will be committed by the caller (CompleteEventAsync)

            _logger.LogInformation(
                "Created recurring event {NewEventId} for task template {TaskId} with due date {DueDate}",
                newEvent.Id, taskTemplate.Id, nextDueDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring event for completed event {EventId}", completedEvent.Id);
            throw; // Re-throw to rollback the entire transaction
        }
    }

    /// <summary>
    /// Calculates next due date based on task template's interval settings
    /// </summary>
    private static DateOnly CalculateNextDueDate(DateOnly completionDate, TaskEntity taskTemplate)
    {
        var nextDate = completionDate;

        // Add years
        if (taskTemplate.YearsValue.HasValue && taskTemplate.YearsValue.Value > 0)
        {
            nextDate = nextDate.AddYears(taskTemplate.YearsValue.Value);
        }

        // Add months
        if (taskTemplate.MonthsValue.HasValue && taskTemplate.MonthsValue.Value > 0)
        {
            nextDate = nextDate.AddMonths(taskTemplate.MonthsValue.Value);
        }

        // Add weeks (convert to days)
        if (taskTemplate.WeeksValue.HasValue && taskTemplate.WeeksValue.Value > 0)
        {
            nextDate = nextDate.AddDays(taskTemplate.WeeksValue.Value * 7);
        }

        // Add days
        if (taskTemplate.DaysValue.HasValue && taskTemplate.DaysValue.Value > 0)
        {
            nextDate = nextDate.AddDays(taskTemplate.DaysValue.Value);
        }

        return nextDate;
    }

    /// <summary>
    /// Maps EventEntity to EventDto
    /// </summary>
    private static EventDto MapToDto(EventEntity entity)
    {
        return new EventDto
        {
            Id = entity.Id,
            TaskId = entity.TaskId ?? Guid.Empty,
            TaskName = entity.Task?.Name ?? string.Empty,
            HouseholdId = entity.HouseholdId,
            HouseholdName = entity.Household?.Name,
            AssignedTo = entity.AssignedTo,
            DueDate = entity.DueDate,
            Status = entity.Status,
            Priority = entity.Priority,
            CompletionDate = entity.CompletionDate,
            CompletionNotes = entity.CompletionNotes,
            PostponedFromDate = entity.PostponedFromDate,
            PostponeReason = entity.PostponeReason,
            Notes = entity.Notes,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
