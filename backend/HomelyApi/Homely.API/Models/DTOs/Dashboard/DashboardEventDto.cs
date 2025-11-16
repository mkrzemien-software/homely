namespace Homely.API.Models.DTOs.Dashboard;

/// <summary>
/// Dashboard event data transfer object
/// Simplified event model for dashboard display with all required nested data
/// </summary>
public class DashboardEventDto
{
    /// <summary>
    /// Event ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Due date (ISO 8601 format)
    /// </summary>
    public string DueDate { get; set; } = string.Empty;

    /// <summary>
    /// Urgency status (calculated by backend)
    /// </summary>
    public string UrgencyStatus { get; set; } = string.Empty;

    /// <summary>
    /// Task information
    /// </summary>
    public DashboardTaskInfo Task { get; set; } = new();

    /// <summary>
    /// Assigned user information
    /// </summary>
    public DashboardAssignedUserInfo AssignedTo { get; set; } = new();

    /// <summary>
    /// Event priority
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Event status
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Task information for dashboard event
/// </summary>
public class DashboardTaskInfo
{
    /// <summary>
    /// Task name (displayed as event title)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category information
    /// </summary>
    public DashboardCategoryInfo Category { get; set; } = new();
}

/// <summary>
/// Category information for dashboard event
/// </summary>
public class DashboardCategoryInfo
{
    /// <summary>
    /// Category name (subcategory)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category type information
    /// </summary>
    public DashboardCategoryTypeInfo CategoryType { get; set; } = new();
}

/// <summary>
/// Category type information for dashboard event
/// </summary>
public class DashboardCategoryTypeInfo
{
    /// <summary>
    /// Category type name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Assigned user information for dashboard event
/// </summary>
public class DashboardAssignedUserInfo
{
    /// <summary>
    /// User first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
