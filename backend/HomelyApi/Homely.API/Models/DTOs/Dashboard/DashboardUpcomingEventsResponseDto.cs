namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard upcoming events response data transfer object
/// Response from GET /api/dashboard/upcoming-events
/// </summary>
public class DashboardUpcomingEventsResponseDto
{
    /// <summary>
    /// Array of upcoming events
    /// </summary>
    public List<DashboardEventDto> Data { get; set; } = new();

    /// <summary>
    /// Summary statistics
    /// </summary>
    public DashboardEventsSummaryDto Summary { get; set; } = new();
}
