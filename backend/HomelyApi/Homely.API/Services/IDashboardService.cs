using Homely.API.Models.DTOs.Dashboard;

namespace Homely.API.Services;

/// <summary>
/// Service interface for dashboard data operations
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get upcoming events for dashboard with full nested data
    /// Includes task, category, and assigned user information
    /// Calculates urgency status and provides summary statistics
    /// </summary>
    /// <param name="householdId">Household ID</param>
    /// <param name="days">Number of days to look ahead (default: 7)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard upcoming events response with data and summary</returns>
    Task<DashboardUpcomingEventsResponseDto> GetUpcomingEventsAsync(
        Guid householdId,
        int days = 7,
        CancellationToken cancellationToken = default);
}
