using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Homely.API.Data;
using Homely.API.Entities;
using Homely.API.Models.Constants;
using Homely.API.Models.ViewModels;
using Homely.API.Repositories.Base;
using Homely.API.Repositories.Interfaces;

namespace Homely.API.Repositories.Implementations;

/// <summary>
/// Repository for managing Events (concrete scheduled occurrences).
/// Events are stored in the 'tasks' table but semantically represent scheduled appointments/occurrences.
/// </summary>
public class EventRepository : BaseRepository<EventEntity, Guid>, IEventRepository
{
    public EventRepository(HomelyDbContext context) : base(context)
    {
    }

    protected override Expression<Func<EventEntity, bool>> GetIdPredicate(Guid id)
    {
        return e => e.Id == id;
    }

    // ============================================================================
    // QUERY METHODS
    // ============================================================================

    public async Task<IEnumerable<EventEntity>> GetHouseholdEventsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            e => e.HouseholdId == householdId,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.AssignedToUser!);
    }

    public async Task<IEnumerable<EventEntity>> GetAssignedEventsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            e => e.AssignedTo == userId,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.Household,
            e => e.AssignedToUser!);
    }

    public async Task<IEnumerable<EventEntity>> GetByStatusAsync(
        Guid householdId,
        string status,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(
            e => e.HouseholdId == householdId && e.Status == status,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.AssignedToUser!);
    }

    public async Task<IEnumerable<EventEntity>> GetUpcomingEventsAsync(
        Guid householdId,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

        return await Context.Set<EventEntity>()
            .Where(e => e.HouseholdId == householdId &&
                       e.Status == DatabaseConstants.TaskStatuses.Pending &&
                       e.DueDate <= endDate &&
                       e.DeletedAt == null)
            .Include(e => e.Task)
                .ThenInclude(t => t!.Category)
                .ThenInclude(c => c!.CategoryType)
            .Include(e => e.Household)
            .Include(e => e.AssignedToUser)
            .OrderBy(e => e.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventEntity>> GetDueEventsAsync(
        Guid householdId,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        fromDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
        toDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        return await GetWhereAsync(
            e => e.HouseholdId == householdId &&
                 e.DueDate >= fromDate &&
                 e.DueDate <= toDate &&
                 e.Status == DatabaseConstants.TaskStatuses.Pending,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.AssignedToUser!);
    }

    public async Task<IEnumerable<EventEntity>> GetOverdueEventsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await GetWhereAsync(
            e => e.HouseholdId == householdId &&
                 e.DueDate < today &&
                 e.Status == DatabaseConstants.TaskStatuses.Pending,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.AssignedToUser!);
    }

    public async Task<IEnumerable<EventEntity>> GetTaskEventsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await GetWhereAsync(e => e.TaskId == taskId);
    }

    public async Task<EventEntity?> GetWithDetailsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(
            eventId,
            e => e.Task!,
            e => e.Task!.Category!,
            e => e.Task!.Category!.CategoryType,
            e => e.Household,
            e => e.AssignedToUser!,
            e => e.CreatedByUser);
    }

    public async Task<bool> CanUserAccessEventAsync(
        Guid eventId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(e => e.Id == eventId &&
                          e.Household.HouseholdMembers.Any(hm => hm.UserId == userId && hm.DeletedAt == null),
                cancellationToken);
    }

    // ============================================================================
    // COMPLEX OPERATIONS
    // ============================================================================

    public async Task<(EventEntity completedEvent, EventEntity? nextEvent)> CompleteEventAsync(
        Guid eventId,
        DateOnly completionDate,
        string? completionNotes = null,
        CancellationToken cancellationToken = default)
    {
        // Get event with task template details
        var eventEntity = await GetWithDetailsAsync(eventId, cancellationToken);
        if (eventEntity == null)
            throw new InvalidOperationException($"Event with ID {eventId} not found");

        // Mark event as completed
        eventEntity.Status = DatabaseConstants.TaskStatuses.Completed;
        eventEntity.CompletionDate = completionDate;
        eventEntity.CompletionNotes = completionNotes;
        eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await UpdateAsync(eventEntity, cancellationToken);

        // Check if we need to generate next event
        EventEntity? nextEvent = null;

        if (eventEntity.Task != null && HasInterval(eventEntity.Task))
        {
            // Calculate next due date based on completion date + interval
            var nextDueDate = CalculateNextDueDate(completionDate, eventEntity.Task);

            // Create next event
            nextEvent = new EventEntity
            {
                TaskId = eventEntity.TaskId,
                HouseholdId = eventEntity.HouseholdId,
                AssignedTo = eventEntity.AssignedTo, // Keep same assignee (can be changed later)
                DueDate = nextDueDate,
                Status = DatabaseConstants.TaskStatuses.Pending,
                Priority = eventEntity.Task.Priority, // Inherit priority from task template
                Notes = null, // New event starts with no notes
                CreatedBy = eventEntity.CreatedBy,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await AddAsync(nextEvent, cancellationToken);
        }

        // Save changes
        await SaveChangesAsync(cancellationToken);

        return (eventEntity, nextEvent);
    }

    public async Task<EventEntity> PostponeEventAsync(
        Guid eventId,
        DateOnly newDueDate,
        string postponeReason,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null)
            throw new InvalidOperationException($"Event with ID {eventId} not found");

        // Store original due date if not already postponed
        if (eventEntity.PostponedFromDate == null)
        {
            eventEntity.PostponedFromDate = eventEntity.DueDate;
        }

        eventEntity.DueDate = newDueDate;
        eventEntity.Status = DatabaseConstants.TaskStatuses.Postponed;
        eventEntity.PostponeReason = postponeReason;
        eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await UpdateAsync(eventEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return eventEntity;
    }

    public async Task<EventEntity> CancelEventAsync(
        Guid eventId,
        string cancelReason,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null)
            throw new InvalidOperationException($"Event with ID {eventId} not found");

        eventEntity.Status = DatabaseConstants.TaskStatuses.Cancelled;
        eventEntity.CompletionNotes = cancelReason; // Store cancel reason in completion notes
        eventEntity.UpdatedAt = DateTimeOffset.UtcNow;

        await UpdateAsync(eventEntity, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return eventEntity;
    }

    // ============================================================================
    // FILTERING AND PAGINATION
    // ============================================================================

    public async Task<PagedResult<EventEntity>> GetFilteredEventsAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = Query()
            .Include(e => e.Task!)
            .ThenInclude(t => t.Category!)
            .ThenInclude(c => c!.CategoryType)
            .Include(e => e.AssignedToUser)
            .Where(e => e.HouseholdId == householdId);

        // Apply filters
        if (taskId.HasValue)
            query = query.Where(e => e.TaskId == taskId.Value);

        if (assignedTo.HasValue)
            query = query.Where(e => e.AssignedTo == assignedTo.Value);

        if (categoryId.HasValue)
            query = query.Where(e => e.Task != null && e.Task.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(e => e.Priority == priority);

        if (dueDateFrom.HasValue)
            query = query.Where(e => e.DueDate >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(e => e.DueDate <= dueDateTo.Value);

        if (isOverdue.HasValue && isOverdue.Value)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(e => e.DueDate < today &&
                                   (e.Status == DatabaseConstants.TaskStatuses.Pending ||
                                    e.Status == DatabaseConstants.TaskStatuses.Postponed));
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "duedate" => ascending
                ? query.OrderBy(e => e.DueDate)
                : query.OrderByDescending(e => e.DueDate),
            "priority" => ascending
                ? query.OrderBy(e => e.Priority)
                : query.OrderByDescending(e => e.Priority),
            "createdat" => ascending
                ? query.OrderBy(e => e.CreatedAt)
                : query.OrderByDescending(e => e.CreatedAt),
            "status" => ascending
                ? query.OrderBy(e => e.Status)
                : query.OrderByDescending(e => e.Status),
            _ => ascending
                ? query.OrderBy(e => e.DueDate)
                : query.OrderByDescending(e => e.DueDate)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EventEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    // ============================================================================
    // STATISTICS
    // ============================================================================

    public async Task<Dictionary<string, int>> GetEventCountsByStatusAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(e => e.HouseholdId == householdId)
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<EventStatisticsViewModel> GetStatisticsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var monthStart = DateOnly.FromDateTime(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));

        var events = await Query()
            .Where(e => e.HouseholdId == householdId)
            .ToListAsync(cancellationToken);

        var countsByStatus = events
            .GroupBy(e => e.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var countsByPriority = events
            .Where(e => e.Status == DatabaseConstants.TaskStatuses.Pending)
            .GroupBy(e => e.Priority)
            .ToDictionary(g => g.Key, g => g.Count());

        return new EventStatisticsViewModel
        {
            TotalCount = events.Count,
            PendingCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Pending),
            OverdueCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Pending && e.DueDate < today),
            TodayCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Pending && e.DueDate == today),
            ThisWeekCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Pending && e.DueDate <= weekEnd && e.DueDate >= today),
            CompletedThisMonthCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Completed && e.CompletionDate >= monthStart),
            PostponedCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Postponed),
            CancelledCount = events.Count(e => e.Status == DatabaseConstants.TaskStatuses.Cancelled),
            CountsByStatus = countsByStatus,
            CountsByPriority = countsByPriority
        };
    }

    // ============================================================================
    // HELPER METHODS
    // ============================================================================

    private static bool HasInterval(TaskEntity task)
    {
        return (task.YearsValue ?? 0) > 0 ||
               (task.MonthsValue ?? 0) > 0 ||
               (task.WeeksValue ?? 0) > 0 ||
               (task.DaysValue ?? 0) > 0;
    }

    private static DateOnly CalculateNextDueDate(DateOnly completionDate, TaskEntity task)
    {
        var nextDate = completionDate;

        if (task.YearsValue.HasValue && task.YearsValue.Value > 0)
            nextDate = nextDate.AddYears(task.YearsValue.Value);

        if (task.MonthsValue.HasValue && task.MonthsValue.Value > 0)
            nextDate = nextDate.AddMonths(task.MonthsValue.Value);

        if (task.WeeksValue.HasValue && task.WeeksValue.Value > 0)
            nextDate = nextDate.AddDays(task.WeeksValue.Value * 7);

        if (task.DaysValue.HasValue && task.DaysValue.Value > 0)
            nextDate = nextDate.AddDays(task.DaysValue.Value);

        return nextDate;
    }

    private static string CalculateUrgencyStatus(DateOnly dueDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysDiff = dueDate.DayNumber - today.DayNumber;

        return daysDiff switch
        {
            < 0 => DatabaseConstants.UrgencyStatuses.Overdue,
            0 => DatabaseConstants.UrgencyStatuses.Today,
            <= 7 => DatabaseConstants.UrgencyStatuses.ThisWeek,
            <= 30 => DatabaseConstants.UrgencyStatuses.ThisMonth,
            _ => DatabaseConstants.UrgencyStatuses.Upcoming
        };
    }

    private static int CalculatePriorityScore(DateOnly dueDate, string priority)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysDiff = dueDate.DayNumber - today.DayNumber;

        var priorityValue = priority switch
        {
            DatabaseConstants.PriorityLevels.High => 3,
            DatabaseConstants.PriorityLevels.Medium => 2,
            DatabaseConstants.PriorityLevels.Low => 1,
            _ => 1
        };

        return daysDiff switch
        {
            < 0 => 1000 + priorityValue,  // Overdue
            0 => 500 + priorityValue,     // Today
            <= 7 => 100 + priorityValue,  // This week
            _ => priorityValue             // Future
        };
    }
}
