using Homely.API.Models.DTOs.Tasks;
using Homely.API.Repositories.Base;
using Homely.API.Entities;
using Homely.API.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for event management (scheduled task occurrences)
/// </summary>
public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EventService> _logger;
    private readonly EventGenerationSettings _eventSettings;

    public EventService(
        IUnitOfWork unitOfWork,
        ILogger<EventService> logger,
        IOptions<EventGenerationSettings> eventSettings)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _eventSettings = eventSettings.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventDto>> GetHouseholdEventsAsync(Guid householdId, DateOnly? startDate = null, DateOnly? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<EventEntity> events;
            
            // If date range is specified, use custom query without status filter
            if (startDate.HasValue || endDate.HasValue)
            {
                // Build query with date filtering but without status restriction
                var query = _unitOfWork.Events.Query()
                    .Include(e => e.Task!)
                        .ThenInclude(t => t.Category!)
                        .ThenInclude(c => c!.CategoryType)
                    .Include(e => e.AssignedToUser)
                    .Where(e => e.HouseholdId == householdId && e.DeletedAt == null);

                if (startDate.HasValue)
                {
                    query = query.Where(e => e.DueDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.DueDate <= endDate.Value);
                }

                events = await query.ToListAsync(cancellationToken);
            }
            else
            {
                events = await _unitOfWork.Events.GetHouseholdEventsAsync(householdId, cancellationToken);
            }
            
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
            // Get the task with full details to inherit priority and category info
            var task = await _unitOfWork.Tasks.GetWithDetailsAsync(createDto.TaskId, cancellationToken);
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

            // Load full event details to return complete DTO
            var createdEvent = await _unitOfWork.Events.GetWithDetailsAsync(eventEntity.Id, cancellationToken);
            return MapToDto(createdEvent!);
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
    public async Task<EventDto> CompleteEventAsync(Guid eventId, CompleteEventDto completeDto, Guid completedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute in transaction using the execution strategy
            return await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
            {
                var eventEntity = await _unitOfWork.Events.GetWithDetailsAsync(eventId, ct);

                if (eventEntity == null || eventEntity.DeletedAt != null)
                {
                    throw new InvalidOperationException($"Event with ID {eventId} not found");
                }

                if (eventEntity.Status == "completed")
                {
                    throw new InvalidOperationException($"Event {eventId} is already completed");
                }

                // Parse completion date or use today
                var completionDate = !string.IsNullOrEmpty(completeDto.CompletionDate)
                    ? DateOnly.Parse(completeDto.CompletionDate)
                    : DateOnly.FromDateTime(DateTime.UtcNow);

                // Mark event as completed
                eventEntity.Status = "completed";
                eventEntity.CompletionDate = completionDate;
                eventEntity.CompletionNotes = completeDto.Notes;
                eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.Events.UpdateAsync(eventEntity, ct);

                _logger.LogInformation("Event {EventId} marked as completed", eventId);

                // NOTE: Event series generation is now handled at task creation time.
                // No need to create individual events on completion.
                // Future events are pre-generated and replenished via scheduled workflow.

                // Archive to events_history if household has premium plan
                await CreateEventHistoryIfPremiumAsync(eventEntity, completedBy, ct);

                return MapToDto(eventEntity);
            }, cancellationToken);
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

            // Parse new due date
            var newDueDate = DateOnly.Parse(postponeDto.NewDueDate);

            // Validate new due date is in the future
            if (newDueDate <= DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new InvalidOperationException("New due date must be in the future");
            }

            // Save original due date if not already postponed
            if (eventEntity.PostponedFromDate == null)
            {
                eventEntity.PostponedFromDate = eventEntity.DueDate;
            }

            // Update event with new due date and reason
            eventEntity.DueDate = newDueDate;
            eventEntity.PostponeReason = postponeDto.Reason;
            eventEntity.Status = "postponed";
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event {EventId} postponed to {NewDueDate}", eventId, newDueDate);

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error postponing event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EventDto> CancelEventAsync(Guid eventId, CancelEventDto cancelDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventEntity = await _unitOfWork.Events.GetWithDetailsAsync(eventId, cancellationToken);

            if (eventEntity == null || eventEntity.DeletedAt != null)
            {
                throw new InvalidOperationException($"Event with ID {eventId} not found");
            }

            if (eventEntity.Status == "completed")
            {
                throw new InvalidOperationException($"Cannot cancel completed event {eventId}");
            }

            // Update event status to cancelled and store reason in Notes
            eventEntity.Status = "cancelled";
            eventEntity.Notes = $"[CANCELLED] {cancelDto.Reason}" +
                               (string.IsNullOrEmpty(eventEntity.Notes) ? "" : $"\n\nPrevious notes:\n{eventEntity.Notes}");
            eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Event {EventId} cancelled with reason: {Reason}", eventId, cancelDto.Reason);

            return MapToDto(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling event {EventId}", eventId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GenerateEventSeriesAsync(Guid taskId, DateOnly startDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _unitOfWork.Tasks.GetWithDetailsAsync(taskId, cancellationToken);
            if (task == null || task.DeletedAt != null)
            {
                throw new InvalidOperationException($"Task template with ID {taskId} not found");
            }

            // Check if task has any interval values set
            bool hasInterval = (task.YearsValue.HasValue && task.YearsValue.Value > 0) ||
                             (task.MonthsValue.HasValue && task.MonthsValue.Value > 0) ||
                             (task.WeeksValue.HasValue && task.WeeksValue.Value > 0) ||
                             (task.DaysValue.HasValue && task.DaysValue.Value > 0);

            if (!hasInterval)
            {
                _logger.LogInformation("Task {TaskId} has no interval - skipping event series generation", taskId);
                return 0;
            }

            // Calculate end date based on configuration (today + FutureYears)
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(_eventSettings.FutureYears);

            var eventsGenerated = 0;
            var currentDueDate = startDate;

            // Generate events until we reach the end date
            while (true)
            {
                // Calculate next due date
                currentDueDate = CalculateNextDueDate(currentDueDate, task);

                // Stop if next event would be beyond our future window
                if (currentDueDate > endDate)
                {
                    break;
                }

                // Create event
                var eventEntity = new EventEntity
                {
                    TaskId = task.Id,
                    HouseholdId = task.HouseholdId,
                    AssignedTo = task.AssignedTo, // Copy from task template
                    DueDate = currentDueDate,
                    Status = "pending",
                    Priority = task.Priority,
                    Notes = null,
                    CreatedBy = task.CreatedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Events.AddAsync(eventEntity, cancellationToken);
                eventsGenerated++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Generated {Count} events for task {TaskId} from {StartDate} to {EndDate}",
                eventsGenerated, taskId, startDate, endDate);

            return eventsGenerated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating event series for task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> RegenerateEventsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async (ct) =>
            {
                var task = await _unitOfWork.Tasks.GetWithDetailsAsync(taskId, ct);
                if (task == null || task.DeletedAt != null)
                {
                    throw new InvalidOperationException($"Task template with ID {taskId} not found");
                }

                // Delete all future pending events for this task
                var futureEvents = await _unitOfWork.Events.Query()
                    .Where(e => e.TaskId == taskId &&
                               e.DeletedAt == null &&
                               e.Status == "pending" &&
                               e.DueDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                    .ToListAsync(ct);

                foreach (var evt in futureEvents)
                {
                    evt.DeletedAt = DateTimeOffset.UtcNow;
                    evt.UpdatedAt = DateTimeOffset.UtcNow;
                    await _unitOfWork.Events.UpdateAsync(evt, ct);
                }

                _logger.LogInformation(
                    "Deleted {Count} future events for task {TaskId}",
                    futureEvents.Count, taskId);

                // Generate new series of events
                var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var eventsGenerated = await GenerateEventSeriesAsync(
                    taskId,
                    startDate,
                    ct);

                return eventsGenerated;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating events for task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountFutureEventsForTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _unitOfWork.Events.Query()
                .CountAsync(e => e.TaskId == taskId &&
                               e.DeletedAt == null &&
                               e.Status == "pending" &&
                               e.DueDate >= today,
                           cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting future events for task {TaskId}", taskId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> RefillEventsForHouseholdAsync(Guid householdId, CancellationToken cancellationToken = default)
    {
        try
        {
            var activeTasks = await _unitOfWork.Tasks.GetActiveTasksAsync(householdId, cancellationToken);
            var totalEventsGenerated = 0;

            foreach (var task in activeTasks)
            {
                // Skip tasks without intervals (one-time tasks)
                bool hasInterval = (task.YearsValue.HasValue && task.YearsValue.Value > 0) ||
                                 (task.MonthsValue.HasValue && task.MonthsValue.Value > 0) ||
                                 (task.WeeksValue.HasValue && task.WeeksValue.Value > 0) ||
                                 (task.DaysValue.HasValue && task.DaysValue.Value > 0);

                if (!hasInterval)
                {
                    continue;
                }

                // Find the last future event date
                var lastFutureEvent = await _unitOfWork.Events.Query()
                    .Where(e => e.TaskId == task.Id &&
                               e.DeletedAt == null &&
                               e.Status == "pending")
                    .OrderByDescending(e => e.DueDate)
                    .FirstOrDefaultAsync(cancellationToken);

                // Calculate the threshold date (today + MinFutureMonthsThreshold)
                var thresholdDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(_eventSettings.MinFutureMonthsThreshold);

                // If the last event is before the threshold (or no events exist), generate more
                if (lastFutureEvent == null || lastFutureEvent.DueDate < thresholdDate)
                {
                    var startDate = lastFutureEvent?.DueDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

                    // Generate events up to FutureYears
                    var generated = await GenerateEventSeriesAsync(
                        task.Id,
                        startDate,
                        cancellationToken);

                    totalEventsGenerated += generated;

                    var lastEventDate = lastFutureEvent?.DueDate.ToString("yyyy-MM-dd") ?? "none";
                    _logger.LogInformation(
                        "Refilled {Count} events for task {TaskId} (last event was: {LastEventDate}, threshold: {ThresholdDate})",
                        generated, task.Id, lastEventDate, thresholdDate);
                }
            }

            _logger.LogInformation(
                "Refill complete for household {HouseholdId}: {TotalGenerated} total events generated",
                householdId, totalEventsGenerated);

            return totalEventsGenerated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refilling events for household {HouseholdId}", householdId);
            throw;
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
    /// Creates event history record if household has premium plan.
    /// Premium plans are identified by plan name containing "Premium" or "Rodzinny".
    /// This method is called within a transaction context and does not commit changes.
    /// </summary>
    private async Task CreateEventHistoryIfPremiumAsync(EventEntity eventEntity, Guid completedBy, CancellationToken cancellationToken)
    {
        try
        {
            // Get household with plan details
            var household = await _unitOfWork.Households.GetByIdAsync(
                eventEntity.HouseholdId,
                h => h.PlanType);

            if (household == null || household.PlanType == null)
            {
                _logger.LogWarning("Cannot create event history: Household {HouseholdId} or plan type not found", eventEntity.HouseholdId);
                return;
            }

            // Check if household has premium plan (Premium or Rodzinny)
            var isPremium = household.PlanType.Name.Contains("Premium", StringComparison.OrdinalIgnoreCase) ||
                           household.PlanType.Name.Contains("Rodzinny", StringComparison.OrdinalIgnoreCase);

            if (!isPremium)
            {
                _logger.LogDebug("Skipping event history creation: Household {HouseholdId} does not have premium plan", eventEntity.HouseholdId);
                return;
            }

            // Get task name if event is linked to a task
            string taskName = "One-off event";
            if (eventEntity.TaskId.HasValue)
            {
                var task = await _unitOfWork.Tasks.GetByIdAsync(eventEntity.TaskId.Value, cancellationToken);
                if (task != null)
                {
                    taskName = task.Name;
                }
            }

            // Create event history record
            var historyEntry = new EventHistoryEntity
            {
                EventId = eventEntity.Id,
                TaskId = eventEntity.TaskId,
                HouseholdId = eventEntity.HouseholdId,
                AssignedTo = eventEntity.AssignedTo,
                CompletedBy = completedBy,
                DueDate = eventEntity.DueDate,
                CompletionDate = eventEntity.CompletionDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                TaskName = taskName,
                CompletionNotes = eventEntity.CompletionNotes,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.EventsHistory.AddAsync(historyEntry, cancellationToken);
            // Note: SaveChanges is NOT called here - transaction will be committed by the caller

            _logger.LogInformation(
                "Created event history entry {HistoryId} for event {EventId} in premium household {HouseholdId}",
                historyEntry.Id,
                eventEntity.Id,
                eventEntity.HouseholdId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event history for event {EventId}", eventEntity.Id);
            throw; // Re-throw to rollback the entire transaction
        }
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
            AssignedToFirstName = entity.AssignedToUser?.FirstName,
            AssignedToLastName = entity.AssignedToUser?.LastName,
            CategoryId = entity.Task?.Category?.Id,
            CategoryName = entity.Task?.Category?.Name,
            CategoryTypeName = entity.Task?.Category?.CategoryType?.Name,
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
