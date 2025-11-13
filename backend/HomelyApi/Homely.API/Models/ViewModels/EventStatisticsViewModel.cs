namespace Homely.API.Models.ViewModels;

/// <summary>
/// Statistics about events for a household
/// </summary>
public class EventStatisticsViewModel
{
    /// <summary>
    /// Total number of pending events
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Number of overdue events (past due date, still pending)
    /// </summary>
    public int OverdueCount { get; set; }

    /// <summary>
    /// Number of events due today
    /// </summary>
    public int TodayCount { get; set; }

    /// <summary>
    /// Number of events due this week
    /// </summary>
    public int ThisWeekCount { get; set; }

    /// <summary>
    /// Number of events completed this month
    /// </summary>
    public int CompletedThisMonthCount { get; set; }

    /// <summary>
    /// Number of postponed events
    /// </summary>
    public int PostponedCount { get; set; }

    /// <summary>
    /// Number of cancelled events
    /// </summary>
    public int CancelledCount { get; set; }

    /// <summary>
    /// Total number of events (all statuses)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Event counts by status
    /// </summary>
    public Dictionary<string, int> CountsByStatus { get; set; } = new();

    /// <summary>
    /// Event counts by priority
    /// </summary>
    public Dictionary<string, int> CountsByPriority { get; set; } = new();
}
