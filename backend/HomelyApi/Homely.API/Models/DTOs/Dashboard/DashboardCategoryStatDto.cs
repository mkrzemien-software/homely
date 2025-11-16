namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard category statistics data transfer object
/// Statistics for a single category
/// </summary>
public class DashboardCategoryStatDto
{
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Number of tasks in this category
    /// </summary>
    public int Count { get; set; }
}
