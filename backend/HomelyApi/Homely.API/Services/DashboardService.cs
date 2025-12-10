using Homely.API.Models.DTOs.Dashboard;
using Homely.API.Repositories.Base;
using Homely.API.Entities;
using Homely.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Homely.API.Services;

/// <summary>
/// Service implementation for dashboard data operations
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HomelyDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUnitOfWork unitOfWork,
        HomelyDbContext context,
        ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DashboardUpcomingEventsResponseDto> GetUpcomingEventsAsync(
        Guid householdId,
        int days = 7,
        DateOnly? startDate = null,
        bool includeCompleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard events for household {HouseholdId}, days: {Days}, startDate: {StartDate}, includeCompleted: {IncludeCompleted}",
                householdId, days, startDate, includeCompleted);

            var today = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = today.AddDays(days);
            
            _logger.LogInformation("Using date range: {StartDate} to {EndDate} (exclusive)", today, endDate);

            // Build status filter
            var statusFilter = includeCompleted
                ? new[] { "pending", "completed", "postponed" }
                : new[] { "pending", "postponed" };

            // Get events with all necessary navigation properties
            var events = await _context.Set<EventEntity>()
                .Where(e => e.HouseholdId == householdId
                    && e.DeletedAt == null
                    && statusFilter.Contains(e.Status)
                    && e.DueDate >= today
                    && e.DueDate < endDate)
                .Include(e => e.Task)
                    .ThenInclude(t => t!.Category)
                        .ThenInclude(c => c!.CategoryType)
                .Include(e => e.AssignedToUser)
                .OrderBy(e => e.DueDate)
                    .ThenBy(e => e.Status == "completed" ? 1 : 0) // Show pending/postponed before completed
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} events for household {HouseholdId}",
                events.Count, householdId);

            // Calculate summary statistics from entities (before mapping to DTOs)
            // Only count pending/postponed events (not completed) for summary
            // Use actual current date for overdue/today calculation (not the query startDate)
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var weekEnd = currentDate.AddDays(7);
            var pendingEvents = events.Where(e => e.Status == "pending" || e.Status == "postponed").ToList();

            var summary = new DashboardEventsSummaryDto
            {
                Overdue = pendingEvents.Count(e => e.DueDate < currentDate),
                Today = pendingEvents.Count(e => e.DueDate == currentDate),
                ThisWeek = pendingEvents.Count(e => e.DueDate >= currentDate && e.DueDate < weekEnd)
            };

            // Map to dashboard DTOs
            // Use currentDate for urgency calculation, not the query startDate
            var dashboardEvents = events
                .Select(e => MapToDashboardEventDto(e, currentDate))
                .ToList();

            _logger.LogInformation("Dashboard summary - Overdue: {Overdue}, Today: {Today}, ThisWeek: {ThisWeek}",
                summary.Overdue, summary.Today, summary.ThisWeek);

            return new DashboardUpcomingEventsResponseDto
            {
                Data = dashboardEvents,
                Summary = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard upcoming events for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<DashboardStatisticsResponseDto> GetStatisticsAsync(
        Guid householdId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard statistics for household {HouseholdId}", householdId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

            // Get events statistics
            var allEvents = await _context.Set<EventEntity>()
                .Where(e => e.HouseholdId == householdId && e.DeletedAt == null)
                .ToListAsync(cancellationToken);

            var pendingEvents = allEvents.Where(e => e.Status == "pending").ToList();
            var overdueEvents = pendingEvents.Where(e => e.DueDate < today).Count();
            var completedThisMonth = allEvents
                .Where(e => e.Status == "completed" && e.CompletionDate.HasValue &&
                       e.CompletionDate.Value >= firstDayOfMonth)
                .Count();

            // Get tasks statistics
            var tasks = await _context.Set<TaskEntity>()
                .Where(t => t.HouseholdId == householdId && t.DeletedAt == null)
                .Include(t => t.Category)
                .ToListAsync(cancellationToken);

            var tasksByCategory = tasks
                .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                .Select(g => new DashboardCategoryStatDto
                {
                    CategoryName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Get household and plan limits
            var household = await _context.Set<HouseholdEntity>()
                .Include(h => h.PlanType)
                .FirstOrDefaultAsync(h => h.Id == householdId && h.DeletedAt == null, cancellationToken);

            var membersCount = await _context.Set<HouseholdMemberEntity>()
                .Where(hm => hm.HouseholdId == householdId && hm.DeletedAt == null)
                .CountAsync(cancellationToken);

            var tasksLimit = household?.PlanType?.MaxTasks ?? 5; // Default free plan limit
            var membersLimit = household?.PlanType?.MaxHouseholdMembers ?? 3; // Default free plan limit

            var statistics = new DashboardStatisticsResponseDto
            {
                Events = new DashboardEventsStatisticsDto
                {
                    Pending = pendingEvents.Count,
                    Overdue = overdueEvents,
                    CompletedThisMonth = completedThisMonth
                },
                Tasks = new DashboardTasksStatisticsDto
                {
                    Total = tasks.Count,
                    ByCategory = tasksByCategory
                },
                PlanUsage = new DashboardPlanUsageDto
                {
                    TasksUsed = tasks.Count,
                    TasksLimit = tasksLimit,
                    MembersUsed = membersCount,
                    MembersLimit = membersLimit
                }
            };

            _logger.LogInformation(
                "Dashboard statistics - Pending: {Pending}, Overdue: {Overdue}, Completed: {Completed}, Tasks: {Tasks}, Members: {Members}",
                statistics.Events.Pending, statistics.Events.Overdue, statistics.Events.CompletedThisMonth,
                statistics.Tasks.Total, statistics.PlanUsage.MembersUsed);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics for household {HouseholdId}", householdId);
            throw;
        }
    }

    /// <summary>
    /// Maps EventEntity to DashboardEventDto with all nested data
    /// </summary>
    private static DashboardEventDto MapToDashboardEventDto(EventEntity entity, DateOnly today)
    {
        // Calculate urgency status
        string urgencyStatus;
        if (entity.DueDate < today)
        {
            urgencyStatus = "overdue";
        }
        else if (entity.DueDate == today)
        {
            urgencyStatus = "today";
        }
        else
        {
            urgencyStatus = "upcoming";
        }

        return new DashboardEventDto
        {
            Id = entity.Id,
            DueDate = entity.DueDate.ToString("yyyy-MM-dd") + "T00:00:00Z", // ISO 8601 format
            UrgencyStatus = urgencyStatus,
            Task = new DashboardTaskInfo
            {
                Name = entity.Task?.Name ?? "Unnamed Task",
                Category = new DashboardCategoryInfo
                {
                    Name = entity.Task?.Category?.Name ?? "Uncategorized",
                    CategoryType = new DashboardCategoryTypeInfo
                    {
                        Name = entity.Task?.Category?.CategoryType?.Name ?? "General"
                    }
                }
            },
            AssignedTo = new DashboardAssignedUserInfo
            {
                FirstName = entity.AssignedToUser?.FirstName ?? "Unassigned",
                LastName = entity.AssignedToUser?.LastName ?? ""
            },
            Priority = entity.Priority,
            Status = entity.Status,
            CompletionDate = entity.CompletionDate.HasValue
                ? entity.CompletionDate.Value.ToString("yyyy-MM-dd") + "T00:00:00Z"
                : null
        };
    }
}
