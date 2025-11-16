namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard tasks statistics data transfer object
/// Statistics about tasks grouped by categories
/// </summary>
public class DashboardTasksStatisticsDto
{
    /// <summary>
    /// Total number of tasks
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Tasks grouped by category
    /// </summary>
    public List<DashboardCategoryStatDto> ByCategory { get; set; } = new();
}
