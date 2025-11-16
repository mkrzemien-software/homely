namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard plan usage data transfer object
/// Statistics about current plan usage limits
/// </summary>
public class DashboardPlanUsageDto
{
    /// <summary>
    /// Number of tasks used
    /// </summary>
    public int TasksUsed { get; set; }

    /// <summary>
    /// Maximum tasks allowed by plan
    /// </summary>
    public int TasksLimit { get; set; }

    /// <summary>
    /// Number of household members
    /// </summary>
    public int MembersUsed { get; set; }

    /// <summary>
    /// Maximum members allowed by plan
    /// </summary>
    public int MembersLimit { get; set; }
}
