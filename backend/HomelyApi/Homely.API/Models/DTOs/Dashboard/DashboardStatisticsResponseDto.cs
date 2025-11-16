namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard statistics response data transfer object
/// Complete statistics for household dashboard
/// </summary>
public class DashboardStatisticsResponseDto
{
    /// <summary>
    /// Tasks statistics
    /// </summary>
    public DashboardTasksStatisticsDto Tasks { get; set; } = new();

    /// <summary>
    /// Events statistics
    /// </summary>
    public DashboardEventsStatisticsDto Events { get; set; } = new();

    /// <summary>
    /// Plan usage statistics
    /// </summary>
    public DashboardPlanUsageDto PlanUsage { get; set; } = new();
}
