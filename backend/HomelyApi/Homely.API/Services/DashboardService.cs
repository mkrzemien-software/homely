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
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching dashboard upcoming events for household {HouseholdId}, days: {Days}",
                householdId, days);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

            // Get events with all necessary navigation properties
            var events = await _context.Set<EventEntity>()
                .Where(e => e.HouseholdId == householdId
                    && e.DeletedAt == null
                    && e.Status == "pending"
                    && e.DueDate <= endDate)
                .Include(e => e.Task)
                    .ThenInclude(t => t!.Category)
                        .ThenInclude(c => c!.CategoryType)
                .Include(e => e.AssignedToUser)
                .OrderBy(e => e.DueDate)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} events for household {HouseholdId}",
                events.Count, householdId);

            // Calculate summary statistics from entities (before mapping to DTOs)
            var weekEnd = today.AddDays(7);
            var summary = new DashboardEventsSummaryDto
            {
                Overdue = events.Count(e => e.DueDate < today),
                Today = events.Count(e => e.DueDate == today),
                ThisWeek = events.Count(e => e.DueDate >= today && e.DueDate <= weekEnd)
            };

            // Map to dashboard DTOs
            var dashboardEvents = events
                .Select(e => MapToDashboardEventDto(e, today))
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
            Status = entity.Status
        };
    }
}
