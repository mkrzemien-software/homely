namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard events statistics data transfer object
/// Statistics about events (pending, overdue, completed)
/// </summary>
public class DashboardEventsStatisticsDto
{
    /// <summary>
    /// Number of pending events
    /// </summary>
    public int Pending { get; set; }

    /// <summary>
    /// Number of overdue events
    /// </summary>
    public int Overdue { get; set; }

    /// <summary>
    /// Number of events completed this month
    /// </summary>
    public int CompletedThisMonth { get; set; }
}
