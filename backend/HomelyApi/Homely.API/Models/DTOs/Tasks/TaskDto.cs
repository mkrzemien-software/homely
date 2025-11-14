namespace Homely.API.Models.DTOs.Tasks;

/// <summary>
/// Task interval configuration
/// </summary>
public class TaskIntervalDto
{
    public int Years { get; set; }
    public int Months { get; set; }
    public int Weeks { get; set; }
    public int Days { get; set; }
}

/// <summary>
/// Category reference in task
/// </summary>
public class TaskCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TaskCategoryTypeDto CategoryType { get; set; } = new();
}

/// <summary>
/// Category type reference
/// </summary>
public class TaskCategoryTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// User reference in task
/// </summary>
public class TaskUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Task template data transfer object - represents a task definition with recurrence rules
/// </summary>
public class TaskDto
{
    /// <summary>
    /// Task ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Household ID
    /// </summary>
    public string HouseholdId { get; set; } = string.Empty;

    /// <summary>
    /// Category (subcategory) details
    /// </summary>
    public TaskCategoryDto Category { get; set; } = new();

    /// <summary>
    /// Task name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Recurrence interval (null if one-time task)
    /// </summary>
    public TaskIntervalDto? Interval { get; set; }

    /// <summary>
    /// Priority level: low, medium, high
    /// </summary>
    public string Priority { get; set; } = "medium";

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether task is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// User who created the task
    /// </summary>
    public TaskUserDto CreatedBy { get; set; } = new();

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public string? UpdatedAt { get; set; }
}
