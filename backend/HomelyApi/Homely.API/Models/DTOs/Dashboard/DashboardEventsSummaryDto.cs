namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard events summary data transfer object
/// Statistics about upcoming events
/// </summary>
public class DashboardEventsSummaryDto
{
    /// <summary>
    /// Number of overdue events
    /// </summary>
    public int Overdue { get; set; }

    /// <summary>
    /// Number of events due today
    /// </summary>
    public int Today { get; set; }

    /// <summary>
    /// Number of events due this week
    /// </summary>
    public int ThisWeek { get; set; }
}
